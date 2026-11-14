using ChronoBastion.Core;
using ChronoBastion.Core.Content;
using ChronoBastion.Core.Model;
using ChronoBastion.Core.Rules;

namespace ChronoBastion.Tests;

public sealed class CommandTests
{
    [Fact]
    public void New_game_has_only_the_starting_resources()
    {
        var state = GameEngine.NewGame();
        Assert.Equal(GamePhase.Planning, state.Phase);
        Assert.Equal(300, state.Gold);
        Assert.Equal(20, state.CoreHealth);
        Assert.Equal(0, state.Score);
        Assert.Empty(state.Enemies);
        Assert.Empty(state.Towers);
    }

    [Theory]
    [InlineData(TowerKind.Pulse, 90)]
    [InlineData(TowerKind.Mortar, 140)]
    [InlineData(TowerKind.Frost, 100)]
    public void Building_spends_exactly_the_price_without_changing_the_input(TowerKind kind, int price)
    {
        var original = GameEngine.NewGame();
        var built = TestGame.Apply(original, new GameCommand.BuildTower(1, kind));
        Assert.Equal(300 - price, built.Gold);
        Assert.Equal(kind, Assert.Single(built.Towers).Kind);
        Assert.Equal(price, built.Towers[0].InvestedGold);
        Assert.Empty(original.Towers);
        Assert.Equal(300, original.Gold);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(11)]
    public void Buildings_require_a_real_pad(int padId)
    {
        Assert.Equal(GameErrorCode.InvalidPad,
            TestGame.Error(GameEngine.NewGame(), new GameCommand.BuildTower(padId, TowerKind.Pulse)).Code);
    }

    [Fact]
    public void Occupied_pads_and_insufficient_gold_leave_the_state_unchanged()
    {
        var state = TestGame.Apply(GameEngine.NewGame(), new GameCommand.BuildTower(1, TowerKind.Pulse));
        Assert.Equal(GameErrorCode.PadOccupied, TestGame.Error(state, new GameCommand.BuildTower(1, TowerKind.Frost)).Code);
        var broke = state with { Gold = 89 };
        Assert.Equal(GameErrorCode.InsufficientGold, TestGame.Error(broke, new GameCommand.BuildTower(2, TowerKind.Pulse)).Code);
        Assert.Equal(89, broke.Gold);
        Assert.Single(broke.Towers);
    }

    [Fact]
    public void Invalid_enum_values_are_reported_as_errors()
    {
        Assert.Equal(GameErrorCode.InvalidTowerKind,
            TestGame.Error(GameEngine.NewGame(), new GameCommand.BuildTower(1, (TowerKind)99)).Code);
        Assert.Equal(GameErrorCode.InvalidPriority,
            TestGame.Error(GameEngine.NewGame(), new GameCommand.SetTargetPriority(1, (TargetPriority)99)).Code);
    }

    [Fact]
    public void Upgrade_pipeline_validates_before_spending_gold()
    {
        var state = TestGame.Apply(GameEngine.NewGame() with { Gold = 500 }, new GameCommand.BuildTower(1, TowerKind.Pulse));
        var upgraded = TestGame.Apply(state, new GameCommand.UpgradeTower(1));
        Assert.Equal(300, upgraded.Gold);
        Assert.Equal(2, upgraded.Towers[0].Level);
        Assert.Equal(200, upgraded.Towers[0].InvestedGold);
        Assert.Equal(1, state.Towers[0].Level);

        var poor = state with { Gold = 109 };
        Assert.Equal(GameErrorCode.InsufficientGold, TestGame.Error(poor, new GameCommand.UpgradeTower(1)).Code);
        Assert.Equal(109, poor.Gold);
        Assert.Equal(1, poor.Towers[0].Level);
        Assert.Equal(GameErrorCode.TowerNotFound, TestGame.Error(state, new GameCommand.UpgradeTower(2)).Code);
    }

