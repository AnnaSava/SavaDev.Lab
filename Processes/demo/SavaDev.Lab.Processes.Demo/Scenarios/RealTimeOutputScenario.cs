using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates real-time process execution with
/// streaming standard output.
/// </summary>
/// <remarks>
/// This scenario runs a simple command that produces
/// delayed output and forwards each output line to the console
/// as it is produced by the process.
/// 
/// The example highlights the difference between:
/// <list type="bullet">
/// <item>real-time output observation during execution,</item>
/// <item>and the final aggregated process result.</item>
/// </list>
/// </remarks>
public sealed class RealTimeOutputScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Real-time output";

    /// <summary>
    /// Runs the real-time output demonstration.
    /// </summary>
    /// <param name="ct">
    /// A cancellation token that can be used to cancel
    /// the process execution.
    /// </param>
    public async Task RunAsync(CancellationToken ct)
    {
        var processLauncher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new ConsoleContextualObserver();
        var output = new ProcessOutputHandling(observer);

        ProcessRequest request;

        if (OperatingSystem.IsWindows())
        {
            request = new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo Starting... & timeout /t 2 & echo Finished."
            };
        }
        else
        {
            request = new ProcessRequest
            {
                FileName = "/bin/sh",
                Arguments = "-c \"echo Starting...; sleep 2; echo Finished.\""
            };
        }

        var result = await processLauncher.RunAsync(
            request,
            output,
            ct);

        Console.WriteLine();
        Console.WriteLine("=== Process completed ===");
        Console.WriteLine($"Exit code: {result.ExitCode}");
    }
}
