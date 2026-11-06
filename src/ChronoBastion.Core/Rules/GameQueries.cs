using ChronoBastion.Core.Content;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Core.Rules;

public static class GameQueries
{
    public static Option<Tower> TowerAt(GameState state, int padId) =>
        state.Towers.FirstOrDefault(tower => tower.PadId == padId) is { } tower
            ? Option<Tower>.Some(tower)
            : Option<Tower>.None;

    public static Option<int> UpgradePrice(Tower tower) => tower.Level < GameRules.MaxTowerLevel
        ? Option<int>.Some(TowerCatalog.Get(tower.Kind).AtLevel(tower.Level + 1).Price)
        : Option<int>.None;

    public static string Describe(Enemy enemy) => enemy.Kind switch
    {
        EnemyKind.Grunt => "A standard unit with no armor.",
        EnemyKind.Runner => "Fast and fragile. Do not let it slip past.",
        EnemyKind.Armored => "Heavy armor reduces incoming damage by 35%.",
        EnemyKind.Boss => "The Overseer can destroy a full-health core in one hit.",
        _ => "Unknown enemy."
    };
}
