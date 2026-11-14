namespace ChronoBastion.Core.Model;

public abstract record GameCommand
{
    public sealed record BuildTower(int PadId, TowerKind Kind) : GameCommand;
    public sealed record UpgradeTower(int PadId) : GameCommand;
    public sealed record SellTower(int PadId) : GameCommand;
    public sealed record SetTargetPriority(int PadId, TargetPriority Priority) : GameCommand;
    public sealed record NextStage : GameCommand;
    public sealed record StartWave : GameCommand;
    public sealed record SetPaused(bool Paused) : GameCommand;
    public sealed record SetAutoWaves(bool Enabled) : GameCommand;
    public sealed record Suspend : GameCommand;
    public sealed record SetSpeed(int Multiplier) : GameCommand;
    public sealed record Restart : GameCommand;
}

public enum GameErrorCode
{
    InvalidPad, InvalidTowerKind, PadOccupied, TowerNotFound, InsufficientGold,
    MaxLevelReached, InvalidPhase, GameFinished, InvalidSpeed, InvalidPriority, UnknownCommand
}

public sealed record GameError(GameErrorCode Code, string Message);
