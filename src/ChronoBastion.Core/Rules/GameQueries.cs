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
        EnemyKind.Boss => "Summons three goblins every six seconds. Reaching the gate ends the run.",
        EnemyKind.Shaman => "Heals up to three nearby allies every four seconds. Cannot heal itself or the War Chief.",
        EnemyKind.Wolf => "A fast forest hunter. Archers and frost stop its rush.",
        EnemyKind.Spider => "Fragile swarm attacker. Its venom harms the gate on arrival, not your towers.",
        EnemyKind.Ent => "Slow, tough bark armor. Concentrate fire before it reaches the gate.",
        EnemyKind.Skeleton => "A nimble undead fighter with light armor.",
        EnemyKind.ArmoredSkeleton => "Heavy armor absorbs 35% of incoming damage.",
        EnemyKind.Zombie => "Slow and tough. No resurrection: every kill stays a kill.",
        EnemyKind.Wraith => "Fast spectral raider. All three towers can hit it; follows the road.",
        EnemyKind.Troll => "A slow siege brute that deals four damage to the gate.",
        EnemyKind.Orc => "A sturdy warrior with light armor.",
        _ => "Unknown enemy."
    };
}
