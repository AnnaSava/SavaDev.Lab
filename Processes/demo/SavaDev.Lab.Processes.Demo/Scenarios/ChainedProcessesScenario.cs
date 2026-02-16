using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Extensions;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates sequential execution of multiple
/// external processes where each step depends
/// on the successful completion of the previous one.
/// </summary>
/// <remarks>
/// This scenario illustrates a simple process chain:
/// <list type="number">
/// <item>The first process produces standard output.</item>
/// <item>The second process runs only if the first succeeds.</item>
/// <item>The chain stops immediately if any process fails.</item>
/// </list>
///
/// The scenario models a common orchestration pattern
/// used in build pipelines, migration workflows,
/// and deployment scripts.
/// </remarks>
public sealed class ChainedProcessesScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Chained processes";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var launcher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new ConsoleContextualObserver();
        var output = new ProcessOutputHandling(observer);

        Console.WriteLine("Starting chained process execution...");
        Console.WriteLine();

        // Step 1
        var firstRequest = CreateFirstRequest();

        Console.WriteLine("Step 1: Running first process...");
        Console.WriteLine();

        var firstResult = await launcher.RunAsync(
            firstRequest,
            output,
            ct);

        if (!firstResult.IsSuccess())
        {
            Console.WriteLine();
            Console.WriteLine("Step 1 failed. Chain execution aborted.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Step 1 completed successfully.");
        Console.WriteLine();

        // Step 2
        var secondRequest = CreateSecondRequest();

        Console.WriteLine("Step 2: Running second process...");
        Console.WriteLine();

        var secondResult = await launcher.RunAsync(
            secondRequest,
            output,
            ct);

        if (!secondResult.IsSuccess())
        {
            Console.WriteLine();
            Console.WriteLine("Step 2 failed. Chain execution aborted.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("Step 2 completed successfully.");
        Console.WriteLine();
        Console.WriteLine("=== Chain completed successfully ===");
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> for the first step
    /// in the chained process execution.
    /// </summary>
    /// <remarks>
    /// The request launches a simple shell command that writes
    /// a single line to the standard output stream and exits
    /// successfully.
    ///
    /// The command is intentionally minimal and deterministic,
    /// serving only to demonstrate a successful process execution
    /// that allows the chain to proceed to the next step.
    ///
    /// The concrete shell and command syntax depend on the
    /// operating system in order to use a native command
    /// interpreter.
    /// </remarks>
    /// <returns>
    /// A <see cref="ProcessRequest"/> representing the first
    /// process in the execution chain.
    /// </returns>
    private static ProcessRequest CreateFirstRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo First process executed"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = "-c \"echo First process executed\""
        };
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> for the second step
    /// in the chained process execution.
    /// </summary>
    /// <remarks>
    /// The request launches a simple shell command that writes
    /// a single line to the standard output stream and exits
    /// successfully.
    ///
    /// This step is executed only if the first process in the
    /// chain completes successfully, illustrating conditional
    /// sequencing at the scenario level rather than within
    /// the process launcher itself.
    ///
    /// As with the first step, the command is constructed
    /// differently depending on the operating system to use
    /// a native shell.
    /// </remarks>
    /// <returns>
    /// A <see cref="ProcessRequest"/> representing the second
    /// process in the execution chain.
    /// </returns>
    private static ProcessRequest CreateSecondRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo Second process executed"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = "-c \"echo Second process executed\""
        };
    }

}

