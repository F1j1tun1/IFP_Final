using System.Collections.Immutable;
using ChronoBastion.Core.Model;
namespace ChronoBastion.Core.Content;

public sealed record Chapter(int Number, string Name, string Subtitle, int Waves, BattleMap Map);
public static class Campaign
{
    public static ImmutableArray<Chapter> Chapters { get; } =
    [
        new(1, "The Whispering Wood", "Guard the old forest road", 12, new(BastionMap.Path, BastionMap.Pads)),
        new(2, "Ashfall Village", "Hold the road through the ruins", 12, new(
            [new(0,400), new(240,400), new(240,150), new(480,150), new(480,390), new(730,390), new(730,140), new(960,140)],
            [new(1,new(115,290)), new(2,new(330,300)), new(3,new(120,480)), new(4,new(340,70)), new(5,new(395,240)), new(6,new(570,280)), new(7,new(600,465)), new(8,new(820,275)), new(9,new(640,180)), new(10,new(860,60))])),
        new(3, "The Ironthorn Keep", "Break the War Chief's siege", 15, new(
            [new(0,150), new(250,150), new(250,410), new(500,410), new(500,150), new(750,150), new(750,400), new(960,400)],
            [new(1,new(130,255)), new(2,new(340,260)), new(3,new(140,445)), new(4,new(365,490)), new(5,new(410,320)), new(6,new(585,260)), new(7,new(625,65)), new(8,new(845,270)), new(9,new(650,400)), new(10,new(850,480))]))
    ];
    public static Chapter Get(int number) => number is >= 1 and <= 3 ? Chapters[number - 1] : throw new ArgumentOutOfRangeException(nameof(number));
}
