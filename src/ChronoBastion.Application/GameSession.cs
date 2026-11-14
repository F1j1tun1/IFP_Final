using System.Collections.Immutable;
using ChronoBastion.Core;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Application;

public sealed class GameSession
{
    private const long TickDuration = TimeSpan.TicksPerSecond / GameRules.TicksPerSecond;
    private const long MaxElapsed = TimeSpan.TicksPerSecond / 4;
    private long accumulatedTime;

    public GameState State { get; private set; } = GameEngine.NewGame();
    public event Action<GameTransition>? Updated;

    public Result<GameTransition, GameError> Dispatch(GameCommand command)
    {
        var result = GameEngine.Execute(State, command);
        if (result is Result<GameTransition, GameError>.Success success)
        {
            if (command is GameCommand.NextStage or GameCommand.Restart or GameCommand.StartWave or GameCommand.SetPaused or GameCommand.SetSpeed or GameCommand.SetAutoWaves or GameCommand.Suspend)
                accumulatedTime = 0;
            State = success.Value.State;
            Updated?.Invoke(success.Value);
        }
        return result;
    }

    public Result<GameTransition, string> Restore(string json)
    {
        return SavedRuns.Parse(json).Map(state =>
        {
            accumulatedTime = 0;
            State = state with { IsPaused = state.Phase == GamePhase.Combat, AutoWaveTicksRemaining = 0 };
            var transition = GameTransition.Unchanged(State);
            Updated?.Invoke(transition);
            return transition;
        });
    }

    public void Advance(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(elapsed));
        if (!State.CanAdvance)
        {
            accumulatedTime = 0;
            return;
        }

        // Drop long background-tab gaps instead of simulating an unseen battle.
        var phase = State.Phase;
        accumulatedTime += Math.Min(elapsed.Ticks, MaxElapsed) * (phase == GamePhase.Combat ? State.SpeedMultiplier : 1);
        var events = ImmutableArray.CreateBuilder<GameEvent>();
        var advanced = false;
        while (accumulatedTime >= TickDuration && State.CanAdvance && State.Phase == phase)
        {
            var transition = GameEngine.Tick(State);
            State = transition.State;
            events.AddRange(transition.Events);
            accumulatedTime -= TickDuration;
            advanced = true;
        }
        if (!State.CanAdvance || State.Phase != phase)
            accumulatedTime = 0;
        if (advanced)
            Updated?.Invoke(new(State, events.ToImmutable()));
    }
}
