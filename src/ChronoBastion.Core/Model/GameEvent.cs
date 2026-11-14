using System.Collections.Immutable;

namespace ChronoBastion.Core.Model;

public abstract record GameEvent
{
    public sealed record EnemyHealed(int EnemyId, double Amount) : GameEvent;
    public sealed record TowerBuilt(int PadId, TowerKind Kind) : GameEvent;
    public sealed record TowerUpgraded(int PadId, int Level) : GameEvent;
    public sealed record TowerSold(int PadId, int Refund) : GameEvent;
    public sealed record EnemySpawned(int EnemyId, EnemyKind Kind) : GameEvent;
    public sealed record ShotFired(int PadId, TowerKind Kind, Position Target) : GameEvent;
    public sealed record EnemyKilled(int EnemyId, Position Position, int Gold, int Points) : GameEvent;
    public sealed record EnemyEscaped(int EnemyId, int CoreDamage) : GameEvent;
    public sealed record WaveStarted(int Number) : GameEvent;
    public sealed record WaveCleared(int Number, int Gold, int Points) : GameEvent;
    public sealed record GameEnded(bool Won, int Score) : GameEvent;
    public sealed record StageEntered(int Number, int Refund, int Bonus) : GameEvent;
    public sealed record Summoned(Position Position) : GameEvent;
    public sealed record GameRestarted : GameEvent;
}

public sealed record GameTransition(GameState State, ImmutableArray<GameEvent> Events)
{
    public static GameTransition Unchanged(GameState state) => new(state, []);
}
