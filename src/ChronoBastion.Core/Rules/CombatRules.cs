using ChronoBastion.Core.Content;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Core.Rules;

public static class CombatRules
{
    public static Func<Enemy, double> DamageFor(TowerStats stats) =>
        enemy => Math.Max(1, stats.Damage * (1 - EnemyCatalog.Get(enemy.Kind).Armor));

    public static Enemy ApplyHit(Enemy enemy, TowerStats stats)
    {
        var hit = enemy with { Health = Math.Max(0, enemy.Health - DamageFor(stats)(enemy)) };
        if (stats.SlowDurationTicks == 0)
            return hit;

        return hit with
        {
            SlowMultiplier = enemy.SlowTicks > 0
                ? Math.Min(enemy.SlowMultiplier, stats.SlowMultiplier)
                : stats.SlowMultiplier,
            SlowTicks = Math.Max(enemy.SlowTicks, stats.SlowDurationTicks)
        };
    }

    public static Enemy Move(Enemy enemy)
    {
        var multiplier = enemy.SlowTicks > 0 ? enemy.SlowMultiplier : 1;
        var remaining = Math.Max(0, enemy.SlowTicks - 1);
        return enemy with
        {
            Distance = enemy.Distance + EnemyCatalog.Get(enemy.Kind).Speed * multiplier * GameRules.TickSeconds,
            SlowTicks = remaining,
            SlowMultiplier = remaining == 0 ? 1 : multiplier
        };
    }

    public static Func<Enemy, double> RankBy(TargetPriority priority, Position origin, BattleMap? map = null) => priority switch
    {
        TargetPriority.First => enemy => enemy.Distance,
        TargetPriority.Strongest => enemy => enemy.Health,
        TargetPriority.Nearest => enemy => -origin.DistanceSquaredTo((map?.PositionAt(enemy.Distance) ?? BastionMap.PositionAt(enemy.Distance))),
        _ => throw new ArgumentOutOfRangeException(nameof(priority))
    };

    public static Option<Enemy> SelectTarget(
        IEnumerable<Enemy> enemies, Position origin, double range, Func<Enemy, double> rank, BattleMap? map = null)
    {
        Enemy? best = null;
        var bestRank = double.NegativeInfinity;
        foreach (var enemy in enemies)
        {
            if (enemy.Health <= 0 || origin.DistanceSquaredTo(map?.PositionAt(enemy.Distance) ?? BastionMap.PositionAt(enemy.Distance)) > range * range)
                continue;

            var score = rank(enemy);
            if (best is null || score > bestRank || (score == bestRank && enemy.Id < best.Id))
            {
                best = enemy;
                bestRank = score;
            }
        }
        return best is null ? Option<Enemy>.None : Option<Enemy>.Some(best);
    }
}
