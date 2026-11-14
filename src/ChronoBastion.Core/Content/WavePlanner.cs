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

        var stage = number <= 12 ? 1 : number <= 24 ? 2 : 3;
        var local = number - (stage - 1) * 12;
        var packs = ImmutableArray.CreateBuilder<WaveNode>();
        var basic = stage == 1 ? EnemyKind.Grunt : stage == 2 ? EnemyKind.Skeleton : EnemyKind.Orc;
        packs.Add(new EnemyPack(basic, local + 4, Math.Max(12, 24-local)));
        if (local >= 2) packs.Add(new EnemyPack(stage == 1 ? EnemyKind.Runner : stage == 2 ? EnemyKind.Zombie : EnemyKind.Armored, local / 2 + 1, 20));
        if (local >= 3) packs.Add(new EnemyPack(stage == 1 ? EnemyKind.Wolf : stage == 2 ? EnemyKind.Wraith : EnemyKind.Shaman, local / 3 + 1, 22));
        if (local >= 5) packs.Add(new EnemyPack(stage == 1 ? EnemyKind.Spider : stage == 2 ? EnemyKind.ArmoredSkeleton : EnemyKind.Troll, local / 3 + 1, 24));
        if (stage == 1 && local >= 8) packs.Add(new EnemyPack(EnemyKind.Ent, local / 4, 30));
        if (number == GameRules.TotalWaves) packs.Add(new EnemyPack(EnemyKind.Boss, 1, 20));
        return new([new WaveGroup(packs.ToImmutable())]);
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
