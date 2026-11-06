using System.Collections.Immutable;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Core.Content;

public sealed record EnemyDefinition(
    EnemyKind Kind, string Name, double Health, double Speed, double Armor,
    int Gold, int Points, int CoreDamage);

public static class EnemyCatalog
{
    public static ImmutableArray<EnemyDefinition> All { get; } =
    [
        new(EnemyKind.Grunt, "Grunt", 35, 60, 0, 12, 20, 1),
        new(EnemyKind.Runner, "Runner", 25, 95, 0, 10, 25, 1),
        new(EnemyKind.Armored, "Armored", 95, 40, 0.35, 22, 40, 2),
        new(EnemyKind.Boss, "The Overseer", 1300, 25, 0.25, 250, 1000, 20)
    ];

    public static EnemyDefinition Get(EnemyKind kind) => All.FirstOrDefault(enemy => enemy.Kind == kind)
        ?? throw new ArgumentOutOfRangeException(nameof(kind));

    public static Enemy Spawn(EnemyKind kind, int id, int wave)
    {
        var health = Get(kind).Health * GameRules.HealthMultiplier(wave);
        return new(id, kind, health, health, 0);
    }
}
