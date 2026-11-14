using System.Text.Json;
using System.Text.Json.Serialization;
using ChronoBastion.Core.Content;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Application;

public sealed record SavedRun([property: JsonRequired] int Version, [property: JsonRequired] GameState State);

public static class SavedRuns
{
    public const string StorageKey = "thornwatch.run.v1";
    public static string Serialize(GameState state) =>
        JsonSerializer.Serialize(new SavedRun(1, state), SavedRunJsonContext.Default.SavedRun);

    public static Result<GameState, string> Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json.Length > 131072)
            return Invalid();
        try
        {
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("State", out var data) || data.ValueKind != JsonValueKind.Object)
                return Invalid();
            string[] required = ["StageNumber", "WaveNumber", "WavesCleared", "Gold", "CoreHealth", "Phase",
                "Score", "Kills", "Escaped", "WaveTick", "NextEnemyId", "NextSpawnIndex", "SpeedMultiplier",
                "IsPaused", "Towers", "Enemies", "Schedule"];
            if (required.Any(name => !data.TryGetProperty(name, out _))) return Invalid();
            var saved = JsonSerializer.Deserialize(json, SavedRunJsonContext.Default.SavedRun);
            return saved is { Version: 1, State: { } state } && Valid(state)
                ? Result<GameState, string>.Ok(state)
                : Invalid();
        }
        catch (Exception error) when (error is JsonException or ArgumentException or InvalidOperationException or OverflowException)
        { return Invalid(); }
    }

    private static Result<GameState, string> Invalid() =>
        Result<GameState, string>.Fail("This saved journey is damaged or from another version. Start a new journey.");

    private static bool Valid(GameState s)
    {
        if (!Enum.IsDefined(s.Phase) || s.IsFinished || s.StageNumber is < 1 or > 3 ||
            s.CoreHealth is < 1 or > GameRules.StartingHealth || s.Gold is < 0 or > 500000 ||
            s.Score is < 0 or > 2000000 || s.Kills is < 0 or > 100000 || s.Escaped is < 0 or > 20 ||
            s.SpeedMultiplier is not (1 or 2) || s.WaveTick is < 0 or > 120000 ||
            s.NextEnemyId is < 1 or > 100000 || s.Towers.IsDefault || s.Enemies.IsDefault || s.Schedule.IsDefault ||
            s.Towers.Length > 10 || s.Enemies.Length > 200 || s.Schedule.Length > 200 ||
            s.Towers.Any(t => t is null) || s.Enemies.Any(e => e is null) || s.Schedule.Any(s => s is null))
            return false;
        var start = (s.StageNumber - 1) * 12;
        var end = start + s.StageWaves;
        if (s.WaveNumber < start || s.WaveNumber > end) return false;
        if (s.Phase == GamePhase.Combat)
        {
            if (s.WaveNumber <= start || s.WavesCleared != s.WaveNumber - 1 ||
                !s.Schedule.SequenceEqual(WavePlanner.Schedule(s.WaveNumber)) ||
                s.NextSpawnIndex < 0 || s.NextSpawnIndex > s.Schedule.Length) return false;
            if (s.Schedule.Take(s.NextSpawnIndex).Any(spawn => spawn.AtTick >= s.WaveTick) ||
                s.Schedule.Skip(s.NextSpawnIndex).Any(spawn => spawn.AtTick < s.WaveTick)) return false;
        }
        else
        {
            if (s.WavesCleared != s.WaveNumber || !s.Enemies.IsEmpty || !s.Schedule.IsEmpty || s.NextSpawnIndex != 0)
                return false;
            if (s.Phase == GamePhase.Planning && s.WaveNumber == end) return false;
            if (s.Phase == GamePhase.StageCleared && (s.StageNumber == 3 || s.WaveNumber != end)) return false;
        }
        if (s.Towers.Select(t => t.PadId).Distinct().Count() != s.Towers.Length ||
            !s.Towers.Select(t => t.PadId).SequenceEqual(s.Towers.Select(t => t.PadId).Order())) return false;
        foreach (var tower in s.Towers)
        {
            if (tower.PadId is < 1 or > 10 || !Enum.IsDefined(tower.Kind) || !Enum.IsDefined(tower.Priority) ||
                tower.Level is < 1 or > 3) return false;
            var catalog = TowerCatalog.Get(tower.Kind);
            if (tower.InvestedGold != catalog.Levels.Take(tower.Level).Sum(l => l.Price) ||
                tower.CooldownTicks < 0 || tower.CooldownTicks > 28) return false;
        }
        if (s.Enemies.Select(e => e.Id).Distinct().Count() != s.Enemies.Length) return false;
        foreach (var enemy in s.Enemies)
        {
            if (!Enum.IsDefined(enemy.Kind) || enemy.Id < 1 || enemy.Id >= s.NextEnemyId ||
                !double.IsFinite(enemy.Health) || !double.IsFinite(enemy.MaxHealth) || !double.IsFinite(enemy.Distance) ||
                enemy.Health <= 0 || enemy.Health > enemy.MaxHealth || enemy.Distance < 0 ||
                enemy.Distance >= Campaign.Get(s.StageNumber).Map.Length || enemy.SlowTicks is < 0 or > 45 ||
                enemy.SlowMultiplier is not (1 or 0.4 or 0.5 or 0.6) ||
                (enemy.SlowTicks == 0 && enemy.SlowMultiplier != 1) ||
                Math.Abs(enemy.MaxHealth - EnemyCatalog.Get(enemy.Kind).Health * GameRules.HealthMultiplier(s.WaveNumber)) > 0.001)
                return false;
        }
        return true;
    }
}

[JsonSerializable(typeof(SavedRun))]
internal partial class SavedRunJsonContext : JsonSerializerContext;
