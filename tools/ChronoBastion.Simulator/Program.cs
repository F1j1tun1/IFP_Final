using ChronoBastion.Core.Model;
using ChronoBastion.Simulator;

var scenario = args.FirstOrDefault() ?? "all";
if (scenario == "balance")
{
    Directory.CreateDirectory("artifacts");
    var rows = new List<string> { "Strategy,Wave,Phase,Health,Gold,Earned,Spent,TravelIncome,Towers" };
    foreach (var strategy in new[] { "mixed", "archers", "catapults", "poor-placement", "no-upgrades", "none" })
    {
        var result = DemoScenarios.Run(strategy);
        foreach (var w in result.Waves)
            rows.Add($"{strategy},{w.Number},{w.Phase},{w.Health},{w.Gold},{w.Earned},{w.Spent},{w.TravelIncome},{w.Towers}");
        Console.WriteLine($"{strategy}: {result.FinalState.Phase}, wave {result.FinalState.WaveNumber}, health {result.FinalState.CoreHealth}, gold {result.FinalState.Gold}, kills {result.FinalState.Kills}");
    }
    File.WriteAllLines(args.ElementAtOrDefault(1) ?? "artifacts/balance.csv", rows);
    return 0;
}
if (scenario is not ("all" or "victory" or "defeat"))
{
    Console.Error.WriteLine("Usage: dotnet run --project tools/ChronoBastion.Simulator -- [all|victory|defeat]");
    return 2;
}

foreach (var defend in scenario switch { "victory" => new[] { true }, "defeat" => [false], _ => [true, false] })
{
    var report = DemoScenarios.Run(defend);
    Console.WriteLine(defend ? "DEFENDED RUN" : "UNDEFENDED RUN");
    foreach (var wave in report.Waves)
        Console.WriteLine($"Wave {wave.Number,2}: {wave.Phase,-8} | core {wave.Health,2} | gold {wave.Gold,4} | score {wave.Score,5}");
    Console.WriteLine($"Result: {report.FinalState.Phase}; {report.FinalState.Kills} kills; {report.Ticks} ticks.");
    Console.WriteLine();
    if (report.FinalState.Phase != (defend ? GamePhase.Won : GamePhase.Lost))
        return 1;
}
return 0;
