using System.Collections.Immutable;
using System.Text.Json;
using ChronoBastion.Core;
using ChronoBastion.Core.Content;
using ChronoBastion.Core.Model;
using ChronoBastion.Core.Rules;

namespace ChronoBastion.Tests;

public sealed class CombatTests
{
    [Fact]
    public void Armor_reduces_damage_without_mutating_the_enemy()
    {
        var enemy = EnemyCatalog.Spawn(EnemyKind.Armored, 1, 1);
        var stats = TowerCatalog.Get(TowerKind.Pulse).AtLevel(1);
        var hit = CombatRules.ApplyHit(enemy, stats);
        Assert.Equal(95 - 12 * 0.65, hit.Health, 6);
        Assert.Equal(95, enemy.Health);
    }

    [Fact]
    public void Slow_expires_and_does_not_multiply_with_other_frost_hits()
    {
        var enemy = EnemyCatalog.Spawn(EnemyKind.Grunt, 1, 1);
        var weak = TowerCatalog.Get(TowerKind.Frost).AtLevel(1);
        var strong = TowerCatalog.Get(TowerKind.Frost).AtLevel(3);
        var slowed = CombatRules.ApplyHit(CombatRules.ApplyHit(enemy, strong), weak);
        Assert.Equal(0.4, slowed.SlowMultiplier);
        Assert.Equal(45, slowed.SlowTicks);
        Assert.Equal(1.2, CombatRules.Move(slowed).Distance, 6);
        var expiring = slowed with { SlowTicks = 1 };
        var expired = CombatRules.Move(expiring);
        Assert.Equal(1, expired.SlowMultiplier);
        Assert.Equal(0, expired.SlowTicks);
        Assert.Equal(3, CombatRules.Move(expired).Distance - expired.Distance, 6);
    }

    [Fact]
    public void Target_priorities_and_ties_are_deterministic()
    {
        var origin = new Position(120, 130);
        Enemy[] enemies = [new(2, EnemyKind.Grunt, 20, 35, 160), new(1, EnemyKind.Grunt, 30, 35, 100)];
        Assert.Equal(2, Choose(TargetPriority.First));
        Assert.Equal(1, Choose(TargetPriority.Strongest));
        Assert.Equal(1, Choose(TargetPriority.Nearest));
        var tied = enemies.Select(enemy => enemy with { Distance = 100 }).Reverse();
        Assert.Equal(1, CombatRules.SelectTarget(tied, origin, 200, _ => 1).Match(e => e.Id, () => -1));
        Assert.False(CombatRules.SelectTarget(enemies, new(900, 0), 1, e => e.Distance).IsSome);

        int Choose(TargetPriority priority) => CombatRules.SelectTarget(enemies, origin, 200,
            CombatRules.RankBy(priority, origin)).Match(e => e.Id, () => -1);
    }

    [Fact]
    public void Mortar_hits_nearby_enemies_but_not_distant_ones()
    {
        var enemies = new[] { 100.0, 130.0, 700.0 }.Select((distance, i) =>
            new Enemy(i + 1, EnemyKind.Grunt, 100, 100, distance)).ToArray();
        var state = TestGame.Apply(TestGame.Combat(enemies), new GameCommand.BuildTower(1, TowerKind.Mortar));
        var tick = GameEngine.Tick(state);
        Assert.Equal(72, tick.State.Enemies[0].Health);
        Assert.Equal(72, tick.State.Enemies[1].Health);
        Assert.Equal(100, tick.State.Enemies[2].Health);
        Assert.Single(tick.Events.OfType<GameEvent.ShotFired>());
    }

    [Fact]
    public void Kills_pay_once_and_dead_targets_cannot_fire_another_reward()
    {
        var state = TestGame.Combat(new Enemy(1, EnemyKind.Grunt, 1, 35, 240));
        state = TestGame.Apply(state, new GameCommand.BuildTower(1, TowerKind.Pulse));
        state = TestGame.Apply(state, new GameCommand.BuildTower(2, TowerKind.Pulse));
        var result = GameEngine.Tick(state);
        Assert.Equal(state.Gold + 4, result.State.Gold);
        Assert.Equal(20, result.State.Score);
        Assert.Equal(1, result.State.Kills);
        Assert.Empty(result.State.Enemies);
        Assert.Single(result.Events.OfType<GameEvent.EnemyKilled>());
        Assert.Equal(result.State.Gold, GameEngine.Tick(result.State).State.Gold);
    }

