using System.Collections.Immutable;
using ChronoBastion.Core;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;
using ChronoBastion.Core.Rules;

namespace ChronoBastion.Simulator;

public sealed record WaveReport(int Number, GamePhase Phase, int Health, int Gold, int Score);
public sealed record SimulationReport(GameState FinalState, ImmutableArray<WaveReport> Waves, int Ticks);

public static class DemoScenarios
{
    private static readonly (int Pad, TowerKind Kind)[] BuildOrder =
    [
        (2, TowerKind.Pulse), (5, TowerKind.Mortar), (6, TowerKind.Frost),
        (1, TowerKind.Pulse), (9, TowerKind.Pulse), (8, TowerKind.Mortar),
        (4, TowerKind.Pulse), (7, TowerKind.Frost), (10, TowerKind.Pulse), (3, TowerKind.Mortar)
    ];

    public static SimulationReport Run(bool defend)
    {
        var state = GameEngine.NewGame();
        var waves = ImmutableArray.CreateBuilder<WaveReport>();
        var ticks = 0;
        while (!state.IsFinished && ticks < 100000)
        {
            if (state.Phase == GamePhase.Planning)
            {
                if (defend)
                    state = PrepareDefense(state);
                state = Apply(state, new GameCommand.StartWave());
            }
            var transition = GameEngine.Tick(state);
            state = transition.State;
            ticks++;
            if (state.Phase != GamePhase.Combat)
                waves.Add(new(state.WaveNumber, state.Phase, state.CoreHealth, state.Gold, state.Score));
        }
        if (!state.IsFinished)
            throw new InvalidOperationException("Simulation did not finish within its tick budget.");
        return new(state, waves.ToImmutable(), ticks);
    }

    private static GameState PrepareDefense(GameState state)
    {
        foreach (var (pad, kind) in BuildOrder)
        {
            if (GameQueries.TowerAt(state, pad).IsSome)
                continue;
            var result = GameEngine.Execute(state, new GameCommand.BuildTower(pad, kind));
            if (result is not Result<GameTransition, GameError>.Success built)
                return state;
            state = built.Value.State;
        }
        foreach (var (pad, _) in BuildOrder)
        {
            while (GameEngine.Execute(state, new GameCommand.UpgradeTower(pad)) is Result<GameTransition, GameError>.Success upgraded)
                state = upgraded.Value.State;
        }
        return state;
    }

    private static GameState Apply(GameState state, GameCommand command) => GameEngine.Execute(state, command).Match(
        result => result.State,
        error => throw new InvalidOperationException(error.Message));
}
