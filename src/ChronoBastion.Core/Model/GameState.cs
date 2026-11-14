using System.Collections.Immutable;

namespace ChronoBastion.Core.Model;

public enum GamePhase { Planning, Combat, Won, Lost, StageCleared }
public enum TowerKind { Pulse, Mortar, Frost }
public enum EnemyKind { Grunt, Runner, Armored, Boss, Orc, Shaman, Wolf, Spider, Ent, Skeleton, ArmoredSkeleton, Zombie, Wraith, Troll }
public enum TargetPriority { First, Strongest, Nearest }

public readonly record struct Position(double X, double Y)
{
    public double DistanceSquaredTo(Position other) =>
        (X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y);
}

public sealed record Tower(
    int PadId,
    TowerKind Kind,
    int Level,
    int InvestedGold,
    int CooldownTicks,
    TargetPriority Priority);

public sealed record Enemy(
    int Id,
    EnemyKind Kind,
    double Health,
    double MaxHealth,
    double Distance,
    double SlowMultiplier = 1,
    int SlowTicks = 0);

public sealed record Spawn(int AtTick, EnemyKind Kind);

public sealed record GameState
{
    public GamePhase Phase { get; init; } = GamePhase.Planning;
    public int StageNumber { get; init; } = 1;
    public int WaveInStage => WaveNumber - (StageNumber - 1) * 12;
    public int StageWaves => StageNumber == 3 ? 15 : 12;
    public int WaveNumber { get; init; }
    public int WavesCleared { get; init; }
    public int Gold { get; init; } = GameRules.StartingGold;
    public int CoreHealth { get; init; } = GameRules.StartingHealth;
    public int Score { get; init; }
    public int Kills { get; init; }
    public int Escaped { get; init; }
    public int WaveTick { get; init; }
    public int NextEnemyId { get; init; } = 1;
    public int NextSpawnIndex { get; init; }
    public bool IsPaused { get; init; }
    public int SpeedMultiplier { get; init; } = 1;
    public ImmutableArray<Tower> Towers { get; init; } = [];
    public ImmutableArray<Enemy> Enemies { get; init; } = [];
    public ImmutableArray<Spawn> Schedule { get; init; } = [];
    public bool IsFinished => Phase is GamePhase.Won or GamePhase.Lost;
    public int PendingEnemies => Schedule.Length - NextSpawnIndex;
}

public static class GameRules
{
    public const int StartingGold = 300;
    public const int StartingHealth = 20;
    public const int TotalWaves = 39;
    public const int MaxTowerLevel = 3;
    public const int TicksPerSecond = 20;
    public const double TickSeconds = 1.0 / TicksPerSecond;
    public const int VictoryBonus = 1000;
    public const int HealthBonus = 50;

    public static int WaveGold(int wave) => 30 + wave * 5;
    public static int WaveScore(int wave) => wave * 100;
    public static int SellRefund(Tower tower) => tower.InvestedGold * 70 / 100;
    public static double HealthMultiplier(int wave) => 1 + (wave - 1) * 0.1;
}
