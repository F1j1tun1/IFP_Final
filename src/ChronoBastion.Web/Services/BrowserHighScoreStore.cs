using ChronoBastion.Application;
using ChronoBastion.Core.Functional;
using ChronoBastion.Core.Model;
using Microsoft.JSInterop;

namespace ChronoBastion.Web.Services;

public sealed class BrowserHighScoreStore(IJSRuntime js)
{
    public Option<HighScore> Record { get; private set; }
    public string? Warning { get; private set; }
    private bool loaded;

    public async Task LoadAsync()
    {
        if (loaded) return;
        loaded = true;
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", HighScores.StorageKey);
            Record = HighScores.Parse(json).Match(record => record, message =>
            {
                Warning = message + " You can still play; a new finished run can replace it.";
                return Option<HighScore>.None;
            });
        }
        catch (JSException) { Warning = "Record storage is unavailable. You can still play on this visit."; }
    }

    public async Task SaveFinishedAsync(GameState state)
    {
        var candidate = HighScores.FromFinishedGame(state);
        if (!candidate.IsSome) return;
        var best = candidate.Match(score => HighScores.Best(Record, score), () => throw new InvalidOperationException());
        if (Record.Match(current => current == best, () => false)) return;
        Record = Option<HighScore>.Some(best);
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", HighScores.StorageKey, HighScores.Serialize(best));
            Warning = null;
        }
        catch (JSException) { Warning = "Your record could not be saved on this device. It remains available for this visit."; }
    }
}
