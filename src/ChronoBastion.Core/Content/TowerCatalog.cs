using System.Collections.Immutable;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Core.Content;

public sealed record TowerStats(
    int Price, double Damage, double Range, int FireIntervalTicks,
    double SplashRadius = 0, double SlowMultiplier = 1, int SlowDurationTicks = 0);

public sealed record TowerDefinition(
    TowerKind Kind, string Name, string Description, ImmutableArray<TowerStats> Levels)
{
    public TowerStats AtLevel(int level) => level is >= 1 and <= GameRules.MaxTowerLevel
        ? Levels[level - 1]
        : throw new ArgumentOutOfRangeException(nameof(level));
}

public static class TowerCatalog
{
    public static ImmutableArray<TowerDefinition> All { get; } =
    [
        new(TowerKind.Pulse, "Archer", "Rapid, precise fire against a single enemy.",
            [new(90, 12, 135, 8), new(110, 20, 150, 8), new(180, 32, 165, 7)]),
        new(TowerKind.Mortar, "Catapult", "Heavy stones strike groups of enemies.",
            [new(140, 28, 160, 28, 65), new(160, 46, 175, 26, 75), new(260, 72, 190, 24, 85)]),
        new(TowerKind.Frost, "Frost mage", "Slows enemies so other towers can keep firing.",
            [new(100, 5, 140, 16, 0, 0.6, 35), new(120, 8, 155, 14, 0, 0.5, 40),
             new(190, 12, 170, 12, 0, 0.4, 45)])
    ];

    public static TowerDefinition Get(TowerKind kind) => All.FirstOrDefault(tower => tower.Kind == kind)
        ?? throw new ArgumentOutOfRangeException(nameof(kind));
}
