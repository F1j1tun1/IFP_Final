using ChronoBastion.Core.Content;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Tests;

public sealed class ContentTests
{
    [Theory]
    [InlineData(-1, 0, 130)]
    [InlineData(220, 220, 130)]
    [InlineData(340, 220, 250)]
    [InlineData(1650, 960, 420)]
    [InlineData(9999, 960, 420)]
    public void Path_positions_follow_segments_and_clamp_to_endpoints(double distance, double x, double y)
    {
        Assert.Equal(new Position(x, y), BastionMap.PositionAt(distance));
        Assert.Equal(1650, BastionMap.Length);
    }

    [Fact]
    public void Every_wave_has_a_valid_increasing_schedule_and_the_final_wave_has_one_boss()
    {
        var previousCount = 0;
        for (var wave = 1; wave <= GameRules.TotalWaves; wave++)
        {
            if (wave is 13 or 25) previousCount = 0;
            var schedule = WavePlanner.Schedule(wave);
            Assert.Equal(WavePlanner.CountEnemies(WavePlanner.Describe(wave)), schedule.Length);
            Assert.True(schedule.Length > previousCount);
            Assert.Equal(0, schedule[0].AtTick);
            Assert.All(schedule.Zip(schedule.Skip(1)), pair => Assert.True(pair.Second.AtTick > pair.First.AtTick));
            Assert.Equal(wave == GameRules.TotalWaves ? 1 : 0, schedule.Count(spawn => spawn.Kind == EnemyKind.Boss));
            previousCount = schedule.Length;
        }
    }

    [Fact]
    public void Recursive_count_handles_empty_and_nested_groups()
    {
        var nested = new WaveGroup([new EnemyPack(EnemyKind.Grunt, 2, 20),
            new WaveGroup([new WaveGroup([]), new EnemyPack(EnemyKind.Runner, 3, 20)])]);
        Assert.Equal(5, WavePlanner.CountEnemies(nested));
        Assert.Equal(0, WavePlanner.CountEnemies(new WaveGroup([])));
    }

    [Fact]
    public void Content_contains_ten_distinct_pads_and_three_increasing_tower_levels()
    {
        Assert.Equal(10, BastionMap.Pads.Select(p => p.Id).Distinct().Count());
        Assert.All(BastionMap.Pads, pad =>
        {
            Assert.InRange(pad.Position.X, 0, BastionMap.Width);
            Assert.InRange(pad.Position.Y, 0, BastionMap.Height);
        });
        Assert.All(TowerCatalog.All, tower =>
        {
            Assert.Equal(3, tower.Levels.Length);
            Assert.True(tower.AtLevel(3).Damage > tower.AtLevel(2).Damage);
            Assert.True(tower.AtLevel(2).Damage > tower.AtLevel(1).Damage);
        });
        Assert.Equal(35 * 2.08, EnemyCatalog.Spawn(EnemyKind.Grunt, 1, 10).MaxHealth, 6);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(40)]
    public void Nonexistent_waves_are_rejected(int wave) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => WavePlanner.Describe(wave));
}
