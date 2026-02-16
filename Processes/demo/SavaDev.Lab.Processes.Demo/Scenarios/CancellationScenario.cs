using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.DemoKit.ConsoleEngine.Assets;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates cancellation of a long-running external process
/// using a <see cref="CancellationToken"/>.
/// </summary>
/// <remarks>
/// This scenario starts a dedicated demo worker process and lets it
/// run until cancellation is requested.
///
/// Cancellation is triggered after a fixed delay using a
/// scenario-specific timer-based cancellation token. This ensures
/// that the worker process is already running when cancellation
/// occurs.
/// </remarks>
public sealed class CancellationScenario : IConsoleDemoScenario
{
    /// <summary>
    /// Relative path to the demo worker build output directory,
    /// excluding configuration and target framework.
    /// </summary>
    private const string WorkerProjectBaseRelativePath =
        "../../../../SavaDev.Lab.Processes.DemoAssets.LongProcess/bin/";

    /// <summary>
    /// Name of the demo worker project and its executable.
    /// </summary>
    private const string WorkerProjectName =
        "SavaDev.Lab.Processes.DemoAssets.LongProcess";

    /// <summary>
    /// Target framework moniker used by the demo worker project.
    /// </summary>
    private const string WorkerTargetFramework = "net8.0";

    /// <summary>
    /// Number of seconds to wait before requesting cancellation
    /// of the demo worker process.
    /// </summary>
    private const int CancellationDelay = 10;

    /// <inheritdoc />
    public string Name => "Cancellation";

    /// <summary>
    /// Runs the cancellation demonstration.
    /// </summary>
    /// <param name="ct">
    /// A cancellation token representing global application cancellation
    /// (for example, triggered by Ctrl+C).
    /// </param>
    /// <returns>
    /// A task representing the asynchronous execution of the scenario.
    /// </returns>
    public async Task RunAsync(CancellationToken ct)
    {
        using var scenarioCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // This cancellation is scenario-specific (timer-based),
        // not a global application cancellation.
        scenarioCts.CancelAfter(TimeSpan.FromSeconds(CancellationDelay));

        var processLauncher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new ConsoleContextualObserver();
        var output = new ProcessOutputHandling(observer);

        var workerPath = ConsoleAssetResolver.ResolveAssetExecutablePath(WorkerProjectName,
            WorkerProjectBaseRelativePath,
            WorkerTargetFramework);

        Console.WriteLine("Using demo worker executable:");
        Console.WriteLine(workerPath);
        Console.WriteLine();

        var request = new ProcessRequest
        {
            FileName = workerPath,
            Arguments = string.Empty
        };

        Console.WriteLine("Worker process started.");
        Console.WriteLine($"Cancellation will be requested in {CancellationDelay} seconds...");
        Console.WriteLine();

        try
        {
            await processLauncher.RunAsync(
                request,
                output,
                scenarioCts.Token);

            Console.WriteLine();
            Console.WriteLine("=== Worker process completed ===");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Global cancellation (Ctrl+C): propagate to the host.
            throw;
        }
        catch (OperationCanceledException)
        {
            // Scenario cancellation (timer): expected outcome.
            Console.WriteLine();
            Console.WriteLine(">>> Cancelling worker process...");
            Console.WriteLine("=== Worker process execution was cancelled ===");
        }
    }
}
