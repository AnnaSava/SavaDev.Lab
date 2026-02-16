using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Extensions;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates handling of a process that fails
/// with a non-zero exit code.
/// </summary>
/// <remarks>
/// This scenario launches a process that explicitly
/// writes an error message to the standard error stream
/// and terminates with a non-zero exit code.
///
/// The scenario illustrates that:
/// <list type="bullet">
/// <item>A non-zero exit code does not result in an exception.</item>
/// <item>Standard error output is captured independently.</item>
/// <item>The failure is represented in <see cref="ProcessResult"/>.</item>
/// </list>
///
/// This behavior allows consumers to decide how to
/// interpret process failures instead of relying on
/// exceptions for control flow.
/// </remarks>
public sealed class FailingProcessScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Failing process";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var processLauncher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new ConsoleContextualObserver();
        var output = new ProcessOutputHandling(observer);

        var request = CreateRequest();

        Console.WriteLine("Starting a process that will fail...");
        Console.WriteLine();

        var result = await processLauncher.RunAsync(
            request,
            output,
            ct);

        Console.WriteLine();
        Console.WriteLine("=== Process completed ===");
        Console.WriteLine($"Exit code: {result.ExitCode}");
        Console.WriteLine($"Is success: {result.IsSuccess()}");
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> that represents
    /// a failing external process.
    /// </summary>
    /// <remarks>
    /// The request is constructed differently depending on
    /// the operating system in order to use a native shell:
    /// <list type="bullet">
    /// <item>
    /// On Windows, <c>cmd</c> is used to write an error message
    /// to the standard error stream and terminate the process
    /// with a non-zero exit code.
    /// </item>
    /// <item>
    /// On Unix-like systems, <c>/bin/sh</c> is used to perform
    /// the same actions using POSIX shell syntax.
    /// </item>
    /// </list>
    ///
    /// The redirection operator <c>1&gt;&amp;2</c> is used to
    /// explicitly write output to the standard error stream,
    /// while the shell <c>exit</c> command terminates the process
    /// with an exit code of <c>1</c>.
    ///
    /// This request demonstrates that a process failure is
    /// represented by its exit code and captured error output,
    /// and does not automatically result in an exception being
    /// thrown by the process launcher.
    /// </remarks>
    /// <returns>
    /// A <see cref="ProcessRequest"/> configured to launch
    /// a process that writes to standard error and exits
    /// with a non-zero exit code.
    /// </returns>
    private static ProcessRequest CreateRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo Fatal error occurred 1>&2 & exit /b 1"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = "-c \"echo Fatal error occurred 1>&2; exit 1\""
        };
    }
}
