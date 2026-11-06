using System.Collections.Immutable;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Core.Content;

public abstract record WaveNode;
public sealed record EnemyPack(EnemyKind Kind, int Count, int SpacingTicks) : WaveNode;
public sealed record WaveGroup(ImmutableArray<WaveNode> Children) : WaveNode;

public static class WavePlanner
{
    public static WaveGroup Describe(int number)
    {
        if (number is < 1 or > GameRules.TotalWaves)
            throw new ArgumentOutOfRangeException(nameof(number));

        var reinforcements = ImmutableArray.CreateBuilder<WaveNode>();
        if (number >= 2)
            reinforcements.Add(new EnemyPack(EnemyKind.Runner, number / 2 + 1, 14));
        if (number >= 4)
            reinforcements.Add(new EnemyPack(EnemyKind.Armored, number / 3 + 1, 24));
        if (number == GameRules.TotalWaves)
            reinforcements.Add(new EnemyPack(EnemyKind.Boss, 1, 20));

        return new([
            new EnemyPack(EnemyKind.Grunt, number + 4, Math.Max(12, 24 - number)),
            new WaveGroup(reinforcements.ToImmutable())
        ]);
    }

    public static int CountEnemies(WaveNode node) => node switch
    {
        EnemyPack pack => pack.Count,
        WaveGroup group => group.Children.Sum(CountEnemies),
        _ => throw new ArgumentException("Unknown wave node.", nameof(node))
    };

    public static ImmutableArray<Spawn> Schedule(int number)
    {
        var schedule = ImmutableArray.CreateBuilder<Spawn>();
        var tick = 0;
        foreach (var pack in Flatten(Describe(number)))
        {
            for (var i = 0; i < pack.Count; i++)
            {
                schedule.Add(new(tick, pack.Kind));
                tick += pack.SpacingTicks;
            }
        }
        return schedule.ToImmutable();
    }

    private static IEnumerable<EnemyPack> Flatten(WaveNode node) => node switch
    {
        EnemyPack pack => [pack],
        WaveGroup group => group.Children.SelectMany(Flatten),
        _ => throw new ArgumentException("Unknown wave node.", nameof(node))
    };
}
