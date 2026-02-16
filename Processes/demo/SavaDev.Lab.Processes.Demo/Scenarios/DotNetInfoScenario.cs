using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Launchers.DotNet;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates executing a real <c>dotnet</c> command
/// using <see cref="DotNetProcessLauncher"/>.
/// </summary>
/// <remarks>
/// This scenario shows how <see cref="DotNetProcessLauncher"/>
/// can be used as a convenience wrapper around
/// <see cref="IProcessLauncher"/> to execute <c>dotnet</c>
/// CLI commands.
///
/// The scenario runs <c>dotnet --info</c>, which outputs
/// information about the installed .NET SDKs and runtimes.
/// This makes it a safe, read-only command suitable for
/// demonstration purposes.
///
/// The scenario highlights that:
/// <list type="bullet">
/// <item>The <c>dotnet</c> executable name is fixed by the launcher.</item>
/// <item>Command arguments are passed verbatim.</item>
/// <item>Output handling works the same way as with a generic process launcher.</item>
/// </list>
/// </remarks>
public sealed class DotNetInfoScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "dotnet --info";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var baseLauncher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var dotnetLauncher = new DotNetProcessLauncher(baseLauncher);

        var observer = new ConsoleContextualObserver();
        var output = new ProcessOutputHandling(observer);

        Console.WriteLine("Running 'dotnet --info'...");
        Console.WriteLine();

        var result = await dotnetLauncher.RunAsync(
            "--info",
            output: output,
            ct: ct);

        Console.WriteLine();
        Console.WriteLine("=== dotnet command completed ===");
        Console.WriteLine($"Exit code: {result.ExitCode}");

        if (!string.IsNullOrWhiteSpace(result.StandardError))
        {
            Console.WriteLine();
            Console.WriteLine("Standard error:");
            Console.WriteLine(result.StandardError);
        }
    }
}
