using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Demo.Observers;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates measuring process execution time
/// using a timing process observer.
/// </summary>
/// <remarks>
/// This scenario shows how execution timing can be
/// collected independently of the process launcher
/// by observing process output events.
///
/// The observer measures elapsed time starting from
/// the first observed output line and reports the
/// total duration after process completion.
/// </remarks>
public sealed class TimingObserverScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Timing observer";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var launcher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new TimingProcessObserver();
        var output = new ProcessOutputHandling(observer);

        var request = CreateRequest();

        Console.WriteLine("Starting timed process...");
        Console.WriteLine();

        await launcher.RunAsync(
            request,
            output,
            ct);

        observer.Stop();

        Console.WriteLine();
        Console.WriteLine("=== Process completed ===");
        Console.WriteLine($"Elapsed time: {observer.Elapsed.TotalMilliseconds:N0} ms");
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> that produces
    /// a small amount of output with a deliberate delay
    /// between messages.
    /// </summary>
    /// <remarks>
    /// The request launches a simple shell command that writes
    /// an initial line to the standard output stream, waits
    /// for a short, fixed duration, and then writes a final line
    /// before exiting.
    ///
    /// This controlled delay allows scenarios to demonstrate
    /// time-based behaviors such as output streaming,
    /// cancellation, or execution timing, while keeping the
    /// command logic minimal and predictable.
    ///
    /// The command is constructed differently depending on the
    /// operating system in order to use a native shell and
    /// equivalent delay mechanisms on each platform.
    /// </remarks>
    /// <returns>
    /// A <see cref="ProcessRequest"/> configured to emit output
    /// before and after a short delay.
    /// </returns>
    private static ProcessRequest CreateRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo Start & timeout /t 2 >nul & echo End"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = "-c \"echo Start; sleep 2; echo End\""
        };
    }
}
