using AwesomeAssertions;
using NSubstitute;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.ProcessLauncherTests;

/// <summary>
/// Contains tests that exercise <see cref="ProcessLauncher.RunAsync(ProcessRequest, ProcessOutputHandling?, CancellationToken)"/>
/// using an external demo worker project.
/// </summary>
/// <remarks>
/// These tests rely on a prebuilt demo worker executable from
/// <c>SavaDev.Lab.Processes.TestAssets.LongProcess</c>.
/// The executable is expected to be built using the same
/// configuration and target framework as the test project.
/// </remarks>
[Trait("Category", "ExternalProject")]
/// <summary>
/// Tests for RunAsync_ExternalProject_Tests.
/// </summary>
public sealed class RunAsync_ExternalProject_Tests
{
    /// <summary>
    /// Relative path to the demo worker build output directory,
    /// excluding configuration and target framework.
    /// </summary>
    private const string WorkerProjectBaseRelativePath =
        "../../../../SavaDev.Lab.Processes.TestAssets.LongProcess/bin/";

    /// <summary>
    /// Name of the demo worker project and its executable.
    /// </summary>
    private const string WorkerProjectName =
        "SavaDev.Lab.Processes.TestAssets.LongProcess";

    /// <summary>
    /// Target framework moniker used by the demo worker project.
    /// </summary>
    private const string WorkerTargetFramework = "net8.0";

    /// <summary>
    /// Number of milliseconds to wait before requesting cancellation
    /// of the demo worker process during the test.
    /// </summary>
    private const int CancellationDelayMilliseconds = 300;

    [Fact]
/// <summary>
/// Tests RunAsync_ShouldHonorCancellation.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldHonorCancellation()
    {
        // Arrange
        var processLauncher =
            new ProcessLauncher(Substitute.For<IProcessOutputObserverResolver>());

        var request = new ProcessRequest
        {
            FileName = ResolveWorkerPath(),
            Arguments = string.Empty
        };

        using var cts = new CancellationTokenSource();

        // Act
        var action = async () =>
            await processLauncher.RunAsync(
                request,
                output: null,
                ct: cts.Token);

        // Give the process a moment to start
        await Task.Delay(CancellationDelayMilliseconds);

        cts.Cancel();

        // Assert
        await action.Should()
            .ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Resolves the full path to the demo worker executable.
    /// </summary>
    /// <remarks>
    /// The worker executable is expected to be built using the same
    /// configuration as the current test run (Debug or Release)
    /// and the <c>net8.0</c> target framework.
    /// </remarks>
    private static string ResolveWorkerPath()
    {
        var configuration = ResolveConfiguration();

        var workerExeName =
            OperatingSystem.IsWindows()
                ? $"{WorkerProjectName}.exe"
                : WorkerProjectName;

        var workerPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                WorkerProjectBaseRelativePath,
                configuration,
                WorkerTargetFramework,
                workerExeName));

        if (!File.Exists(workerPath))
        {
            throw new FileNotFoundException(
                $"Demo worker executable was not found for configuration '{configuration}'.",
                workerPath);
        }

        return workerPath;
    }

    /// <summary>
    /// Resolves the build configuration used for the current test run.
    /// </summary>
    /// <remarks>
    /// Falls back to <c>Debug</c> if no configuration is explicitly provided.
    /// </remarks>
    private static string ResolveConfiguration()
    {
        return
            Environment.GetEnvironmentVariable("DOTNET_CONFIGURATION")
            ?? Environment.GetEnvironmentVariable("CONFIGURATION")
            ?? "Debug";
    }
}

