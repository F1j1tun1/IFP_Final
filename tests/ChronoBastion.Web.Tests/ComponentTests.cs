using Bunit;
using ChronoBastion.Application;
using ChronoBastion.Core.Model;
using ChronoBastion.Core;
using ChronoBastion.Core.Content;
using ChronoBastion.Web.Components;

namespace ChronoBastion.Web.Tests;

public sealed class ComponentTests : BunitContext
{
    [Theory]
    [InlineData(1, "forest")]
    [InlineData(2, "village")]
    [InlineData(3, "keep")]
    public void GateHealthUpdatesWhenAnEnemyEscapes(int stage, string art)
    {
        var state = new GameState { StageNumber = stage, WaveNumber = (stage - 1) * 12 + 1,
            Phase = GamePhase.Combat, Enemies = [EnemyCatalog.Spawn(EnemyKind.Grunt, 1, 1) with { Distance = Campaign.Get(stage).Map.Length - .01 }],
            Schedule = [new(10000, EnemyKind.Grunt)] };
        var cut = Render<GameBoard>(p => p.Add(c => c.State, state));
        Assert.Equal($"art/maps/{art}.png", cut.Find(".map-scenery").GetAttribute("href"));
        Assert.Equal("Gate health: 20 of 20", cut.Find(".gate-health").GetAttribute("aria-label"));
        var damaged = GameEngine.Tick(state).State;
        cut.Render(p => p.Add(c => c.State, damaged));
        Assert.Equal(19, damaged.CoreHealth);
        Assert.Equal("Gate health: 19 of 20", cut.Find(".gate-health").GetAttribute("aria-label"));
        Assert.Equal("width:95%", cut.Find(".hp-fill").GetAttribute("style"));
    }

    [Fact]
    public void PausedCountdownNamesItsActualResumeAction()
    {
        var state = new GameState { AutoWaves = true, AutoWaveTicksRemaining = 40, IsPaused = true };
        var cut = Render<GameBoard>(p => p.Add(c => c.State, state));
        Assert.Contains("COUNTDOWN PAUSED", cut.Find(".pause-label").TextContent);
        Assert.Contains("Resume countdown", cut.Find(".pause-label").TextContent);
    }

    [Fact]
    public void BoardRendersEnemiesAndPadsWithOverlappingIds()
    {
        var session = new GameSession();
        var cut = Render<GameBoard>(p => p.Add(c => c.State, session.State));
        session.Dispatch(new GameCommand.StartWave());
        session.Advance(TimeSpan.FromMilliseconds(100));
        cut.Render(p => p.Add(c => c.State, session.State));
        Assert.Single(cut.FindAll(".enemy"));
        Assert.Equal(10, cut.FindAll(".pad").Count);
    }
    [Fact]
    public void HudRendersChangedSessionValues()
    {
        var session = new GameSession();
        var cut = Render<GameHud>(p => p.Add(c => c.State, session.State));
        Assert.Equal("300", cut.Find("[data-testid=gold]").TextContent);
        session.Dispatch(new GameCommand.BuildTower(2, TowerKind.Pulse));
        cut.Render(p => p.Add(c => c.State, session.State));
        Assert.Equal("210", cut.Find("[data-testid=gold]").TextContent);
    }

    [Fact]
    public void BoardReportsPadSelectionWithoutChangingDomain()
    {
        var selected = 0;
        var session = new GameSession();
        var cut = Render<GameBoard>(p => p.Add(c => c.State, session.State).Add(c => c.OnSelect, pad => selected = pad));
        cut.Find("[aria-label='Pad 2: empty']").Click();
        Assert.Equal(2, selected);
        Assert.Empty(session.State.Towers);
        Assert.Equal(10, cut.FindAll("[role=button]").Count);
    }

