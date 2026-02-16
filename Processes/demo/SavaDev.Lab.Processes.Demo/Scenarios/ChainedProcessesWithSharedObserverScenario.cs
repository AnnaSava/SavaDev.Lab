using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Extensions;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates sequential execution of multiple processes
/// that share the same set of process observers.
/// </summary>
/// <remarks>
/// This scenario illustrates that the same observer instances
/// can be reused across multiple process executions to collect
/// aggregated information.
///
/// A shared buffered observer is used to accumulate output
/// lines from all steps in the chain, allowing post-execution
/// analysis across process boundaries.
///
/// Observers are provided directly via
/// <see cref="ProcessOutputHandling"/>, without using a
/// composite observer abstraction.
/// </remarks>
public sealed class ChainedProcessesWithSharedObserverScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Chained processes (shared observer)";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var launcher = new ProcessLauncher(new ProcessOutputObserverResolver());

        var bufferedObserver = new BufferedBasicObserver();

        var output = new ProcessOutputHandling(
            new IProcessOutputObserver[]
            {
                new ConsoleContextualObserver(),
                bufferedObserver
            });

        Console.WriteLine("Starting chained processes with shared observers...");
        Console.WriteLine();

        // Step 1
        Console.WriteLine("Step 1: Running first process...");
        Console.WriteLine();

        var firstResult = await launcher.RunAsync(
            CreateFirstRequest(),
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
        Console.WriteLine("Step 2: Running second process...");
        Console.WriteLine();

        var secondResult = await launcher.RunAsync(
            CreateSecondRequest(),
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
        Console.WriteLine();

        PrintSample(
            "Aggregated output from all steps:",
            bufferedObserver.StandardOutputLines);
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> for the first step
    /// of a sequential process execution.
    /// </summary>
    /// <remarks>
    /// The request launches a minimal shell command that writes
    /// a single line to the standard output stream and exits
    /// successfully.
    ///
    /// This step is intended to represent an initial, successful
    /// operation in a process chain, allowing subsequent steps
    /// to execute.
    ///
    /// The command is constructed differently depending on the
    /// operating system to ensure compatibility with the native
    /// shell environment.
    /// </remarks>
    /// <returns>
    /// A <see cref="ProcessRequest"/> representing the first
    /// process in the sequence.
    /// </returns>
    private static ProcessRequest CreateFirstRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo First step output"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = "-c \"echo First step output\""
        };
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> for the second step
    /// of a sequential process execution.
    /// </summary>
    /// <remarks>
    /// The request launches a minimal shell command that writes
    /// a single line to the standard output stream and exits
    /// successfully.
    ///
    /// This step is executed only after the successful completion
    /// of the first step, demonstrating conditional sequencing
    /// at the scenario level rather than within the process launcher.
    ///
    /// As with the first step, the command is tailored to the
    /// operating system to use a native shell.
    /// </remarks>
    /// <returns>
    /// A <see cref="ProcessRequest"/> representing the second
    /// process in the sequence.
    /// </returns>
    private static ProcessRequest CreateSecondRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo Second step output"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = "-c \"echo Second step output\""
        };
    }

    /// <summary>
    /// Prints a subset of the provided output lines to the console.
    /// </summary>
    /// <param name="title">
    /// A descriptive title printed before the output sample.
    /// </param>
    /// <param name="lines">
    /// A collection of output lines from which a limited number
    /// of entries will be displayed.
    /// </param>
    /// <remarks>
    /// This helper method is intended for demonstration purposes.
    /// To keep console output readable, only the first few lines
    /// are printed when the collection exceeds a fixed threshold.
    /// An ellipsis is displayed to indicate omitted output.
    /// </remarks>
    private static void PrintSample(
        string title,
        IReadOnlyList<string> lines)
    {
        const int maxLines = 10;

        Console.WriteLine(title);

        var count = Math.Min(lines.Count, maxLines);

        for (var i = 0; i < count; i++)
        {
            Console.WriteLine(lines[i]);
        }

        if (lines.Count > maxLines)
        {
            Console.WriteLine("...");
        }
    }
}

