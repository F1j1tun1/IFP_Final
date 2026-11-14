using System.Collections.Immutable;
using ChronoBastion.Core.Model;
namespace ChronoBastion.Core.Content;

public sealed record Chapter(int Number, string Name, string Subtitle, int Waves, BattleMap Map);
public static class Campaign
{
    public static ImmutableArray<Chapter> Chapters { get; } =
    [
        new(1, "The Whispering Wood", "Guard the old forest road", 12, new(
            Road(new(0,341), new(114,338), new(80,239), new(159,173),
                new(210,126), new(285,129), new(320,206), new(365,278), new(354,383), new(429,399),
                new(536,429), new(570,345), new(632,300), new(695,257), new(804,254), new(847,205),
                new(856,195), new(862,183), new(864,175)),
            Pads((9,40), (25,44), (25,67), (45,35), (49,55), (49,85), (63,35), (70,62), (80,35), (86,62)))),
        new(2, "Ashfall Village", "Hold the road through the ruins", 12, new(
            Road(new(0,267), new(100,267), new(205,239), new(250,300),
                new(286,343), new(256,394), new(380,399), new(495,410), new(563,380), new(595,326),
                new(627,276), new(600,201), new(671,178), new(721,155), new(775,196), new(821,164),
                new(837,154), new(841,141), new(843,130)),
            Pads((10,40), (20,62), (34,40), (43,62), (43,84), (55,40), (62,84), (76,57), (76,40), (76,74)))),
        new(3, "The Ironthorn Keep", "Break the War Chief's siege", 15, new(
            Road(new(0,330), new(130,330), new(112,290), new(140,218),
                new(157,156), new(180,160), new(264,160), new(348,160), new(400,138), new(421,218),
                new(452,295), new(364,356), new(440,391), new(486,416), new(590,408), new(647,408),
                new(731,406), new(735,362), new(760,273), new(776,232), new(790,202), new(807,168)),
            Pads((10,46), (28,46), (24,66), (36,66), (56,46), (56,66), (56,86), (73,46), (73,86), (90,66))))
    ];
    public static Chapter Get(int number) => number is >= 1 and <= 3 ? Chapters[number - 1] : throw new ArgumentOutOfRangeException(nameof(number));

    private static ImmutableArray<BuildPad> Pads(params (double X, double Y)[] positions) =>
        positions.Select((p, i) => new BuildPad(i + 1, new(p.X * 9.6, p.Y * 5.4))).ToImmutableArray();

    private static ImmutableArray<Position> Road(params Position[] points)
    {
        var path = ImmutableArray.CreateBuilder<Position>();
        for (var segment = 0; segment < points.Length - 1; segment += 3)
        {
            var a = points[segment]; var b = points[segment + 1];
            var c = points[segment + 2]; var d = points[segment + 3];
            for (var step = 0; step < 16; step++)
            {
                var t = step / 16d; var u = 1 - t;
                path.Add(new(u*u*u*a.X + 3*u*u*t*b.X + 3*u*t*t*c.X + t*t*t*d.X,
                    u*u*u*a.Y + 3*u*u*t*b.Y + 3*u*t*t*c.Y + t*t*t*d.Y));
            }
        }
        path.Add(points[^1]);
        return path.ToImmutable();
    }
}
