using ChronoBastion.Application;
using ChronoBastion.Core.Model;
using ChronoBastion.Web.Services;
using Microsoft.JSInterop;

namespace ChronoBastion.Web.Tests;

public sealed class StorageTests
{
    [Fact]
    public async Task LoadsOnceAndDoesNotSaveUnfinishedRuns()
    {
        var js = new StorageJs();
        var store = new BrowserHighScoreStore(js);
        await store.LoadAsync(); await store.LoadAsync();
        await store.SaveFinishedAsync(new GameState());
        Assert.Equal(1, js.Reads); Assert.Equal(0, js.Writes);
    }

    [Theory]
    [InlineData("{broken")]
    [InlineData("{\"Version\":99,\"Score\":1,\"WavesCleared\":0,\"Won\":false}")]
    public async Task CorruptedRecordDoesNotPreventNewRecord(string data)
    {
        var js = new StorageJs { Data = data };
        var store = new BrowserHighScoreStore(js);
        await store.LoadAsync();
        Assert.NotNull(store.Warning); Assert.False(store.Record.IsSome);
        await store.SaveFinishedAsync(new GameState { Phase = GamePhase.Lost, Score = 300, WavesCleared = 2 });
        Assert.Null(store.Warning);
        Assert.True(HighScores.Parse(js.Data).IsSuccess);
    }

    [Fact]
    public async Task BlockedStorageRetainsBestRecordInMemory()
    {
        var store = new BrowserHighScoreStore(new StorageJs { Blocked = true });
        await store.LoadAsync();
        await store.SaveFinishedAsync(new GameState { Phase = GamePhase.Lost, Score = 300, WavesCleared = 2 });
        await store.SaveFinishedAsync(new GameState { Phase = GamePhase.Lost, Score = 100, WavesCleared = 1 });
        Assert.Equal(300, store.Record.Match(r => r.Score, () => 0));
        Assert.NotNull(store.Warning);
    }

    private sealed class StorageJs : IJSRuntime
    {
        public string? Data { get; set; }
        public bool Blocked { get; set; }
        public int Reads { get; private set; }
        public int Writes { get; private set; }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, default, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (Blocked) throw new JSException("Storage denied");
            if (identifier == "localStorage.getItem") { Reads++; return ValueTask.FromResult((TValue)(object?)Data!); }
            Writes++; Data = (string)args![1]!;
            return ValueTask.FromResult(default(TValue)!);
        }
    }
}
