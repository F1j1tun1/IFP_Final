using ChronoBastion.Application;
using ChronoBastion.Core;
using ChronoBastion.Core.Content;
using ChronoBastion.Core.Model;
using ChronoBastion.Core.Rules;

namespace ChronoBastion.Tests;

public sealed class EnemyAbilityTests
{
    [Theory]
    [InlineData(1, 180, 320)]
    [InlineData(2, 200, 340)]
    [InlineData(3, 210, 350)]
    public void Healing_uses_world_distance_on_the_current_map(int stage, double casterDistance, double allyDistance)
    {
        var wave = (stage - 1) * 12 + 1;
        var caster = EnemyCatalog.Spawn(EnemyKind.Shaman, 1, wave) with
            { Distance = casterDistance, AbilityTicks = EnemyAbilities.HealInterval - 1 };
        var ally = EnemyCatalog.Spawn(EnemyKind.Orc, 2, wave) with { Health = 1, Distance = allyDistance };
        var state = TestGame.Combat(caster, ally) with { StageNumber = stage, WaveNumber = wave, NextEnemyId = 3 };
        var result = GameEngine.Tick(state);
        Assert.Equal(1 + ally.MaxHealth * EnemyAbilities.HealFraction, result.State.Enemies[1].Health, 6);
    }

    private static Enemy Ready(EnemyKind kind, int id, double distance = 100) =>
        EnemyCatalog.Spawn(kind, id, 39) with { Distance = distance, AbilityTicks = EnemyAbilities.Interval(kind) - 1 };

    private static GameState Battle(params Enemy[] enemies) => TestGame.Combat(enemies) with
    {
        StageNumber = 3, WaveNumber = 39, WavesCleared = 38,
        NextEnemyId = enemies.Max(e => e.Id) + 1
    };

    [Fact]
    public void Boss_summons_three_unique_goblins_on_its_current_road()
    {
        var boss = Ready(EnemyKind.Boss, 8, 600);
        var original = Battle(boss);
        var result = GameEngine.Tick(original);
        Assert.Single(original.Enemies);
        Assert.Equal(0, result.State.Enemies[0].AbilityTicks);
        Assert.Equal(new[] { 9, 10, 11 }, result.State.Enemies.Skip(1).Select(e => e.Id));
        Assert.All(result.State.Enemies.Skip(1), e =>
        {
            Assert.Equal(EnemyKind.Grunt, e.Kind);
            Assert.InRange(e.Distance, 560, 600);
        });
        Assert.Equal(Campaign.Get(3).Map.PositionAt(600), Assert.Single(result.Events.OfType<GameEvent.Summoned>()).Position);
        for (var i = 0; i < EnemyAbilities.SummonInterval - 1; i++) result = GameEngine.Tick(result.State);
        Assert.Equal(4, result.State.Enemies.Length);
        Assert.Equal(7, GameEngine.Tick(result.State).State.Enemies.Length);
    }

    [Fact]
    public void Summons_respect_population_limit()
    {
        var enemies = Enumerable.Range(1, EnemyAbilities.MaxLivingEnemies - 2)
            .Select(id => EnemyCatalog.Spawn(EnemyKind.Grunt, id, 39)).ToList();
        enemies.Add(Ready(EnemyKind.Boss, 119));
        var result = GameEngine.Tick(Battle([.. enemies]));
        Assert.Equal(120, result.State.Enemies.Length);
        Assert.Equal(120, result.State.Enemies.Select(e => e.Id).Distinct().Count());
    }