    [Fact]
    public void BuildCardEmitsCommandAndDisplaysFailure()
    {
        var session = new GameSession();
        session.Dispatch(new GameCommand.BuildTower(1, TowerKind.Mortar));
        session.Dispatch(new GameCommand.BuildTower(2, TowerKind.Mortar));
        GameCommand? received = null;
        var cut = Render<TowerPanel>(p => p.Add(c => c.State, session.State).Add(c => c.SelectedPad, 3)
            .Add(c => c.OnCommand, command => received = command));
        cut.Find(".build-card.pulse").Click();
        Assert.Equal(new GameCommand.BuildTower(3, TowerKind.Pulse), received);
        var message = session.Dispatch(received!).Match(_ => "", e => e.Message);
        cut.Render(p => p.Add(c => c.Error, message));
        Assert.Contains("Not enough gold", cut.Find("[role=alert]").TextContent);
        Assert.Equal(20, session.State.Gold);
    }

    [Fact]
    public void OccupiedPadCannotChargeTwice()
    {
        var session = new GameSession();
        session.Dispatch(new GameCommand.BuildTower(2, TowerKind.Pulse));
        var error = session.Dispatch(new GameCommand.BuildTower(2, TowerKind.Pulse)).Match(_ => "", e => e.Message);
        var cut = Render<TowerPanel>(p => p.Add(c => c.State, session.State).Add(c => c.SelectedPad, 2).Add(c => c.Error, error));
        Assert.Contains("already has a tower", cut.Find("[role=alert]").TextContent);
        Assert.Empty(cut.FindAll(".build-card"));
        Assert.Equal(210, session.State.Gold);
    }

    [Fact]
    public void TowerPanelEmitsUpgradeSaleAndTargetCommands()
    {
        var session = new GameSession();
        session.Dispatch(new GameCommand.BuildTower(2, TowerKind.Pulse));
        var commands = new List<GameCommand>();
        var cut = Render<TowerPanel>(p => p.Add(c => c.State, session.State).Add(c => c.SelectedPad, 2).Add(c => c.OnCommand, commands.Add));
        cut.Find("select").Change("Strongest");
        cut.FindAll("button").Single(b => b.TextContent.Contains("Upgrade")).Click();
        cut.FindAll("button").Single(b => b.TextContent.Contains("Sell")).Click();
        Assert.Equal(new GameCommand.SetTargetPriority(2, TargetPriority.Strongest), commands[0]);
        Assert.Equal(new GameCommand.UpgradeTower(2), commands[1]);
        Assert.Equal(new GameCommand.SellTower(2), commands[2]);
    }

    [Fact]
    public void MaximumTowerHasNoUpgradeAction()
    {
        var session = new GameSession();
        Assert.True(session.Restore(SavedRuns.Serialize(new GameState { Gold = 1000 })).IsSuccess);
        session.Dispatch(new GameCommand.BuildTower(2, TowerKind.Pulse));
        session.Dispatch(new GameCommand.UpgradeTower(2));
        session.Dispatch(new GameCommand.UpgradeTower(2));
        var cut = Render<TowerPanel>(p => p.Add(c => c.State, session.State).Add(c => c.SelectedPad, 2));
        Assert.Contains("Max level", cut.Markup);
        Assert.DoesNotContain(cut.FindAll("button"), b => b.TextContent.Contains("Upgrade"));
    }

    [Theory]
    [InlineData(GamePhase.Won, "Victory")]
    [InlineData(GamePhase.Lost, "Defeat")]
    public void ResultOffersFreshRun(GamePhase phase, string title)
    {
        var replay = false;
        var cut = Render<ResultDialog>(p => p.Add(c => c.State, new GameState { Phase = phase }).Add(c => c.OnPlayAgain, () => replay = true));
        Assert.Equal(title, cut.Find("h2").TextContent);
        cut.FindAll("button").Single(b => b.TextContent == "Play again").Click();
        Assert.True(replay);
        Assert.DoesNotContain("Retry wave", cut.Markup);
    }
}
