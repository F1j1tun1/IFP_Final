using System.Collections.Immutable;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Core.Content;

public sealed record BuildPad(int Id, Position Position);

public sealed class BattleMap(ImmutableArray<Position> path, ImmutableArray<BuildPad> pads)
{
    private readonly double[] distances = Measure(path);
    public ImmutableArray<Position> Path { get; } = path;
    public ImmutableArray<BuildPad> Pads { get; } = pads;
    public double Length => distances[^1];
    public Position PositionAt(double distance)
    {
        if (!double.IsFinite(distance)) throw new ArgumentOutOfRangeException(nameof(distance));
        distance = Math.Clamp(distance, 0, Length);
        var index = Array.BinarySearch(distances, distance);
        if (index >= 0) return Path[index];
        var end = ~index;
        var start = end - 1;
        var ratio = (distance - distances[start]) / (distances[end] - distances[start]);
        return new(Path[start].X + (Path[end].X - Path[start].X) * ratio,
            Path[start].Y + (Path[end].Y - Path[start].Y) * ratio);
    }

    private static double[] Measure(ImmutableArray<Position> path)
    {
        var distances = new double[path.Length];
        for (var i = 1; i < path.Length; i++)
            distances[i] = distances[i - 1] + Math.Sqrt(path[i - 1].DistanceSquaredTo(path[i]));
        return distances;
    }
    public double HeadingAt(double distance)
    {
        var a = PositionAt(Math.Max(0, distance - 1));
        var b = PositionAt(Math.Min(Length, distance + 1));
        return Math.Atan2(b.Y - a.Y, b.X - a.X) * 180 / Math.PI - 90;
    }
}

public static class BastionMap
{
    public const int Width = 960;
    public const int Height = 540;

    public static ImmutableArray<Position> Path { get; } =
    [
        new(0, 130), new(220, 130), new(220, 370), new(500, 370),
        new(500, 170), new(740, 170), new(740, 420), new(960, 420)
    ];

    public static ImmutableArray<BuildPad> Pads { get; } =
    [
        new(1, new(120, 240)), new(2, new(300, 230)), new(3, new(125, 415)),
        new(4, new(350, 455)), new(5, new(410, 275)), new(6, new(590, 270)),
        new(7, new(620, 90)), new(8, new(835, 270)), new(9, new(640, 430)),
        new(10, new(855, 500))
    ];

    public static double Length { get; } = Enumerable.Range(1, Path.Length - 1)
        .Sum(i => Math.Sqrt(Path[i - 1].DistanceSquaredTo(Path[i])));

    public static Option<BuildPad> FindPad(int id) =>
        Pads.FirstOrDefault(pad => pad.Id == id) is { } pad
            ? Option<BuildPad>.Some(pad)
            : Option<BuildPad>.None;

    public static Position PositionAt(double distance) => Interpolate(Path, Length, distance);

    internal static Position Interpolate(ImmutableArray<Position> Path, double Length, double distance)
    {
        if (!double.IsFinite(distance))
            throw new ArgumentOutOfRangeException(nameof(distance));

        distance = Math.Clamp(distance, 0, Length);
        for (var i = 1; i < Path.Length; i++)
        {
            var start = Path[i - 1];
            var end = Path[i];
            var length = Math.Sqrt(start.DistanceSquaredTo(end));
            if (distance <= length)
            {
                var ratio = distance / length;
                return new(start.X + (end.X - start.X) * ratio, start.Y + (end.Y - start.Y) * ratio);
            }
            distance -= length;
        }
        return Path[^1];
    }
}
