using ChronoBastion.Core.Model;
namespace ChronoBastion.Web.Services;
public sealed record BurstEffect(long Id, Position Position, TimeSpan Expires);
