using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Demo.Observers;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates using a custom process output observer
/// to collect and analyze process output.
/// </summary>
/// <remarks>
/// This scenario shows how a consumer can implement
/// <see cref="IProcessOutputObserver"/> to perform custom
/// logic on process output, such as aggregation, counting,
/// or transformation, without modifying the process launcher.
///
/// The observer used in this scenario counts the number of
/// standard output and standard error lines produced by
/// the process.
/// </remarks>
public sealed class CustomObserverScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Custom output observer";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var processLauncher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new CountingProcessObserver();
        var output = new ProcessOutputHandling(observer);

        var request = CreateRequest();

        Console.WriteLine("Starting process with custom observer...");
        Console.WriteLine();

        var result = await processLauncher.RunAsync(
            request,
            output,
            ct);

        Console.WriteLine();
        Console.WriteLine("=== Process completed ===");
        Console.WriteLine($"Exit code: {result.ExitCode}");
        Console.WriteLine();

        Console.WriteLine("Observer statistics:");
        Console.WriteLine($"  StdOut lines: {observer.StandardOutputLineCount}");
        Console.WriteLine($"  StdErr lines: {observer.StandardErrorLineCount}");
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> that produces
    /// both standard output and standard error lines.
    /// </summary>
    /// <remarks>
    /// The request is constructed differently depending on
    /// the operating system in order to use a native shell:
    /// <list type="bullet">
    /// <item>
    /// On Windows, <c>cmd</c> is used with a command sequence
    /// that writes multiple lines to standard output and
    /// intentionally redirects one line to standard error.
    /// </item>
    /// <item>
    /// On Unix-like systems, <c>/bin/sh</c> is used with an
    /// equivalent shell command that produces the same output
    /// behavior.
    /// </item>
    /// </list>
    ///
    /// The redirection operator <c>1&gt;&amp;2</c> is used to
    /// explicitly send output to the standard error stream.
    /// This is done to demonstrate that the process launcher
    /// correctly captures and distinguishes standard output
    /// and standard error independently, even when the process
    /// exits successfully.
    /// </remarks>
    /// <returns>
    /// A <see cref="ProcessRequest"/> configured to launch
    /// a shell command that writes to both output streams.
    /// </returns>
    private static ProcessRequest CreateRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo Line 1 & echo Line 2 & echo Error message 1>&2"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = "-c \"echo Line 1; echo Line 2; echo Error message 1>&2\""
        };
    }
}
