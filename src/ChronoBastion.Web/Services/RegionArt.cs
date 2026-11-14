namespace ChronoBastion.Web.Services;

public static class RegionArt
{
    public static string Key(int stage) => stage switch { 2 => "village", 3 => "keep", _ => "forest" };
    public static string Map(int stage) => $"art/maps/{Key(stage)}.png";
    public static (double X, double Y) Gate(int stage) => stage switch
    {
        2 => (840, 67.5),
        3 => (811.2, 102.6),
        _ => (854.4, 102.6)
    };
}
