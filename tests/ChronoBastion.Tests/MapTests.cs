using ChronoBastion.Core.Content;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Tests;

public sealed class MapTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Clearings_leave_room_for_the_road_and_can_cover_it_without_upgrades(int stage)
    {
        var map = Campaign.Get(stage).Map;
        var samples = Enumerable.Range(0, (int)map.Length / 2)
            .Select(step => map.PositionAt(step * 2)).ToArray();
        var range = TowerCatalog.All.Min(tower => tower.AtLevel(1).Range);
        foreach (var pad in map.Pads)
        {
            Assert.InRange(pad.Position.X, 41, BastionMap.Width - 41);
            Assert.InRange(pad.Position.Y, 56, BastionMap.Height - 40);
            Assert.True(samples.Min(pad.Position.DistanceSquaredTo) >= 60 * 60,
                $"Region {stage}, pad {pad.Id} crowds the road.");
            Assert.True(samples.Count(p => pad.Position.DistanceSquaredTo(p) <= range * range) * 2 >= 120,
                $"Region {stage}, pad {pad.Id} has too little road coverage.");
            Assert.All(map.Pads.Where(other => other.Id != pad.Id), other =>
                Assert.True(pad.Position.DistanceSquaredTo(other.Position) >= 100 * 100));
        }
    }

    [Fact]
    public void Distance_lookup_preserves_speed_across_unequal_segments()
    {
        var map = new BattleMap([new(0, 0), new(30, 0), new(30, 40), new(130, 40)], []);
        Assert.Equal(170, map.Length);
        Assert.Equal(new Position(0, 0), map.PositionAt(-5));
        Assert.Equal(new Position(30, 0), map.PositionAt(30));
        Assert.Equal(new Position(30, 25), map.PositionAt(55));
        Assert.Equal(new Position(45, 40), map.PositionAt(85));
        Assert.Equal(new Position(130, 40), map.PositionAt(500));
        Assert.Throws<ArgumentOutOfRangeException>(() => map.PositionAt(double.PositiveInfinity));
    }
}
