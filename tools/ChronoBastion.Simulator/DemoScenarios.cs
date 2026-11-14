using System.Collections.Immutable;
using ChronoBastion.Core;
using ChronoBastion.Core.Content;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;
using ChronoBastion.Core.Rules;

namespace ChronoBastion.Simulator;

public sealed record WaveReport(int Number, GamePhase Phase, int Health, int Gold, int Score,
    int Earned, int Spent, int TravelIncome, string Towers);
public sealed record SimulationReport(GameState FinalState, ImmutableArray<WaveReport> Waves, int Ticks);

public static class DemoScenarios
{
    private static readonly (int Pad, TowerKind Kind)[] BuildOrder =
    [
        (2, TowerKind.Pulse), (5, TowerKind.Mortar), (6, TowerKind.Frost),
        (1, TowerKind.Pulse), (9, TowerKind.Pulse), (8, TowerKind.Mortar),
        (4, TowerKind.Pulse), (7, TowerKind.Frost), (10, TowerKind.Pulse), (3, TowerKind.Mortar)
    ];

    public static SimulationReport Run(bool defend) => Run(defend ? "mixed" : "none");

    public static SimulationReport Run(string strategy)
    {
        var state = GameEngine.NewGame();
        var waves = ImmutableArray.CreateBuilder<WaveReport>();
        var ticks = 0;
        var spent = 0;
        var travelIncome = 0;
        var startGold = state.Gold;
        while (!state.IsFinished && ticks < 100000)
        {
            if (state.Phase == GamePhase.StageCleared)
            {
                var before = state.Gold;
                state = Apply(state, new GameCommand.NextStage());
                travelIncome = state.Gold - before;
            }
            if (state.Phase == GamePhase.Planning)
            {
                var before = state.Gold;
                if (strategy != "none") state = PrepareDefense(state, strategy);
                spent = before - state.Gold;
                state = Apply(state, new GameCommand.StartWave());
            }
            var transition = GameEngine.Tick(state);
            state = transition.State;
            ticks++;
            if (state.Phase != GamePhase.Combat)
            {
                waves.Add(new(state.WaveNumber, state.Phase, state.CoreHealth, state.Gold, state.Score,
                    state.Gold - startGold + spent - travelIncome, spent, travelIncome,
                    string.Join(";", state.Towers.Select(t => $"{t.PadId}:{t.Kind}:L{t.Level}"))));
                startGold = state.Gold;
                travelIncome = 0;
            }
        }
        if (!state.IsFinished)
            throw new InvalidOperationException("Simulation did not finish within its tick budget.");
        return new(state, waves.ToImmutable(), ticks);
    }

    private static GameState PrepareDefense(GameState state, string strategy)
    {
        var order = strategy switch
        {
            "archers" => BuildOrder.Select(t => (t.Pad, TowerKind.Pulse)).ToArray(),
            "catapults" => BuildOrder.Select((t, i) => (t.Pad, i == 0 ? TowerKind.Pulse : i == 2 ? TowerKind.Frost : TowerKind.Mortar)).ToArray(),
            "poor-placement" => WorstPads(state).Select((pad, i) => (pad, BuildOrder[i].Kind)).ToArray(),
            "no-upgrades" => BuildOrder.Take(3).ToArray(),
            _ => BuildOrder
        };
        foreach (var (pad, kind) in order)
        {
            if (GameQueries.TowerAt(state, pad).IsSome)
                continue;
            var result = GameEngine.Execute(state, new GameCommand.BuildTower(pad, kind));
            if (result is not Result<GameTransition, GameError>.Success built)
                return state;
            state = built.Value.State;
        }
        if (strategy == "no-upgrades") return state;
        foreach (var (pad, _) in order)
        {
            while (GameEngine.Execute(state, new GameCommand.UpgradeTower(pad)) is Result<GameTransition, GameError>.Success upgraded)
                state = upgraded.Value.State;
        }
        return state;
    }

    private static IEnumerable<int> WorstPads(GameState state)
    {
        var map = Campaign.Get(state.StageNumber).Map;
        return map.Pads.OrderBy(pad => Enumerable.Range(0, (int)map.Length / 10)
            .Count(step => pad.Position.DistanceSquaredTo(map.PositionAt(step * 10)) <= 135 * 135))
            .ThenBy(pad => pad.Id).Select(pad => pad.Id);
    }

    private static GameState Apply(GameState state, GameCommand command) => GameEngine.Execute(state, command).Match(
        result => result.State,
        error => throw new InvalidOperationException(error.Message));
}
