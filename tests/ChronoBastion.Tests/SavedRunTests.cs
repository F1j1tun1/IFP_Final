using ChronoBastion.Application;
using ChronoBastion.Core;
using ChronoBastion.Core.Content;
using ChronoBastion.Core.Model;

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
        Assert.False(SavedRuns.Parse(json.Replace("\"Version\":2", "\"Version\":1")).IsSuccess);
        Assert.False(SavedRuns.Parse(json.Replace("\"AutoWaveTicksRemaining\":0", "\"AutoWaveTicksRemaining\":100")).IsSuccess);
    }
}
