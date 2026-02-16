using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates using a buffered process output observer
/// to collect and analyze process output after execution.
/// </summary>
/// <remarks>
/// This scenario illustrates a usage pattern where process
/// output is streamed and accumulated by a custom observer
/// during execution, in addition to any output captured
/// in <see cref="ProcessResult"/>.
/// 
/// The scenario highlights that:
/// <list type="bullet">
/// <item>Output can be captured incrementally during execution.</item>
/// <item>The observer is responsible for buffering and synchronization.</item>
/// <item>
/// The collected output can be inspected independently of
/// <see cref="ProcessResult"/> after the process completes.
/// </item>
/// </list>
/// 
/// This approach is suitable for scenarios where output
/// needs to be analyzed, summarized, or partially retained
/// without coupling buffering behavior to the process launcher,
/// even when result-level output capturing is enabled.
/// </remarks>
public sealed class BufferedObserverScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Buffered observer";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var processLauncher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new BufferedBasicObserver();
        var output = new ProcessOutputHandling(observer);

        var request = CreateRequest();

        Console.WriteLine("Starting process with buffered observer...");
        Console.WriteLine("Process output will be collected by the observer.");
        Console.WriteLine();

        var result = await processLauncher.RunAsync(
            request,
            output,
            ct);

        Console.WriteLine();
        Console.WriteLine("=== Process completed ===");
        Console.WriteLine($"Exit code: {result.ExitCode}");
        Console.WriteLine();

        Console.WriteLine($"Captured standard output lines: {observer.StandardOutputLines.Count}");
        Console.WriteLine($"Captured standard error lines: {observer.StandardErrorLines.Count}");
        Console.WriteLine();

        PrintSample("Standard output (last 5 lines):", observer.StandardOutputLines);
        PrintSample("Standard error (last 5 lines):", observer.StandardErrorLines);
    }

    /// <summary>
    /// Prints a sample of the provided output lines
    /// to the console.
    /// </summary>
    /// <param name="title">
    /// A descriptive title printed before the output sample.
    /// </param>
    /// <param name="lines">
    /// A collection of output lines from which a subset
    /// will be displayed.
    /// </param>
    /// <remarks>
    /// This method is intended for demonstration purposes
    /// only. When the number of lines exceeds a small threshold,
    /// only the last few lines are printed in order to keep
    /// the console output concise and readable.
    /// </remarks>
    private static void PrintSample(
        string title,
        IReadOnlyList<string> lines)
    {
        Console.WriteLine(title);

        if (lines.Count == 0)
        {
            Console.WriteLine("  (none)");
            Console.WriteLine();
            return;
        }

        foreach (var line in lines.TakeLast(5))
        {
            Console.WriteLine($"  {line}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> that produces
    /// both standard output and standard error lines.
    /// </summary>
    /// <remarks>
    /// The request launches a shell command that writes
    /// multiple lines to the standard output stream and
    /// multiple lines to the standard error stream in an
    /// interleaved manner.
    ///
    /// This setup is intended to demonstrate how a buffered
    /// process output observer can collect and separate
    /// output from different streams while preserving
    /// their original order within each stream.
    ///
    /// The command is constructed differently depending on
    /// the operating system to use a native shell.
    /// </remarks>
    /// <returns>
    /// A <see cref="ProcessRequest"/> configured to generate
    /// a mix of standard output and standard error output.
    /// </returns>
    private static ProcessRequest CreateRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments =
                    "/c echo Line 1 & " +
                    "echo Line 2 & " +
                    "echo Error 1>&2 & " +
                    "echo Line 3 & " +
                    "echo Error 2 1>&2"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments =
                "-c \"echo Line 1; " +
                "echo Line 2; " +
                "echo Error 1>&2; " +
                "echo Line 3; " +
                "echo Error 2>&2\""
        };
    }
}