    [Fact]
    public void Level_three_is_the_last_upgrade()
    {
        var state = TestGame.Apply(GameEngine.NewGame() with { Gold = 1000 }, new GameCommand.BuildTower(1, TowerKind.Pulse));
        state = TestGame.Apply(TestGame.Apply(state, new GameCommand.UpgradeTower(1)), new GameCommand.UpgradeTower(1));
        Assert.Equal(GameErrorCode.MaxLevelReached, TestGame.Error(state, new GameCommand.UpgradeTower(1)).Code);
        Assert.False(GameQueries.UpgradePrice(state.Towers[0]).IsSome);
    }

    [Fact]
    public void Selling_refunds_half_of_all_investment_and_frees_the_pad()
    {
        var state = TestGame.Apply(GameEngine.NewGame(), new GameCommand.BuildTower(1, TowerKind.Pulse));
        state = TestGame.Apply(state, new GameCommand.UpgradeTower(1));
        var sold = TestGame.Apply(state, new GameCommand.SellTower(1));
        Assert.Equal(200, sold.Gold);
        Assert.Empty(sold.Towers);
        Assert.Single(TestGame.Apply(sold, new GameCommand.BuildTower(1, TowerKind.Frost)).Towers);
        Assert.Equal(GameErrorCode.TowerNotFound, TestGame.Error(sold, new GameCommand.SellTower(1)).Code);
    }

    [Fact]
    public void A_wave_can_only_be_started_once()
    {
        var combat = TestGame.Apply(GameEngine.NewGame(), new GameCommand.StartWave());
        Assert.Equal(1, combat.WaveNumber);
        Assert.Equal(GamePhase.Combat, combat.Phase);
        Assert.Equal(WavePlanner.CountEnemies(WavePlanner.Describe(1)), combat.PendingEnemies);
        Assert.Equal(GameErrorCode.InvalidPhase, TestGame.Error(combat, new GameCommand.StartWave()).Code);
    }

    [Fact]
    public void Targeting_changes_are_free_and_work_during_a_wave()
    {
        var state = TestGame.Apply(GameEngine.NewGame(), new GameCommand.StartWave());
        state = TestGame.Apply(state, new GameCommand.BuildTower(1, TowerKind.Pulse));
        var changed = TestGame.Apply(state, new GameCommand.SetTargetPriority(1, TargetPriority.Strongest));
        Assert.Equal(state.Gold, changed.Gold);
        Assert.Equal(TargetPriority.Strongest, changed.Towers[0].Priority);
        Assert.Equal(GameErrorCode.TowerNotFound, TestGame.Error(state, new GameCommand.SetTargetPriority(2, TargetPriority.First)).Code);
    }

    [Theory]
    [InlineData(GamePhase.Won)]
    [InlineData(GamePhase.Lost)]
    public void A_finished_game_only_accepts_restart(GamePhase phase)
    {
        var state = GameEngine.NewGame() with { Phase = phase, Score = 900, Gold = 5000, CoreHealth = 0, WaveNumber = 10 };
        GameCommand[] commands = [new GameCommand.StartWave(), new GameCommand.BuildTower(1, TowerKind.Pulse),
            new GameCommand.UpgradeTower(1), new GameCommand.SellTower(1), new GameCommand.SetPaused(true),
            new GameCommand.SetSpeed(2), new GameCommand.SetTargetPriority(1, TargetPriority.First)];
        foreach (var command in commands)
            Assert.Equal(GameErrorCode.GameFinished, TestGame.Error(state, command).Code);
        Assert.Equal(GameEngine.NewGame(), TestGame.Apply(state, new GameCommand.Restart()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void Only_supported_speeds_are_accepted(int speed)
    {
        Assert.Equal(GameErrorCode.InvalidSpeed, TestGame.Error(GameEngine.NewGame(), new GameCommand.SetSpeed(speed)).Code);
    }

    [Fact]
    public void Pausing_is_only_available_during_combat()
    {
        Assert.Equal(GameErrorCode.InvalidPhase, TestGame.Error(GameEngine.NewGame(), new GameCommand.SetPaused(true)).Code);
        var state = TestGame.Apply(GameEngine.NewGame(), new GameCommand.StartWave());
        Assert.True(TestGame.Apply(state, new GameCommand.SetPaused(true)).IsPaused);
    }
}
