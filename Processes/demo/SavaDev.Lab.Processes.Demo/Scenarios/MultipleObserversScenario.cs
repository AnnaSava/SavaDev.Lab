using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Demo.Observers;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates using multiple process observers
/// to handle process output simultaneously.
/// </summary>
/// <remarks>
/// This scenario shows how several independent process
/// observers can be registered to receive output
/// notifications from a single process execution.
///
/// Each observer is added directly to the
/// <see cref="ProcessOutputHandling"/> configuration and
/// is notified according to the observer interfaces it
/// implements. No composite observer is required.
///
/// The scenario demonstrates the following observers:
/// <list type="bullet">
/// <item>
/// A console-aware observer that writes output together
/// with execution context information.
/// </item>
/// <item>
/// A buffered observer that accumulates output lines
/// for post-execution inspection.
/// </item>
/// <item>
/// A timing observer that measures process execution
/// duration.
/// </item>
/// </list>
///
/// All observers operate independently and receive the
/// same output events during process execution.
/// </remarks>
public sealed class MultipleObserversScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Multiple observers";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var launcher = new ProcessLauncher(new ProcessOutputObserverResolver());

        var bufferedObserver = new BufferedBasicObserver();
        var timingObserver = new TimingProcessObserver();

        var output = new ProcessOutputHandling(
            new IProcessOutputObserver[]
            {
                new ConsoleContextualObserver(),
                bufferedObserver,
                timingObserver
            });

        var request = CreateRequest();

        Console.WriteLine("Starting process with multiple observers...");
        Console.WriteLine();

        await launcher.RunAsync(
            request,
            output,
            ct);

        timingObserver.Stop();

        Console.WriteLine();
        Console.WriteLine("=== Process completed ===");
        Console.WriteLine($"Elapsed time: {timingObserver.Elapsed.TotalMilliseconds:N0} ms");
        Console.WriteLine();

        PrintSample(
            "Captured standard output (first lines):",
            bufferedObserver.StandardOutputLines);
    }

    /// <summary>
    /// Creates a process request used by the
    /// multiple observers demonstration scenario.
    /// </summary>
    /// <remarks>
    /// The request starts a simple cross-platform command
    /// that produces several lines of standard output
    /// with a short delay between them. This allows
    /// multiple observers to receive and process output
    /// events during process execution.
    /// </remarks>
    private static ProcessRequest CreateRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo Line 1 & echo Line 2 & timeout /t 1 >nul & echo Line 3"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = "-c \"echo Line 1; echo Line 2; sleep 1; echo Line 3\""
        };
    }

    /// <summary>
    /// Prints a small sample of the provided output lines
    /// to the console.
    /// </summary>
    /// <param name="title">
    /// A descriptive title printed before the output sample.
    /// </param>
    /// <param name="lines">
    /// A collection of output lines to sample from.
    /// </param>
    /// <remarks>
    /// This helper method intentionally limits the number
    /// of printed lines to keep demo output readable,
    /// even when the underlying process produces
    /// a large amount of data.
    /// </remarks>
    private static void PrintSample(
        string title,
        IReadOnlyList<string> lines)
    {
        const int maxLines = 5;

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
