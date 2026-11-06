using ChronoBastion.Core.Model;
namespace ChronoBastion.Web.Services;
public sealed record ShotEffect(long Id, Position Origin, Position Target, TowerKind Kind, TimeSpan Expires);
