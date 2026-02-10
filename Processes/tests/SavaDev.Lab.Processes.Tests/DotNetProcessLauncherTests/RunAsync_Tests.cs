using AwesomeAssertions;
using NSubstitute;
using SavaDev.Lab.Processes.Launchers.DotNet;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.DotNetProcessLauncherTests;

/// <summary>
/// Contains tests verifying the behavior of
/// <see cref="DotNetProcessLauncher.RunAsync(string, string?, ProcessOutputHandling?, CancellationToken)"/>.
/// </summary>
/// <remarks>
/// These tests ensure that the dotnet-specific launcher
/// correctly delegates execution to the underlying
/// <see cref="IProcessLauncher"/> and applies the expected
/// command configuration.
/// </remarks>
public sealed class RunAsync_Tests
{
    /// <summary>
    /// Verifies that the dotnet executable name is used
    /// when executing a command.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The executable name is fixed to <c>dotnet</c>.</item>
    /// <item>The provided arguments are forwarded unchanged.</item>
    /// </list>
    /// </remarks>
     [Fact]
/// <summary>
/// Tests RunAsync_ShouldUseDotNetExecutable.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldUseDotNetExecutable()
    {
        // Arrange
        var processLauncher = Substitute.For<IProcessLauncher>();
        var dotNetLauncher = new DotNetProcessLauncher(processLauncher);

        var result = new ProcessResult { ExitCode = 0 };

        processLauncher
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                Arg.Any<ProcessOutputHandling?>(),
                Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        var actualResult = await dotNetLauncher.RunAsync(
            "build",
            output: null,
            ct: CancellationToken.None);

        // Assert
        actualResult.Should().BeSameAs(result);

        await processLauncher.Received(1)
            .RunAsync(
                Arg.Is<ProcessRequest>(r =>
                    r.FileName == "dotnet" &&
                    r.Arguments == "build"),
                null,
                CancellationToken.None);
    }
        
    /// <summary>
    /// Verifies that the working directory is forwarded
    /// to the underlying process launcher.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The provided working directory is included in the process request.</item>
    /// <item>No additional modifications are applied.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldForwardWorkingDirectory.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldForwardWorkingDirectory()
    {
        // Arrange
        var processLauncher = Substitute.For<IProcessLauncher>();
        var dotNetLauncher = new DotNetProcessLauncher(processLauncher);

        var result = new ProcessResult { ExitCode = 0 };
        const string workingDirectory = "/tmp/project";

        processLauncher
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                Arg.Any<ProcessOutputHandling?>(),
                Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        await dotNetLauncher.RunAsync(
            "restore",
            workingDirectory,
            output: null,
            ct: CancellationToken.None);

        // Assert
        await processLauncher.Received(1)
            .RunAsync(
                Arg.Is<ProcessRequest>(r =>
                    r.WorkingDirectory == workingDirectory),
                null,
                CancellationToken.None);
    }

    /// <summary>
    /// Verifies that output handling options are forwarded
    /// to the underlying process launcher unchanged.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>
    /// The provided <see cref="ProcessOutputHandling"/> instance
    /// is passed to the underlying launcher without modification.
    /// </item>
    /// <item>
    /// The dotnet launcher does not alter or intercept
    /// output handling configuration.
    /// </item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldForwardObserver.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldForwardObserver()
    {
        // Arrange
        var processLauncher = Substitute.For<IProcessLauncher>();
        var dotNetLauncher = new DotNetProcessLauncher(processLauncher);

        var observer = Substitute.For<IProcessOutputBasicObserver>();
        var output = new ProcessOutputHandling(observer);
        var result = new ProcessResult { ExitCode = 0 };

        processLauncher
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                output,
                Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        await dotNetLauncher.RunAsync(
            "test",
            output: output,
            ct: CancellationToken.None);

        // Assert
        await processLauncher.Received(1)
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                output,
                CancellationToken.None);
    }


    /// <summary>
    /// Verifies that the returned process result
    /// is propagated without modification.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The result returned by the underlying launcher is returned as-is.</item>
    /// <item>No additional processing is applied.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldReturnUnderlyingResult.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldReturnUnderlyingResult()
    {
        // Arrange
        var processLauncher = Substitute.For<IProcessLauncher>();
        var dotNetLauncher = new DotNetProcessLauncher(processLauncher);

        var expectedResult = new ProcessResult
        {
            ExitCode = 42,
            StandardOutput = "out",
            StandardError = "err"
        };

        processLauncher
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                Arg.Any<ProcessOutputHandling?>(),
                Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await dotNetLauncher.RunAsync(
            "--info",
            output: null,
            ct: CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResult);
    }
        
    /// <summary>
    /// Verifies that the cancellation token is forwarded
    /// to the underlying process launcher.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The provided cancellation token is passed unchanged.</item>
    /// <item>The dotnet launcher does not replace or ignore the token.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldForwardCancellationToken.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldForwardCancellationToken()
    {
        // Arrange
        var processLauncher = Substitute.For<IProcessLauncher>();
        var dotNetLauncher = new DotNetProcessLauncher(processLauncher);

        using var cts = new CancellationTokenSource();

        processLauncher
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                Arg.Any<ProcessOutputHandling?>(),
                cts.Token)
            .Returns(new ProcessResult());

        // Act
        await dotNetLauncher.RunAsync(
            "build",
            output: null,
            ct: cts.Token);

        // Assert
        await processLauncher.Received(1)
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                Arg.Any<ProcessOutputHandling?>(),
                cts.Token);
    }
}


