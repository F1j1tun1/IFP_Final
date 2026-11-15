using System.Collections.Immutable;
using ChronoBastion.Core.Model;
namespace ChronoBastion.Core.Content;

public sealed record Chapter(int Number, string Name, string Subtitle, int Waves, int StartingGold, BattleMap Map);
public static class Campaign
{
    public static ImmutableArray<Chapter> Chapters { get; } =
    [
        new(1, "The Whispering Wood", "Guard the old forest road", 12, 300, new(
            Road(new(0, 341), new(69, 326), new(134, 294), new(166, 254),
                new(182, 201), new(200, 170), new(227, 152), new(258, 148),
                new(297, 158), new(331, 190), new(352, 230), new(367, 280),
                new(383, 323), new(407, 363), new(438, 389), new(477, 405),
                new(519, 406), new(560, 395), new(600, 368), new(633, 337),
                new(659, 312), new(698, 285), new(742, 264), new(794, 247),
                new(831, 226), new(859, 205), new(867, 193)),
            Pads((96, 237), (254, 238), (290, 350), (430, 175), (495, 315),
                (495, 475), (590, 265), (670, 395), (700, 205), (830, 335)))),
        new(2, "Ashfall Village", "Hold the road through the ruins", 12, 450, new(
            Road(new(0, 273), new(67, 272), new(120, 267), new(167, 262),
                new(214, 270), new(250, 295), new(272, 331), new(291, 365),
                new(320, 385), new(347, 390), new(407, 394), new(467, 391),
                new(524, 385), new(565, 363), new(594, 327), new(612, 284),
                new(614, 245), new(635, 220), new(650, 195), new(678, 183),
                new(723, 175), new(768, 174), new(811, 174), new(838, 156), new(845, 140)),
            Pads((100, 200), (170, 360), (335, 290), (450, 310), (410, 475),
                (530, 225), (540, 465), (620, 400), (685, 320), (770, 260)))),
        new(3, "The Ironthorn Keep", "Break the War Chief's siege", 15, 650, new(
            Road(new(0, 333), new(53, 329), new(90, 319), new(110, 298),
                new(137, 272), new(142, 230), new(153, 202), new(168, 184),
                new(186, 169), new(230, 160), new(287, 163), new(343, 163),
                new(386, 171), new(414, 200), new(426, 238), new(419, 273),
                new(408, 309), new(402, 341), new(408, 372), new(436, 391),
                new(469, 408), new(523, 410), new(587, 405), new(653, 408),
                new(695, 403), new(723, 380), new(747, 354), new(756, 315),
                new(770, 276), new(792, 243), new(801, 211), new(819, 177)),
            Pads((320, 100), (230, 250), (170, 365), (332, 330), (500, 255),
                (570, 335), (540, 480), (660, 260), (690, 480), (845, 330))))
    ];
    public static Chapter Get(int number) => number is >= 1 and <= 3 ? Chapters[number - 1] : throw new ArgumentOutOfRangeException(nameof(number));

    private static ImmutableArray<BuildPad> Pads(params (double X, double Y)[] positions) =>
        positions.Select((p, i) => new BuildPad(i + 1, new(p.X, p.Y))).ToImmutableArray();

    private static ImmutableArray<Position> Road(params Position[] points)
    {
        var path = ImmutableArray.CreateBuilder<Position>();
        for (var segment = 0; segment < points.Length - 1; segment++)
        {
            var a = points[Math.Max(0, segment - 1)];
            var b = points[segment];
            var c = points[segment + 1];
            var d = points[Math.Min(points.Length - 1, segment + 2)];
            for (var step = 0; step < 8; step++)
            {
                var t = step / 8d;
                path.Add(new(Curve(a.X, b.X, c.X, d.X, t), Curve(a.Y, b.Y, c.Y, d.Y, t)));
            }
        }
        path.Add(points[^1]);
        return path.ToImmutable();
    }

    private static double Curve(double a, double b, double c, double d, double t) =>
        0.5 * (2 * b + (c - a) * t + (2 * a - 5 * b + 4 * c - d) * t * t +
            (3 * b - a - 3 * c + d) * t * t * t);
}