    [Fact]
    public void Cooldown_limits_shots_to_the_configured_interval()
    {
        var state = TestGame.Combat(new Enemy(1, EnemyKind.Grunt, 1000, 1000, 100));
        state = TestGame.Apply(state, new GameCommand.BuildTower(1, TowerKind.Pulse));
        Assert.Single(GameEngine.Tick(state).Events.OfType<GameEvent.ShotFired>());
        state = GameEngine.Tick(state).State;
        for (var tick = 1; tick < 8; tick++)
        {
            var result = GameEngine.Tick(state);
            Assert.Empty(result.Events.OfType<GameEvent.ShotFired>());
            state = result.State;
        }
        Assert.Single(GameEngine.Tick(state).Events.OfType<GameEvent.ShotFired>());
    }

    [Fact]
    public void Core_destruction_takes_precedence_over_wave_completion()
    {
        var state = TestGame.Combat(new Enemy(1, EnemyKind.Boss, 10, 10, BastionMap.Length - 0.1))
            with { Schedule = [], WaveNumber = 10 };
        var result = GameEngine.Tick(state);
        Assert.Equal(GamePhase.Lost, result.State.Phase);
        Assert.Equal(0, result.State.CoreHealth);
        Assert.Equal(0, result.State.Score);
        Assert.Empty(result.Events.OfType<GameEvent.WaveCleared>());
        Assert.False(Assert.Single(result.Events.OfType<GameEvent.GameEnded>()).Won);
        Assert.Empty(GameEngine.Tick(result.State).Events);
    }

    [Fact]
    public void A_completed_wave_rewards_once_and_does_not_automatically_start_the_next()
    {
        var state = TestGame.Combat() with { Schedule = [], WaveNumber = 3 };
        var result = GameEngine.Tick(state);
        Assert.Equal(GamePhase.Planning, result.State.Phase);
        Assert.Equal(3, result.State.WavesCleared);
        Assert.Equal(313, result.State.Gold);
        Assert.Equal(300, result.State.Score);
        Assert.Equal(result.State, GameEngine.Tick(result.State).State);
        Assert.Empty(GameEngine.Tick(result.State).Events);
    }

    [Fact]
    public void Final_wave_ends_the_game_and_awards_the_health_bonus_once()
    {
        var state = TestGame.Combat() with { Schedule = [], StageNumber = 3, WaveNumber = 39, CoreHealth = 7, Score = 50 };
        var result = GameEngine.Tick(state);
        Assert.Equal(GamePhase.Won, result.State.Phase);
        Assert.Equal(39, result.State.WavesCleared);
        Assert.Equal(50 + 3900 + 1000 + 350, result.State.Score);
        Assert.True(Assert.Single(result.Events.OfType<GameEvent.GameEnded>()).Won);
        Assert.Equal(result.State.Score, GameEngine.Tick(result.State).State.Score);
    }

    [Fact]
    public void Future_spawns_prevent_early_wave_completion()
    {
        var state = TestGame.Combat();
        var next = GameEngine.Tick(state).State;
        Assert.Equal(GamePhase.Combat, next.Phase);
        Assert.Empty(next.Enemies);
    }

    [Fact]
    public void Paused_ticks_do_nothing()
    {
        var state = TestGame.Combat() with { IsPaused = true };
        Assert.Same(state, GameEngine.Tick(state).State);
        Assert.Empty(GameEngine.Tick(state).Events);
    }

    [Fact]
    public void Identical_inputs_produce_identical_outputs_and_preserve_snapshots()
    {
        var state = TestGame.Apply(GameEngine.NewGame(), new GameCommand.BuildTower(1, TowerKind.Pulse));
        state = TestGame.Apply(state, new GameCommand.StartWave());
        var before = JsonSerializer.Serialize(state);
        var first = GameEngine.Tick(state);
        var second = GameEngine.Tick(state);
        Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        Assert.Equal(before, JsonSerializer.Serialize(state));
    }

    [Fact]
    public void No_defense_eventually_loses_and_never_awards_kill_gold()
    {
        var state = GameEngine.NewGame();
        var ticks = 0;
        while (!state.IsFinished && ticks++ < 100000)
        {
            state = state.Phase == GamePhase.Planning
                ? TestGame.Apply(state, new GameCommand.StartWave())
                : GameEngine.Tick(state).State;
        }
        Assert.Equal(GamePhase.Lost, state.Phase);
        Assert.Equal(0, state.Kills);
        Assert.Equal(300 + Enumerable.Range(1, state.WavesCleared).Sum(GameRules.WaveGold), state.Gold);
        Assert.True(state.Escaped >= 20);
    }
}
