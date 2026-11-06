using System.Text.Json;
using ChronoBastion.Application;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Tests;

public sealed class SessionTests
{
    [Fact]
    public void Real_time_is_accumulated_into_fixed_ticks()
    {
        var session = Start();
        session.Advance(TimeSpan.FromMilliseconds(20));
        Assert.Equal(0, session.State.WaveTick);
        session.Advance(TimeSpan.FromMilliseconds(30));
        Assert.Equal(1, session.State.WaveTick);
        Assert.Single(session.State.Enemies);
    }

    [Fact]
    public void Double_speed_runs_twice_as_many_ticks_without_changing_rules()
    {
        var normal = Start();
        var fast = Start();
        fast.Dispatch(new GameCommand.SetSpeed(2));
        normal.Advance(TimeSpan.FromMilliseconds(100));
        fast.Advance(TimeSpan.FromMilliseconds(100));
        Assert.Equal(2, normal.State.WaveTick);
        Assert.Equal(4, fast.State.WaveTick);
        normal.Advance(TimeSpan.FromMilliseconds(100));
        Assert.Equal(JsonSerializer.Serialize(normal.State), JsonSerializer.Serialize(fast.State with { SpeedMultiplier = 1 }));
    }

    [Fact]
    public void Paused_time_and_partial_ticks_do_not_carry_over_to_resume()
    {
        var session = Start();
        session.Advance(TimeSpan.FromMilliseconds(40));
        session.Dispatch(new GameCommand.SetPaused(true));
        var paused = session.State;
        session.Advance(TimeSpan.FromHours(1));
        Assert.Same(paused, session.State);
        session.Dispatch(new GameCommand.SetPaused(false));
        session.Advance(TimeSpan.FromMilliseconds(10));
        Assert.Equal(0, session.State.WaveTick);
    }

    [Fact]
    public void Background_gaps_are_bounded()
    {
        var session = Start();
        session.Advance(TimeSpan.FromHours(1));
        Assert.Equal(5, session.State.WaveTick);
        session.Dispatch(new GameCommand.SetSpeed(2));
        session.Advance(TimeSpan.FromHours(1));
        Assert.Equal(15, session.State.WaveTick);
    }

    [Fact]
    public void A_failed_command_does_not_notify_or_change_state()
    {
        var session = Start();
        var original = session.State;
        var updates = 0;
        session.Updated += _ => updates++;
        var result = session.Dispatch(new GameCommand.StartWave());
        Assert.False(result.IsSuccess);
        Assert.Same(original, session.State);
        Assert.Equal(0, updates);
    }

    [Fact]
    public void Batched_ticks_emit_one_update_with_all_events()
    {
        var session = Start();
        var updates = new List<GameTransition>();
        session.Updated += updates.Add;
        session.Advance(TimeSpan.FromMilliseconds(250));
        var update = Assert.Single(updates);
        Assert.Equal(5, update.State.WaveTick);
        Assert.Single(update.Events.OfType<GameEvent.EnemySpawned>());
    }

    [Fact]
    public void Restart_discards_the_old_battle_and_timing_remainder()
    {
        var session = Start();
        session.Dispatch(new GameCommand.BuildTower(1, TowerKind.Pulse));
        session.Advance(TimeSpan.FromMilliseconds(90));
        session.Dispatch(new GameCommand.Restart());
        Assert.Empty(session.State.Towers);
        Assert.Empty(session.State.Enemies);
        Assert.Equal(300, session.State.Gold);
        session.Dispatch(new GameCommand.StartWave());
        session.Advance(TimeSpan.FromMilliseconds(10));
        Assert.Equal(0, session.State.WaveTick);
    }

    [Fact]
    public void Negative_elapsed_time_is_a_programming_error() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Start().Advance(TimeSpan.FromSeconds(-1)));

    private static GameSession Start()
    {
        var session = new GameSession();
        session.Dispatch(new GameCommand.StartWave());
        return session;
    }
}
