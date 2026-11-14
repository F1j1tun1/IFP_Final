using ChronoBastion.Application;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;

namespace ChronoBastion.Tests;

public sealed class HighScoreTests
{
    [Fact]
    public void Valid_records_round_trip()
    {
        var score = new HighScore(1, 12000, 39, true);
        var parsed = Assert.IsType<Result<Option<HighScore>, string>.Success>(HighScores.Parse(HighScores.Serialize(score)));
        Assert.Equal(score, parsed.Value.Match(value => value, () => throw new Exception("Missing record.")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Missing_records_are_valid_empty_values(string? json)
    {
        var parsed = Assert.IsType<Result<Option<HighScore>, string>.Success>(HighScores.Parse(json));
        Assert.False(parsed.Value.IsSome);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"Version\":1}")]
    [InlineData("{\"Version\":1,\"Score\":10,\"WavesCleared\":39,\"Won\":false}")]
    [InlineData("{\"Version\":2,\"Score\":10,\"WavesCleared\":0,\"Won\":false}")]
    [InlineData("{\"Version\":1,\"Score\":-1,\"WavesCleared\":0,\"Won\":false}")]
    [InlineData("{\"Version\":1,\"Score\":10,\"WavesCleared\":40,\"Won\":false}")]
    [InlineData("{\"Version\":1,\"Score\":10,\"WavesCleared\":1,\"Won\":true}")]
    [InlineData("{\"Version\":1,\"Score\":\"bad\"}")]
    public void Corrupted_or_incompatible_records_return_an_error(string json) =>
        Assert.False(HighScores.Parse(json).IsSuccess);

    [Fact]
    public void Oversized_records_are_rejected() => Assert.False(HighScores.Parse(new string('x', 4097)).IsSuccess);

    [Fact]
    public void An_unfinished_run_is_not_a_record()
    {
        Assert.False(HighScores.FromFinishedGame(new GameState { Score = 100 }).IsSome);
        Assert.True(HighScores.FromFinishedGame(new GameState { Phase = GamePhase.Lost }).IsSome);
    }

    [Fact]
    public void Worse_scores_never_replace_a_record_and_ties_use_waves_cleared()
    {
        var previous = new HighScore(1, 300, 2, false);
        Assert.Equal(previous, HighScores.Best(Option<HighScore>.Some(previous), new(1, 299, 3, false)));
        Assert.Equal(3, HighScores.Best(Option<HighScore>.Some(previous), new(1, 300, 3, false)).WavesCleared);
        Assert.Equal(previous, HighScores.Best(Option<HighScore>.None, previous));
    }
}
