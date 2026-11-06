using ChronoBastion.Core.Model;
using ChronoBastion.Simulator;

var scenario = args.FirstOrDefault() ?? "all";
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
