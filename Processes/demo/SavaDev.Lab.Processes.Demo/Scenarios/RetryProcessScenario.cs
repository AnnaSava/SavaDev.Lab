using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Extensions;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates retrying a process execution when failures occur,
/// with a limited number of restart attempts and a delay
/// between retries.
/// </summary>
/// <remarks>
/// This scenario illustrates a resilient execution pattern where
/// an external process is restarted if it exits with a non-zero
/// exit code.
///
/// A fixed delay is applied between retry attempts to avoid
/// immediate restarts and to simulate backoff behavior.
///
/// Retry logic, including delay handling, is implemented at the
/// scenario level rather than inside the process launcher.
/// </remarks>
public sealed class RetryProcessScenario : IConsoleDemoScenario
{
    private const int MaxAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    /// <inheritdoc />
    public string Name => "Retry on failure (with delay)";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var launcher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new ConsoleContextualObserver();
        var output = new ProcessOutputHandling(observer);

        var request = CreateRequest();

        Console.WriteLine("Starting process with retry on failure...");
        Console.WriteLine($"Maximum attempts: {MaxAttempts}");
        Console.WriteLine($"Delay between attempts: {RetryDelay.TotalSeconds} seconds");
        Console.WriteLine();

        ProcessResult? lastResult = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            Console.WriteLine($"Attempt {attempt} started...");
            Console.WriteLine();

            lastResult = await launcher.RunAsync(
                request,
                output,
                ct);

            if (lastResult.IsSuccess())
            {
                Console.WriteLine();
                Console.WriteLine("Process completed successfully.");
                Console.WriteLine($"Exit code: {lastResult.ExitCode}");
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"Attempt {attempt} failed (exit code {lastResult.ExitCode}).");

            if (attempt < MaxAttempts)
            {
                Console.WriteLine($"Waiting {RetryDelay.TotalSeconds} seconds before retry...");
                Console.WriteLine();

                await Task.Delay(RetryDelay, ct);
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== All retry attempts exhausted ===");

        if (lastResult is not null)
        {
            Console.WriteLine($"Final exit code: {lastResult.ExitCode}");
        }
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> that deliberately
    /// fails in order to demonstrate retry behavior.
    /// </summary>
    /// <remarks>
    /// The request launches a shell command that exits with a
    /// non-zero exit code after producing output, making the
    /// retry logic observable and deterministic.
    /// </remarks>
    /// <returns>
    /// A <see cref="ProcessRequest"/> configured to simulate
    /// a failing process execution.
    /// </returns>
    private static ProcessRequest CreateRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo Simulated failure & exit /b 1"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = "-c \"echo Simulated failure; exit 1\""
        };
    }
}
