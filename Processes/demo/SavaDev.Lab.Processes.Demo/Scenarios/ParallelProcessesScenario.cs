using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates parallel execution of multiple external
/// processes using concurrent <see cref="IProcessLauncher.RunAsync"/>
/// invocations.
/// </summary>
/// <remarks>
/// This scenario illustrates a usage pattern where several
/// independent processes are started simultaneously and
/// awaited as a group.
///
/// The scenario highlights that:
/// <list type="bullet">
/// <item>Multiple processes can be launched concurrently.</item>
/// <item>Each process has its own execution and exit code.</item>
/// <item>Output handling is isolated per process execution.</item>
/// </list>
///
/// Parallel execution is useful for workloads where processes
/// do not depend on each other and overall execution time can
/// be reduced by running them concurrently.
///
/// Coordination and aggregation of results are handled at the
/// scenario level rather than inside the process launcher.
/// </remarks>
public sealed class ParallelProcessesScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Parallel processes";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var launcher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new ConsoleContextualObserver();
        var output = new ProcessOutputHandling(observer);

        var requests = new[]
        {
            CreateRequest("First"),
            CreateRequest("Second"),
            CreateRequest("Third")
        };

        Console.WriteLine("Starting parallel process execution...");
        Console.WriteLine();

        var tasks = requests
            .Select(request => launcher.RunAsync(request, output, ct))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Console.WriteLine();
        Console.WriteLine("=== Parallel execution completed ===");

        for (var i = 0; i < results.Length; i++)
        {
            Console.WriteLine(
                $"Process {i + 1} exit code: {results[i].ExitCode}");
        }
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> that produces
    /// a small amount of output identifying the process.
    /// </summary>
    /// <remarks>
    /// Each request launches a simple shell command that writes
    /// a single line to the standard output stream. The label
    /// is included in the output to distinguish concurrent
    /// executions when running in parallel.
    ///
    /// The command is constructed differently depending on
    /// the operating system to use a native shell.
    /// </remarks>
    /// <param name="label">
    /// A label used to identify the process in its output.
    /// </param>
    /// <returns>
    /// A <see cref="ProcessRequest"/> configured to emit
    /// labeled output.
    /// </returns>
    private static ProcessRequest CreateRequest(string label)
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = $"/c echo {label} process executed"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"echo {label} process executed\""
        };
    }
}

