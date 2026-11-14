using System.Collections.Immutable;
using ChronoBastion.Core.Content;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Core.Rules;

public static class EnemyAbilities
{
    public const int SummonInterval = 6 * GameRules.TicksPerSecond;
    public const int HealInterval = 4 * GameRules.TicksPerSecond;
    public const double HealRadius = 110;
    public const double HealFraction = 0.12;
    public const int MaxLivingEnemies = 120;

    public static int Interval(EnemyKind kind) => kind switch
    {
        EnemyKind.Boss => SummonInterval,
        EnemyKind.Shaman => HealInterval,
        _ => 0
    };

    internal static void Apply(List<Enemy> enemies, ImmutableArray<GameEvent>.Builder events,
        BattleMap map, int wave, ref int nextId)
    {
        var originalCount = enemies.Count;
        for (var i = 0; i < originalCount; i++)
        {
            var enemy = enemies[i];
            var interval = Interval(enemy.Kind);
            if (enemy.Health <= 0 || enemy.Distance >= map.Length || interval == 0) continue;

            var elapsed = enemy.AbilityTicks + 1;
            enemies[i] = enemy with { AbilityTicks = elapsed % interval };
            if (elapsed < interval) continue;

            var position = map.PositionAt(enemy.Distance);
            if (enemy.Kind == EnemyKind.Boss)
            {
                var count = Math.Min(3, MaxLivingEnemies - enemies.Count(e => e.Health > 0));
                for (var summon = 0; summon < count; summon++)
                {
                    var goblin = EnemyCatalog.Spawn(EnemyKind.Grunt, nextId++, wave) with
                    {
                        Distance = Math.Max(0, enemy.Distance - (summon + 1) * 12)
                    };
                    enemies.Add(goblin);
                    events.Add(new GameEvent.EnemySpawned(goblin.Id, goblin.Kind));
                }
                if (count > 0) events.Add(new GameEvent.Summoned(position));
                continue;
            }

            var targets = Enumerable.Range(0, originalCount)
                .Where(index => enemies[index] is { Health: > 0, Kind: not EnemyKind.Boss } ally &&
                    ally.Id != enemy.Id && ally.Health < ally.MaxHealth && ally.Distance < map.Length &&
                    position.DistanceSquaredTo(map.PositionAt(ally.Distance)) <= HealRadius * HealRadius)
                .OrderBy(index => enemies[index].Health / enemies[index].MaxHealth)
                .ThenBy(index => enemies[index].Id).Take(3).ToArray();
            foreach (var index in targets)
            {
                var ally = enemies[index];
                var health = Math.Min(ally.MaxHealth, ally.Health + ally.MaxHealth * HealFraction);
                enemies[index] = ally with { Health = health };
                events.Add(new GameEvent.EnemyHealed(ally.Id, health - ally.Health));
            }
        }
    }
}
