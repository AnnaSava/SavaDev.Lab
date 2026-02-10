using AwesomeAssertions;
using NSubstitute;
using SavaDev.Lab.Processes.Launchers.Shell;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.ShellProcessLauncherTests;

/// <summary>
/// Contains tests verifying the behavior of
/// <see cref="ShellProcessLauncher.RunAsync(string, string?, ProcessOutputHandling?, CancellationToken)"/>.
/// </summary>
/// <remarks>
/// These tests ensure that shell commands are executed
/// using the default shell of the current operating system
/// and correctly delegated to the underlying
/// <see cref="IProcessLauncher"/>.
/// </remarks>
public sealed class RunAsync_Tests
{
    
    /// <summary>
    /// Verifies that the default shell executable
    /// is selected based on the current operating system.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>On Windows, <c>cmd.exe</c> is used.</item>
    /// <item>On Unix-like systems, <c>/bin/sh</c> is used.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldUseDefaultShellExecutable.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldUseDefaultShellExecutable()
    {
        // Arrange
        var processLauncher = Substitute.For<IProcessLauncher>();
        var shellLauncher = new ShellProcessLauncher(processLauncher);

        var result = new ProcessResult { ExitCode = 0 };

        processLauncher
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                Arg.Any<ProcessOutputHandling?>(),
                Arg.Any<CancellationToken>())
            .Returns(result);

        // Act
        await shellLauncher.RunAsync(
            "echo test",
            output: null,
            ct: CancellationToken.None);

        // Assert
        await processLauncher.Received(1)
            .RunAsync(
                Arg.Is<ProcessRequest>(r =>
                    OperatingSystem.IsWindows()
                        ? r.FileName == "cmd.exe"
                        : r.FileName == "/bin/sh"),
                null,
                CancellationToken.None);
    }
        
    /// <summary>
    /// Verifies that the shell command is wrapped
    /// using the appropriate shell arguments.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The provided command is passed as a single shell invocation.</item>
    /// <item>The shell-specific argument format is applied.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldWrapCommandUsingShellSyntax.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldWrapCommandUsingShellSyntax()
    {
        // Arrange
        var processLauncher = Substitute.For<IProcessLauncher>();
        var shellLauncher = new ShellProcessLauncher(processLauncher);

        const string command = "echo hello";

        processLauncher
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                Arg.Any<ProcessOutputHandling?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ProcessResult());

        // Act
        await shellLauncher.RunAsync(
            command,
            output: null,
            ct: CancellationToken.None);

        // Assert
        await processLauncher.Received(1)
            .RunAsync(
                Arg.Is<ProcessRequest>(r =>
                    OperatingSystem.IsWindows()
                        ? r.Arguments == $"/c \"{command}\""
                        : r.Arguments == $"-c \"{command}\""),
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
    /// <item>No additional transformation is applied.</item>
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
        var shellLauncher = new ShellProcessLauncher(processLauncher);

        const string workingDirectory = "/tmp/project";

        processLauncher
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                Arg.Any<ProcessOutputHandling?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ProcessResult());

        // Act
        await shellLauncher.RunAsync(
            "echo test",
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
    /// The shell launcher does not alter or intercept
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
        var shellLauncher = new ShellProcessLauncher(processLauncher);

        var observer = Substitute.For<IProcessOutputBasicObserver>();
        var output = new ProcessOutputHandling(observer);

        processLauncher
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                output,
                Arg.Any<CancellationToken>())
            .Returns(new ProcessResult());

        // Act
        await shellLauncher.RunAsync(
            "echo test",
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
    /// Verifies that the cancellation token is forwarded
    /// to the underlying process launcher.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The provided cancellation token is passed unchanged.</item>
    /// <item>The shell launcher does not replace or ignore the token.</item>
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
        var shellLauncher = new ShellProcessLauncher(processLauncher);

        using var cts = new CancellationTokenSource();

        processLauncher
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                Arg.Any<ProcessOutputHandling?>(),
                cts.Token)
            .Returns(new ProcessResult());

        // Act
        await shellLauncher.RunAsync(
            "echo test",
            output: null,
            ct: cts.Token);

        // Assert
        await processLauncher.Received(1)
            .RunAsync(
                Arg.Any<ProcessRequest>(),
                Arg.Any<ProcessOutputHandling?>(),
                cts.Token);
    }

    /// <summary>
    /// Verifies that the process result returned by
    /// the underlying launcher is propagated without modification.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The returned <see cref="ProcessResult"/> is passed through unchanged.</item>
    /// <item>The shell launcher does not alter execution results.</item>
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
        var shellLauncher = new ShellProcessLauncher(processLauncher);

        var expectedResult = new ProcessResult
        {
            ExitCode = 123,
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
        var result = await shellLauncher.RunAsync(
            "echo test",
            output: null,
            ct: CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedResult);
    }
}

