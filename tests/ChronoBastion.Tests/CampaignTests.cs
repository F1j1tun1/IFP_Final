using ChronoBastion.Application;
using ChronoBastion.Core;
using ChronoBastion.Core.Content;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;
using ChronoBastion.Core.Rules;
using ChronoBastion.Simulator;

namespace ChronoBastion.Tests;

public sealed class CampaignTests
{
    [Fact]
    public void Campaign_has_three_distinct_maps_and_all_fourteen_enemies()
    {
        Assert.Equal(new[] { 12, 12, 15 }, Campaign.Chapters.Select(c => c.Waves));
        Assert.Equal(3, Campaign.Chapters.Select(c => c.Map.Path[0]).Distinct().Count());
        Assert.All(Campaign.Chapters, c =>
        {
            Assert.Equal(10, c.Map.Pads.Length);
            Assert.Equal(c.Map.Path[^1], c.Map.PositionAt(double.MaxValue));
            Assert.Throws<ArgumentOutOfRangeException>(() => c.Map.PositionAt(double.NaN));
        });
        var kinds = Enumerable.Range(1, 39).SelectMany(number => WavePlanner.Schedule(number)).Select(s => s.Kind).Distinct();
        Assert.Equal(14, kinds.Count());
    }

    [Theory]
    [InlineData(1, 12)]
    [InlineData(2, 24)]
    public void Region_transition_preserves_health_and_credits_refund_exactly_once(int stage, int wave)
    {
        var state = TestGame.Apply(new GameState { Gold = 500 }, new GameCommand.BuildTower(2, TowerKind.Pulse));
        state = state with
        {
            StageNumber = stage,
            WaveNumber = wave,
            WavesCleared = wave - 1,
            Phase = GamePhase.Combat,
            CoreHealth = 9,
            Score = 400
        };
        var completed = GameEngine.Tick(state).State;
        Assert.Equal(GamePhase.StageCleared, completed.Phase);
        Assert.False(GameEngine.Execute(completed, new GameCommand.StartWave()).IsSuccess);
        var next = TestGame.Apply(completed, new GameCommand.NextStage());
        Assert.Equal(stage + 1, next.StageNumber);
        Assert.Equal(0, next.WaveInStage);
        Assert.Equal(9, next.CoreHealth);
        Assert.Equal(completed.Score, next.Score);
        Assert.Equal(completed.Gold + 63 + 150, next.Gold);
        Assert.Empty(next.Towers);
        Assert.False(GameEngine.Execute(next, new GameCommand.NextStage()).IsSuccess);
        Assert.True(SavedRuns.Parse(SavedRuns.Serialize(completed)).IsSuccess);
        Assert.True(SavedRuns.Parse(SavedRuns.Serialize(next)).IsSuccess);
    }

    [Fact]
    public void Combat_uses_current_map_for_targeting()
    {
        var state = new GameState
        {
            StageNumber = 2,
            WaveNumber = 13,
            WavesCleared = 12,
            Phase = GamePhase.Combat,
            Enemies = [EnemyCatalog.Spawn(EnemyKind.Skeleton, 1, 13) with { Distance = 120 }],
            Schedule = [new(10000, EnemyKind.Skeleton)]
        };
        state = TestGame.Apply(state, new GameCommand.BuildTower(1, TowerKind.Pulse));
        Assert.Single(GameEngine.Tick(state).Events.OfType<GameEvent.ShotFired>());
    }

    [Fact]
    public void Full_campaign_can_be_won_with_ordinary_resources_and_restart_is_fresh()
    {
        var run = DemoScenarios.Run(true);
        Assert.Equal(GamePhase.Won, run.FinalState.Phase);
        Assert.Equal(39, run.FinalState.WavesCleared);
        Assert.Equal(39, run.Waves.Length);
        Assert.Equal(2, run.Waves.Count(w => w.Phase == GamePhase.StageCleared));
        Assert.Equal(SavedRuns.Serialize(GameEngine.NewGame()), SavedRuns.Serialize(TestGame.Apply(run.FinalState, new GameCommand.Restart())));
    }
}
