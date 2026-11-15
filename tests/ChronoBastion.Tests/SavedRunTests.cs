using System.Collections.Immutable;
using ChronoBastion.Application;
using ChronoBastion.Core;
using ChronoBastion.Core.Content;
using ChronoBastion.Core.Model;
using ChronoBastion.Core.Functional;

namespace ChronoBastion.Tests;

public sealed class SavedRunTests
{
    private static GameState Battle()
    {
        var state = TestGame.Apply(GameEngine.NewGame(), new GameCommand.BuildTower(2, TowerKind.Pulse));
        state = TestGame.Apply(state, new GameCommand.StartWave());
        for (var i = 0; i < 90; i++) state = GameEngine.Tick(state).State;
        return state;
    }
    [Fact]
    public void Restored_battle_is_paused_and_continues_deterministically()
    {
        var state = Battle();
        var session = new GameSession();
        Assert.True(session.Restore(SavedRuns.Serialize(state)).IsSuccess);
        Assert.True(session.State.IsPaused);
        session.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(state.WaveTick, session.State.WaveTick);
        session.Dispatch(new GameCommand.SetPaused(false));
        for (var i = 0; i < 100; i++) { state = GameEngine.Tick(state).State; session.Advance(TimeSpan.FromMilliseconds(50)); }
        Assert.Equal(SavedRuns.Serialize(state), SavedRuns.Serialize(session.State));
    }
    [Fact]
    public void Every_real_battle_tick_snapshot_round_trips()
    {
        var state = Battle();
        for (var i = 0; i < 800; i++)
        {
            Assert.True(SavedRuns.Parse(SavedRuns.Serialize(state)).IsSuccess, $"Tick {state.WaveTick}");
            state = GameEngine.Tick(state).State;
        }
    }
    [Fact]
    public void Invalid_state_cannot_replace_the_session()
    {
        var session = new GameSession(); var before = session.State;
        GameState[] bad = [Battle() with {StageNumber=4},Battle() with {Gold=-1},Battle() with {CoreHealth=21},
            Battle() with {Phase=GamePhase.Lost},Battle() with {Schedule=[]},Battle() with {NextSpawnIndex=99},
            Battle() with {WavesCleared=39},Battle() with {SpeedMultiplier=100},
            Battle() with {Enemies=[new(1,EnemyKind.Grunt,999,999,0)]},
            Battle() with {Towers=[new(2,TowerKind.Pulse,1,1,0,TargetPriority.First)]}];
        foreach (var invalid in bad) Assert.False(session.Restore(SavedRuns.Serialize(invalid)).IsSuccess);
        Assert.Same(before, session.State);
    }
    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"Version\":1,\"State\":{}}")]
    [InlineData("broken")]
    [InlineData("{\"Version\":99,\"State\":null}")]
    public void Malformed_saves_are_nonfatal(string json) => Assert.False(SavedRuns.Parse(json).IsSuccess);
    [Fact]
    public void Oversized_saves_are_rejected() => Assert.False(SavedRuns.Parse(new string('x', 131073)).IsSuccess);

    [Fact]
    public void Missing_or_invalid_timers_and_previous_versions_are_rejected()
    {
        var json = SavedRuns.Serialize(Battle());
        Assert.False(SavedRuns.Parse(json.Replace("\"AbilityTicks\":0", "\"OldTimer\":0")).IsSuccess);
        Assert.False(SavedRuns.Parse(json.Replace("\"AbilityTicks\":0", "\"AbilityTicks\":120")).IsSuccess);
        Assert.False(SavedRuns.Parse(json.Replace("\"Version\":5", "\"Version\":1")).IsSuccess);
        Assert.False(SavedRuns.Parse(json.Replace("\"AutoWaveTicksRemaining\":0", "\"AutoWaveTicksRemaining\":100")).IsSuccess);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void Old_campaigns_require_a_new_journey_without_loading_old_wealth(int version)
    {
        var json = SavedRuns.Serialize(Battle() with { Gold = 9000 })
            .Replace("\"Version\":5", $"\"Version\":{version}");
        var session = new GameSession();
        var error = Assert.IsType<Result<GameTransition, string>.Failure>(session.Restore(json));
        Assert.Contains("region budgets have changed", error.Error);
        Assert.Equal(0, session.State.WaveNumber);
        Assert.Equal(GameRules.StartingGold, session.State.Gold);
        Assert.Empty(session.State.Towers);
    }

    [Theory]
    [InlineData(1, 7)]
    [InlineData(2, 8)]
    [InlineData(3, 8)]
    public void Previous_layout_keeps_tower_upgrades_and_refunds_removed_sites_once(int stage, int count)
    {
        var towers = Enumerable.Range(1, 10).Select(id =>
        {
            var kind = (TowerKind)((id - 1) % 3);
            return new Tower(id, kind, 3, TowerCatalog.Get(kind).Levels.Sum(l => l.Price), 0, TargetPriority.First);
        }).ToImmutableArray();
        var previous = new GameState
        {
            StageNumber = stage, WaveNumber = (stage - 1) * 12, WavesCleared = (stage - 1) * 12,
            Gold = 123, CoreHealth = 11, Score = 456, Towers = towers, AutoWaves = true, SpeedMultiplier = 2
        };
        var json = SavedRuns.Serialize(previous).Replace("\"Version\":5", "\"Version\":4");
        var current = Assert.IsType<Result<GameState, string>.Success>(SavedRuns.Parse(json)).Value;
        Assert.Equal(count, current.Towers.Length);
        Assert.Equal(Enumerable.Range(1, count), current.Towers.Select(t => t.PadId));
        Assert.Equal(previous.Gold + towers.Sum(t => t.InvestedGold), current.Gold + current.Towers.Sum(t => t.InvestedGold));
        Assert.Equal(towers[stage == 2 ? 0 : 2].Kind, current.Towers[0].Kind);
        Assert.Equal(previous with { Towers = current.Towers, Gold = current.Gold }, current);
        Assert.All(current.Towers, tower => Assert.Equal(3, tower.Level));
        var reloaded = Assert.IsType<Result<GameState, string>.Success>(SavedRuns.Parse(SavedRuns.Serialize(current))).Value;
        Assert.Equal(SavedRuns.Serialize(current), SavedRuns.Serialize(reloaded));
    }

    [Theory]
    [InlineData(1, 8)]
    [InlineData(2, 9)]
    [InlineData(3, 9)]
    public void Current_saves_cannot_place_towers_outside_the_active_map(int stage, int pad)
    {
        var state = new GameState
        {
            StageNumber = stage, WaveNumber = (stage - 1) * 12, WavesCleared = (stage - 1) * 12,
            Towers = [new(pad, TowerKind.Pulse, 1, 90, 0, TargetPriority.First)]
        };
        Assert.False(SavedRuns.Parse(SavedRuns.Serialize(state)).IsSuccess);
    }

    [Fact]
    public void An_invalid_removed_tower_cannot_create_a_refund()
    {
        var state = new GameState { Towers = [new(1, TowerKind.Pulse, 1, 9999, 0, TargetPriority.First)] };
        var json = SavedRuns.Serialize(state).Replace("\"Version\":5", "\"Version\":4");
        Assert.False(SavedRuns.Parse(json).IsSuccess);
    }
}
