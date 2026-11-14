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
            if (command is GameCommand.NextStage or GameCommand.Restart or GameCommand.StartWave or GameCommand.SetPaused or GameCommand.SetSpeed)
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
            State = state with { IsPaused = state.Phase == GamePhase.Combat };
            var transition = GameTransition.Unchanged(State);
            Updated?.Invoke(transition);
            return transition;
        });
    }

    public void Advance(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(elapsed));
        if (State.Phase != GamePhase.Combat || State.IsPaused)
        {
            accumulatedTime = 0;
            return;
        }

        // Drop long background-tab gaps instead of simulating an unseen battle.
        accumulatedTime += Math.Min(elapsed.Ticks, MaxElapsed) * State.SpeedMultiplier;
        var events = ImmutableArray.CreateBuilder<GameEvent>();
        var advanced = false;
        while (accumulatedTime >= TickDuration && State.Phase == GamePhase.Combat)
        {
            var transition = GameEngine.Tick(State);
            State = transition.State;
            events.AddRange(transition.Events);
            accumulatedTime -= TickDuration;
            advanced = true;
        }
        if (State.Phase != GamePhase.Combat)
            accumulatedTime = 0;
        if (advanced)
            Updated?.Invoke(new(State, events.ToImmutable()));
    }
}
