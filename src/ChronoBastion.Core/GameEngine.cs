using System.Collections.Immutable;
using ChronoBastion.Core.Content;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;
using ChronoBastion.Core.Rules;

namespace ChronoBastion.Core;

public static class GameEngine
{
    public static GameState NewGame() => new();

    public static Result<GameTransition, GameError> Execute(GameState state, GameCommand command)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(command);

        if (command is GameCommand.Restart)
            return Success(NewGame(), new GameEvent.GameRestarted());
        if (state.IsFinished)
            return Failure(GameErrorCode.GameFinished, "This run has ended. Start a new game.");

        return command switch
        {
            GameCommand.BuildTower build => Build(state, build),
            GameCommand.UpgradeTower upgrade => Upgrade(state, upgrade.PadId),
            GameCommand.SellTower sell => Sell(state, sell.PadId),
            GameCommand.SetTargetPriority target => SetPriority(state, target),
            GameCommand.StartWave => StartWave(state),
            GameCommand.SetPaused pause => state.Phase == GamePhase.Combat
                ? Success(state with { IsPaused = pause.Paused })
                : Failure(GameErrorCode.InvalidPhase, "Pause is available during a wave."),
            GameCommand.SetSpeed speed => speed.Multiplier is 1 or 2
                ? Success(state with { SpeedMultiplier = speed.Multiplier })
                : Failure(GameErrorCode.InvalidSpeed, "Choose speed 1 or 2."),
            _ => Failure(GameErrorCode.UnknownCommand, "This action is not supported.")
        };
    }

    private static Result<GameTransition, GameError> Build(GameState state, GameCommand.BuildTower build)
    {
        if (!BastionMap.FindPad(build.PadId).IsSome)
            return Failure(GameErrorCode.InvalidPad, "Choose a marked build pad.");
        if (!Enum.IsDefined(build.Kind))
            return Failure(GameErrorCode.InvalidTowerKind, "Choose an available tower type.");
        if (GameQueries.TowerAt(state, build.PadId).IsSome)
            return Failure(GameErrorCode.PadOccupied, "This pad already has a tower.");

        var price = TowerCatalog.Get(build.Kind).AtLevel(1).Price;
        if (state.Gold < price)
            return Failure(GameErrorCode.InsufficientGold, "Not enough gold to build this tower.");

        var tower = new Tower(build.PadId, build.Kind, 1, price, 0, TargetPriority.First);
        return Success(state with
        {
            Gold = state.Gold - price,
            Towers = state.Towers.Add(tower).Sort((a, b) => a.PadId.CompareTo(b.PadId))
        }, new GameEvent.TowerBuilt(build.PadId, build.Kind));
    }

    private static Result<GameTransition, GameError> Upgrade(GameState state, int padId) =>
        FindTower(state, padId)
            .Bind(tower => ValidateUpgrade(state, tower))
            .Map(tower =>
            {
                var price = TowerCatalog.Get(tower.Kind).AtLevel(tower.Level + 1).Price;
                var upgraded = tower with { Level = tower.Level + 1, InvestedGold = tower.InvestedGold + price };
                return new GameTransition(state with
                {
                    Gold = state.Gold - price,
                    Towers = state.Towers.Replace(tower, upgraded)
                }, [new GameEvent.TowerUpgraded(padId, upgraded.Level)]);
            });

    private static Result<Tower, GameError> ValidateUpgrade(GameState state, Tower tower)
    {
        if (tower.Level >= GameRules.MaxTowerLevel)
            return Result<Tower, GameError>.Fail(new(GameErrorCode.MaxLevelReached, "This tower is at maximum level."));
        var price = TowerCatalog.Get(tower.Kind).AtLevel(tower.Level + 1).Price;
        return state.Gold >= price
            ? Result<Tower, GameError>.Ok(tower)
            : Result<Tower, GameError>.Fail(new(GameErrorCode.InsufficientGold, "Not enough gold to upgrade this tower."));
    }

    private static Result<GameTransition, GameError> Sell(GameState state, int padId) =>
        FindTower(state, padId).Map(tower =>
        {
            var refund = GameRules.SellRefund(tower);
            return new GameTransition(state with
            {
                Gold = state.Gold + refund,
                Towers = state.Towers.Remove(tower)
            }, [new GameEvent.TowerSold(padId, refund)]);
        });

    private static Result<GameTransition, GameError> SetPriority(GameState state, GameCommand.SetTargetPriority command)
    {
        if (!Enum.IsDefined(command.Priority))
            return Failure(GameErrorCode.InvalidPriority, "Choose First, Strongest, or Nearest.");

        return FindTower(state, command.PadId).Map(tower => new GameTransition(
            state with { Towers = state.Towers.Replace(tower, tower with { Priority = command.Priority }) }, []));
    }

    private static Result<Tower, GameError> FindTower(GameState state, int padId) =>
        GameQueries.TowerAt(state, padId).Match(
            Result<Tower, GameError>.Ok,
            () => Result<Tower, GameError>.Fail(new(GameErrorCode.TowerNotFound, "There is no tower on this pad.")));

    private static Result<GameTransition, GameError> StartWave(GameState state)
    {
        if (state.Phase != GamePhase.Planning)
            return Failure(GameErrorCode.InvalidPhase, "Finish the current wave first.");

        var number = state.WaveNumber + 1;
        return Success(state with
        {
            Phase = GamePhase.Combat,
            WaveNumber = number,
            WaveTick = 0,
            NextSpawnIndex = 0,
            Schedule = WavePlanner.Schedule(number),
            IsPaused = false,
            Towers = state.Towers.Select(tower => tower with { CooldownTicks = 0 }).ToImmutableArray()
        }, new GameEvent.WaveStarted(number));
    }

    public static GameTransition Tick(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.Phase != GamePhase.Combat || state.IsPaused)
            return GameTransition.Unchanged(state);

        var enemies = state.Enemies.ToList();
        var events = ImmutableArray.CreateBuilder<GameEvent>();
        var nextSpawn = state.NextSpawnIndex;
        var nextId = state.NextEnemyId;
        while (nextSpawn < state.Schedule.Length && state.Schedule[nextSpawn].AtTick <= state.WaveTick)
        {
            var enemy = EnemyCatalog.Spawn(state.Schedule[nextSpawn].Kind, nextId++, state.WaveNumber);
            enemies.Add(enemy);
            events.Add(new GameEvent.EnemySpawned(enemy.Id, enemy.Kind));
            nextSpawn++;
        }

        var towers = FireTowers(state.Towers, enemies, events);
        var gold = state.Gold;
        var score = state.Score;
        var kills = state.Kills;
        var escaped = state.Escaped;
        var health = state.CoreHealth;
        var survivors = ImmutableArray.CreateBuilder<Enemy>();

        foreach (var enemy in enemies)
        {
            var definition = EnemyCatalog.Get(enemy.Kind);
            if (enemy.Health <= 0)
            {
                gold += definition.Gold;
                score += definition.Points;
                kills++;
                events.Add(new GameEvent.EnemyKilled(enemy.Id, BastionMap.PositionAt(enemy.Distance), definition.Gold, definition.Points));
                continue;
            }

            var moved = CombatRules.Move(enemy);
            if (moved.Distance >= BastionMap.Length)
            {
                health = Math.Max(0, health - definition.CoreDamage);
                escaped++;
                events.Add(new GameEvent.EnemyEscaped(enemy.Id, definition.CoreDamage));
            }
            else
            {
                survivors.Add(moved);
            }
        }

        var next = state with
        {
            Enemies = survivors.ToImmutable(), Towers = towers, Gold = gold, Score = score,
            Kills = kills, Escaped = escaped, CoreHealth = health,
            NextEnemyId = nextId, NextSpawnIndex = nextSpawn, WaveTick = state.WaveTick + 1
        };

        if (health == 0)
        {
            next = next with { Phase = GamePhase.Lost, IsPaused = false };
            events.Add(new GameEvent.GameEnded(false, next.Score));
        }
        else if (next.PendingEnemies == 0 && next.Enemies.IsEmpty)
        {
            var waveGold = GameRules.WaveGold(next.WaveNumber);
            var waveScore = GameRules.WaveScore(next.WaveNumber);
            var won = next.WaveNumber == GameRules.TotalWaves;
            next = next with
            {
                Phase = won ? GamePhase.Won : GamePhase.Planning,
                WavesCleared = next.WaveNumber,
                Gold = next.Gold + waveGold,
                Score = next.Score + waveScore + (won ? GameRules.VictoryBonus + health * GameRules.HealthBonus : 0),
                Schedule = [], NextSpawnIndex = 0, IsPaused = false
            };
            events.Add(new GameEvent.WaveCleared(next.WaveNumber, waveGold, waveScore));
            if (won)
                events.Add(new GameEvent.GameEnded(true, next.Score));
        }

        return new(next, events.ToImmutable());
    }

    private static ImmutableArray<Tower> FireTowers(
        ImmutableArray<Tower> towers, List<Enemy> enemies, ImmutableArray<GameEvent>.Builder events)
    {
        var updated = towers.ToBuilder();
        for (var i = 0; i < towers.Length; i++)
        {
            var tower = towers[i] with { CooldownTicks = Math.Max(0, towers[i].CooldownTicks - 1) };
            updated[i] = tower;
            if (tower.CooldownTicks > 0)
                continue;

            var stats = TowerCatalog.Get(tower.Kind).AtLevel(tower.Level);
            var position = BastionMap.Pads.First(pad => pad.Id == tower.PadId).Position;
            var target = CombatRules.SelectTarget(enemies, position, stats.Range, CombatRules.RankBy(tower.Priority, position));
            target.Match(enemy =>
            {
                var impact = BastionMap.PositionAt(enemy.Distance);
                for (var index = 0; index < enemies.Count; index++)
                {
                    var candidate = enemies[index];
                    var inSplash = stats.SplashRadius > 0 &&
                        impact.DistanceSquaredTo(BastionMap.PositionAt(candidate.Distance)) <= stats.SplashRadius * stats.SplashRadius;
                    if (candidate.Health > 0 && (candidate.Id == enemy.Id || inSplash))
                        enemies[index] = CombatRules.ApplyHit(candidate, stats);
                }
                updated[i] = tower with { CooldownTicks = stats.FireIntervalTicks };
                events.Add(new GameEvent.ShotFired(tower.PadId, tower.Kind, impact));
                return true;
            }, () => false);
        }
        return updated.ToImmutable();
    }

    private static Result<GameTransition, GameError> Success(GameState state, params GameEvent[] events) =>
        Result<GameTransition, GameError>.Ok(new(state, [.. events]));

    private static Result<GameTransition, GameError> Failure(GameErrorCode code, string message) =>
        Result<GameTransition, GameError>.Fail(new(code, message));
}
