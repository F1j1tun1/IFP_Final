using ChronoBastion.Core;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Tests;

internal static class TestGame
{
    public static GameState Apply(GameState state, GameCommand command) =>
        Assert.IsType<Result<GameTransition, GameError>.Success>(GameEngine.Execute(state, command)).Value.State;

    public static GameError Error(GameState state, GameCommand command) =>
        Assert.IsType<Result<GameTransition, GameError>.Failure>(GameEngine.Execute(state, command)).Error;

    public static GameState Combat(params Enemy[] enemies) => new()
    {
        Phase = GamePhase.Combat,
        WaveNumber = 1,
        Enemies = [.. enemies],
        Schedule = [new(10000, EnemyKind.Grunt)]
    };
}
