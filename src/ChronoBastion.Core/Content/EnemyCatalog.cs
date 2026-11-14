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
        new(EnemyKind.Grunt, "Goblin", 35, 60, 0, 4, 20, 1),
        new(EnemyKind.Runner, "Goblin runner", 25, 95, 0, 3, 25, 1),
        new(EnemyKind.Armored, "Armored orc", 95, 40, 0.35, 7, 40, 2),
        new(EnemyKind.Boss, "War Chief", 900, 25, 0.25, 80, 1000, 20),
        new(EnemyKind.Orc, "Orc warrior", 80, 50, 0.1, 6, 35, 2),
        new(EnemyKind.Shaman, "Orc shaman", 100, 42, 0, 8, 60, 2),
        new(EnemyKind.Wolf, "Wolf", 30, 100, 0, 3, 25, 1),
        new(EnemyKind.Spider, "Venom spider", 22, 75, 0, 2, 15, 1),
        new(EnemyKind.Ent, "Ancient ent", 140, 30, 0.2, 10, 60, 3),
        new(EnemyKind.Skeleton, "Skeleton", 42, 65, 0.1, 4, 25, 1),
        new(EnemyKind.ArmoredSkeleton, "Armored skeleton", 100, 40, 0.35, 8, 45, 2),
        new(EnemyKind.Zombie, "Zombie", 85, 35, 0, 5, 30, 2),
        new(EnemyKind.Wraith, "Wraith", 45, 85, 0.2, 6, 40, 1),
        new(EnemyKind.Troll, "Troll", 250, 28, 0.15, 15, 90, 4)
    ];

    public static EnemyDefinition Get(EnemyKind kind) => All.FirstOrDefault(enemy => enemy.Kind == kind)
        ?? throw new ArgumentOutOfRangeException(nameof(kind));

    public static Enemy Spawn(EnemyKind kind, int id, int wave)
    {
        var health = Get(kind).Health * GameRules.HealthMultiplier(wave);
        return new(id, kind, health, health, 0);
    }
}
