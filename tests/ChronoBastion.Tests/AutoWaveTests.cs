using ChronoBastion.Application;
using ChronoBastion.Core;
using ChronoBastion.Core.Content;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Tests;

public sealed class AutoWaveTests
{
    private static GameState Cleared(int wave = 1, bool auto = true) => GameEngine.Tick(new()
    {
        Phase = GamePhase.Combat, StageNumber = Math.Min(3, (wave - 1) / 12 + 1),
        WaveNumber = wave, WavesCleared = wave - 1, AutoWaves = auto
    }).State;

    [Fact]
    public void Auto_waves_are_opt_in_and_do_not_start_a_new_region()
    {
        Assert.False(GameEngine.NewGame().AutoWaves);
        var state = TestGame.Apply(GameEngine.NewGame(), new GameCommand.SetAutoWaves(true));
        Assert.Same(state, GameEngine.Tick(state).State);
        Assert.False(Cleared(auto: false).HasCountdown);
        state = TestGame.Apply(Cleared(12), new GameCommand.NextStage());
        Assert.True(state.AutoWaves);
        Assert.False(state.CanAdvance);
        Assert.Same(state, GameEngine.Tick(state).State);
    }

    [Fact]
    public void Countdown_starts_exactly_one_wave_after_five_seconds()
    {
        var state = Cleared();
        for (var i = 0; i < 99; i++) state = GameEngine.Tick(state).State;
        Assert.Equal(GamePhase.Planning, state.Phase);
        var started = GameEngine.Tick(state);
        Assert.Equal(2, started.State.WaveNumber);
        Assert.Single(started.Events.OfType<GameEvent.WaveStarted>());
        Assert.Empty(GameEngine.Tick(started.State).Events.OfType<GameEvent.WaveStarted>());
    }

    [Fact]
    public void Pausing_freezes_countdown_and_disabling_cancels_it()
    {
        var state = TestGame.Apply(Cleared(), new GameCommand.SetPaused(true));
        Assert.Same(state, GameEngine.Tick(state).State);
        state = TestGame.Apply(state, new GameCommand.SetAutoWaves(false));
        Assert.False(state.HasCountdown);
        Assert.False(state.IsPaused);
        Assert.Same(state, GameEngine.Tick(state).State);
    }

    [Theory]
    [InlineData(12)]
    [InlineData(24)]
    [InlineData(39)]
    public void Region_completion_and_victory_never_arm_a_countdown(int wave) => Assert.False(Cleared(wave).CanAdvance);

    [Fact]
    public void Hidden_menu_and_help_suspend_without_rearming_countdown()
    {
        var waiting = TestGame.Apply(Cleared(), new GameCommand.Suspend());
        Assert.True(waiting.AutoWaves);
        Assert.False(waiting.CanAdvance);
        Assert.False(TestGame.Apply(waiting, new GameCommand.SetAutoWaves(true)).CanAdvance);
        Assert.Equal(GamePhase.Combat, TestGame.Apply(waiting, new GameCommand.StartWave()).Phase);
    }

    [Fact]
    public void Reload_requires_manual_start_and_speed_does_not_shorten_countdown()
    {
        var session = new GameSession();
        Assert.True(session.Restore(SavedRuns.Serialize(Cleared() with { SpeedMultiplier = 2 })).IsSuccess);
        Assert.False(session.State.CanAdvance);
        session.Dispatch(new GameCommand.StartWave());
        session.Dispatch(new GameCommand.SetPaused(true));
        Assert.True(session.State.IsPaused);

        var fast = Cleared() with { SpeedMultiplier = 2 };
        var slow = Cleared();
        for (var i = 0; i < 100; i++)
        {
            fast = GameEngine.Tick(fast).State;
            slow = GameEngine.Tick(slow).State;
            Assert.Equal(slow.AutoWaveTicksRemaining, fast.AutoWaveTicksRemaining);
            Assert.Equal(slow.Phase, fast.Phase);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Session_counts_five_real_seconds_and_drops_the_old_phase_remainder(int speed)
    {
        var schedule = WavePlanner.Schedule(1);
        var session = new GameSession();
        Assert.True(session.Restore(SavedRuns.Serialize(new GameState
        {
            Phase = GamePhase.Combat, WaveNumber = 1, AutoWaves = true, SpeedMultiplier = speed,
            Schedule = schedule, NextSpawnIndex = schedule.Length, WaveTick = schedule[^1].AtTick + 1
        })).IsSuccess);
        session.Dispatch(new GameCommand.SetPaused(false));
        session.Advance(TimeSpan.FromMilliseconds(250));
        Assert.Equal(100, session.State.AutoWaveTicksRemaining);
        for (var i = 0; i < 49; i++) session.Advance(TimeSpan.FromMilliseconds(100));
        Assert.Equal(2, session.State.AutoWaveTicksRemaining);
        session.Advance(TimeSpan.FromMilliseconds(150));
        Assert.Equal(2, session.State.WaveNumber);
        Assert.Equal(0, session.State.WaveTick);
        session.Advance(TimeSpan.FromMilliseconds(50));
        Assert.Equal(speed, session.State.WaveTick);
    }
}