    [Fact]
    public void Healing_is_limited_to_three_wounded_living_allies_and_cannot_overheal()
    {
        var shaman = Ready(EnemyKind.Shaman, 1) with { Health = 10 };
        var allies = Enumerable.Range(2, 4).Select(id =>
        {
            var ally = EnemyCatalog.Spawn(EnemyKind.Orc, id, 39);
            return ally with { Distance = 100, Health = id == 2 ? 1 : ally.MaxHealth - 1 };
        }).ToArray();
        var boss = Ready(EnemyKind.Boss, 6) with { Health = 1, AbilityTicks = 0 };
        var dead = EnemyCatalog.Spawn(EnemyKind.Grunt, 7, 39) with { Health = 0, Distance = 100 };
        var far = EnemyCatalog.Spawn(EnemyKind.Orc, 8, 39) with { Health = 1, Distance = 500 };
        var result = GameEngine.Tick(Battle([shaman, .. allies, boss, dead, far]));
        Assert.Equal(3, result.Events.OfType<GameEvent.EnemyHealed>().Count());
        Assert.Equal(10, result.State.Enemies.Single(e => e.Id == 1).Health);
        Assert.Equal(1, result.State.Enemies.Single(e => e.Id == 6).Health);
        Assert.Equal(1, result.State.Enemies.Single(e => e.Id == 8).Health);
        Assert.DoesNotContain(result.State.Enemies, e => e.Id == 7);
        Assert.All(result.State.Enemies, e => Assert.True(e.Health <= e.MaxHealth));
        Assert.Equal(allies[3].Health, result.State.Enemies.Single(e => e.Id == 5).Health);
        Assert.Equal(allies[1].MaxHealth, result.State.Enemies.Single(e => e.Id == 3).Health);
    }

    [Theory]
    [InlineData(EnemyKind.Boss)]
    [InlineData(EnemyKind.Shaman)]
    public void A_caster_killed_this_tick_cannot_use_its_ability(EnemyKind kind)
    {
        var caster = Ready(kind, 1, 130) with { Health = 1 };
        var state = Battle(caster) with { Towers = [new(1, TowerKind.Pulse, 1, 90, 0, TargetPriority.First)] };
        var result = GameEngine.Tick(state);
        Assert.Empty(result.State.Enemies);
        Assert.Empty(result.Events.OfType<GameEvent.Summoned>());
        Assert.Empty(result.Events.OfType<GameEvent.EnemyHealed>());
        Assert.Single(result.Events.OfType<GameEvent.EnemyKilled>());
        Assert.Empty(GameEngine.Tick(result.State).Events.OfType<GameEvent.EnemyKilled>());
    }

    [Fact]
    public void Killing_the_boss_does_not_end_the_wave_while_summons_survive()
    {
        var state = GameEngine.Tick(Battle(Ready(EnemyKind.Boss, 1))).State;
        state = state with { Schedule = [], Enemies = state.Enemies.SetItem(0, state.Enemies[0] with { Health = 0 }) };
        var result = GameEngine.Tick(state);
        Assert.Equal(GamePhase.Combat, result.State.Phase);
        Assert.Equal(3, result.State.Enemies.Length);
        Assert.Empty(result.Events.OfType<GameEvent.GameEnded>());
    }

    [Fact]
    public void Saved_ability_timers_continue_identically_after_reload()
    {
        var state = Battle(Ready(EnemyKind.Boss, 1), Ready(EnemyKind.Shaman, 2)) with
        {
            Schedule = WavePlanner.Schedule(39), NextSpawnIndex = WavePlanner.Schedule(39).Length, WaveTick = 2000
        };
        var session = new GameSession();
        Assert.True(session.Restore(SavedRuns.Serialize(state)).IsSuccess);
        session.Advance(TimeSpan.FromMilliseconds(200));
        Assert.Equal(state.Enemies.ToArray(), session.State.Enemies.ToArray());
        session.Dispatch(new GameCommand.SetPaused(false));
        for (var i = 0; i < 400; i++)
        {
            state = GameEngine.Tick(state).State;
            session.Advance(TimeSpan.FromMilliseconds(50));
            Assert.Equal(SavedRuns.Serialize(state), SavedRuns.Serialize(session.State));
        }
    }
}
