using System.Text.Json;
using System.Text.Json.Serialization;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Application;

public sealed record HighScore(
    [property: JsonRequired] int Version,
    [property: JsonRequired] int Score,
    [property: JsonRequired] int WavesCleared,
    [property: JsonRequired] bool Won);

public static class HighScores
{
    public const string StorageKey = "chrono-bastion.high-score.v1";

    public static Option<HighScore> FromFinishedGame(GameState state) => state.IsFinished
        ? Option<HighScore>.Some(new(1, state.Score, state.WavesCleared, state.Phase == GamePhase.Won))
        : Option<HighScore>.None;

    public static HighScore Best(Option<HighScore> previous, HighScore candidate) => previous.Match(
        current => candidate.Score > current.Score ||
            (candidate.Score == current.Score && candidate.WavesCleared > current.WavesCleared)
                ? candidate : current,
        () => candidate);

    public static string Serialize(HighScore score) => JsonSerializer.Serialize(score, HighScoreJsonContext.Default.HighScore);

    public static Result<Option<HighScore>, string> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Result<Option<HighScore>, string>.Ok(Option<HighScore>.None);
        if (json.Length > 4096)
            return Result<Option<HighScore>, string>.Fail("The saved record is too large.");

        try
        {
            var score = JsonSerializer.Deserialize(json, HighScoreJsonContext.Default.HighScore);
            if (score is null || score.Version != 1 || score.Score < 0 ||
                score.WavesCleared is < 0 or > GameRules.TotalWaves ||
                score.Won != (score.WavesCleared == GameRules.TotalWaves))
                return Result<Option<HighScore>, string>.Fail("The saved record is invalid or from an unsupported version.");
            return Result<Option<HighScore>, string>.Ok(Option<HighScore>.Some(score));
        }
        catch (JsonException)
        {
            return Result<Option<HighScore>, string>.Fail("The saved record could not be read.");
        }
    }
}

[JsonSerializable(typeof(HighScore))]
internal partial class HighScoreJsonContext : JsonSerializerContext;
