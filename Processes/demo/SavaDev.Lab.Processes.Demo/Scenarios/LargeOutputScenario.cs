using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Demo.Observers;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates handling of a process that produces
/// a large amount of standard output.
/// </summary>
/// <remarks>
/// This scenario launches a process that generates
/// a significant number of output lines in order to
/// demonstrate the difference between real-time output
/// streaming via an observer and buffered output returned
/// in <see cref="ProcessResult"/>.
///
/// The scenario highlights that:
/// <list type="bullet">
/// <item>Standard output is read incrementally and does not block execution.</item>
/// <item>An observer can process output in real time.</item>
/// <item>Large outputs are still captured in the final result.</item>
/// </list>
///
/// This scenario is intended to illustrate potential
/// memory considerations when working with large outputs
/// and to encourage the use of observers for streaming
/// processing when appropriate.
/// </remarks>
public sealed class LargeOutputScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Large output";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var processLauncher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new ProgressProcessObserver();
        var output = new ProcessOutputHandling(observer);

        var request = CreateRequest();

        Console.WriteLine("Starting process that produces large output...");
        Console.WriteLine();

        var result = await processLauncher.RunAsync(
            request,
            output,
            ct);

        Console.WriteLine();
        Console.WriteLine("=== Process completed ===");
        Console.WriteLine($"Exit code: {result.ExitCode}");
        Console.WriteLine($"Total output lines: {observer.LineCount}");
        Console.WriteLine($"Captured output length: {result.StandardOutput.Length} characters");
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> that generates
    /// a large volume of standard output.
    /// </summary>
    /// <remarks>
    /// The request launches a simple, deterministic shell loop
    /// that writes a fixed number of lines to the standard output
    /// stream and then exits.
    ///
    /// The number of generated lines is intentionally large
    /// enough to demonstrate the impact of output volume on
    /// buffering and memory usage, while still keeping the
    /// execution time predictable and bounded.
    ///
    /// The command is constructed differently depending on the
    /// operating system in order to use a native shell and
    /// equivalent looping semantics on each platform.
    /// </remarks>
    /// <returns>
    /// A <see cref="ProcessRequest"/> configured to produce
    /// a large amount of standard output for demonstration
    /// purposes.
    /// </returns>
    private static ProcessRequest CreateRequest()
    {
        const int lineCount = 1000;

        if (OperatingSystem.IsWindows())
        {
            // Generates a sequence of lines using a simple loop in cmd
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = $"/c for /L %i in (1,1,{lineCount}) do echo Line %i"
            };
        }

        // Generates a sequence of lines using a shell loop
        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"for i in $(seq 1 {lineCount}); do echo Line $i; done\""
        };
    }
}
