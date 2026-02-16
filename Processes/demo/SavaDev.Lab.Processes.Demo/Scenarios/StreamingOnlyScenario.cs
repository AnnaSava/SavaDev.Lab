using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates a streaming-only execution mode where
/// process output is not accumulated in the process result.
/// </summary>
/// <remarks>
/// This scenario illustrates a streaming-focused usage pattern
/// in which standard output and standard error are delivered
/// to an <see cref="IProcessOutputBasicObserver"/> during execution,
/// without being captured in <see cref="ProcessResult"/>.
///
/// Output buffering at the result level is explicitly disabled
/// for this execution, so process output is processed in
/// real time and not retained after completion.
///
/// This mode is suitable for scenarios involving large,
/// continuous, or unbounded output streams where retaining
/// the full output in <see cref="ProcessResult"/> is unnecessary
/// or undesirable, and output is intended to be handled
/// incrementally by an observer.
/// </remarks>
public sealed class StreamingOnlyScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Streaming only (no buffering)";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var processLauncher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new ConsoleBasicObserver();
        var output = new ProcessOutputHandling(observer, captureOutput: false);

        var request = CreateRequest();

        Console.WriteLine("Starting process in streaming-only mode...");
        Console.WriteLine("Process output will be handled in real time.");
        Console.WriteLine("Output buffering is disabled for this execution.");
        Console.WriteLine();

        await processLauncher.RunAsync(
            request,
            output,
            ct);

        Console.WriteLine();
        Console.WriteLine("=== Process completed ===");
        Console.WriteLine("All output was processed via the observer.");
        Console.WriteLine("No output was accumulated in the process result.");
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> configured for
    /// streaming-only output processing.
    /// </summary>
    /// <remarks>
    /// The request launches a shell command that produces
    /// a fixed number of output lines in a loop in order to
    /// simulate a large output stream.
    ///
    /// Output buffering is explicitly disabled by configuring
    /// <see cref="ProcessOutputHandling"/> with
    /// <c>captureOutput: false</c>, ensuring that all process
    /// output is delivered exclusively via the configured
    /// output observer and is not accumulated in memory.
    ///
    /// The command is constructed differently depending on
    /// the operating system to use a native shell:
    /// <list type="bullet">
    /// <item>
    /// On Windows, <c>cmd</c> is used with a <c>for</c> loop.
    /// </item>
    /// <item>
    /// On Unix-like systems, <c>/bin/sh</c> is used with a
    /// shell loop based on <c>seq</c>.
    /// </item>
    /// </list>
    /// </remarks>
    /// <returns>
    /// A <see cref="ProcessRequest"/> configured to produce
    /// a sequence of output lines in streaming-only mode.
    /// </returns>
    private static ProcessRequest CreateRequest()
    {
        const int lineCount = 500;

        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = $"/c for /L %i in (1,1,{lineCount}) do echo Streamed line %i",
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"for i in $(seq 1 {lineCount}); do echo Streamed line $i; done\"",
        };
    }
}
