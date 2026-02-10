using System.Collections.Concurrent;
using AwesomeAssertions;
using NSubstitute;
using SavaDev.Lab.Processes.Extensions;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.ProcessLauncherTests;

/// <summary>
/// Contains tests verifying the behavior of the
/// <see cref="ProcessLauncher.RunAsync(ProcessRequest, ProcessOutputHandling?, CancellationToken)"/>
/// method.
/// </summary>
/// <remarks>
/// These tests describe the expected behavior of process execution,
/// including handling of standard output and error streams,
/// environment variables, working directory configuration,
/// argument passing, and cancellation.
/// 
/// The tests serve as a behavioral specification and document
/// the guarantees provided by the <see cref="ProcessLauncher"/>
/// implementation.
/// 
/// The tests are written to be cross-platform and rely only
/// on standard shell commands available on supported systems.
/// </remarks>
public sealed class RunAsync_Tests
{
    /// <summary>
    /// Creates a <see cref="ProcessLauncher"/> instance configured
    /// with a substituted observer resolver.
    /// </summary>
    private static ProcessLauncher CreateLauncher() => new(Substitute.For<IProcessOutputObserverResolver>());

    /// <summary>
    /// Verifies that the process request identifier is propagated
    /// to the resulting <see cref="ProcessResult"/>.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process executes successfully.</item>
    /// <item>The result contains the same request identifier.</item>
    /// <item>The identifier matches <see cref="ProcessRequest.Id"/>.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldPropagateRequestIdToResult.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldPropagateRequestIdToResult()
    {
        // Arrange
        var launcher = CreateLauncher();

        var request = CreateEchoRequest("request-id-test");

        // Act
        var result = await launcher.RunAsync(
            request,
            output: null,
            CancellationToken.None);

        // Assert
        result.RequestId.Should().Be(request.Id);
    }

    /// <summary>
    /// Verifies that a successfully completed process returns
    /// its exit code and captured standard output.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process exits with code <c>0</c>.</item>
    /// <item>Standard output is captured and returned.</item>
    /// <item>Standard error output is empty.</item>
    /// <item>The result is marked as successful.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldReturnExitCodeAndCaptureStandardOutput.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldReturnExitCodeAndCaptureStandardOutput()
    {
        // Arrange
        var launcher = CreateLauncher();
        var request = CreateEchoRequest("hello");

        // Act
        var result = await launcher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        result.ExitCode.Should().Be(0);
        result.IsSuccess().Should().BeTrue();
        result.StandardOutput.Should().Contain("hello");
        result.StandardError.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that standard error output is captured
    /// when the process terminates with a non-zero exit code.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process exits with a non-zero code.</item>
    /// <item>Standard error output is captured and returned.</item>
    /// <item>The result is marked as unsuccessful.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldCaptureStandardError.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldCaptureStandardError()
    {
        // Arrange
        var launcher = CreateLauncher();
        var request = CreateWriteToStderrRequest("boom");

        // Act
        var result = await launcher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        result.ExitCode.Should().NotBe(0); // conventionally non-zero for error
        result.IsSuccess().Should().BeFalse();
        result.StandardError.Should().Contain("boom");
    }

    /// <summary>
    /// Verifies that provided environment variables
    /// are applied to the launched process.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Custom environment variables are passed to the process.</item>
    /// <item>The process can access the provided variables during execution.</item>
    /// <item>The resulting output reflects the applied environment.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldApplyEnvironmentVariables.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldApplyEnvironmentVariables()
    {
        // Arrange
        var launcher = CreateLauncher();

        var request = CreateEchoEnvVarRequest("SAVADEV_TEST_VAR");

        request = new ProcessRequest
        {
            FileName = request.FileName,
            Arguments = request.Arguments,
            WorkingDirectory = request.WorkingDirectory,
            EnvironmentVariables = new Dictionary<string, string>
            {
                ["SAVADEV_TEST_VAR"] = "value-123"
            }
        };

        // Act
        var result = await launcher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Contain("value-123");
    }

    /// <summary>
    /// Verifies that the specified working directory
    /// is applied to the launched process.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process is started with the provided working directory.</item>
    /// <item>The process observes the specified directory as its current location.</item>
    /// <item>The resulting output reflects the applied working directory.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldApplyWorkingDirectory.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldApplyWorkingDirectory()
    {
        // Arrange
        var launcher = CreateLauncher();

        var dir = Path.Combine(Path.GetTempPath(), "SavaDev.Lab.Processes.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        var baseRequest = CreatePrintWorkingDirectoryRequest();

        var request = new ProcessRequest
        {
            FileName = baseRequest.FileName,
            Arguments = baseRequest.Arguments,
            WorkingDirectory = dir,
            EnvironmentVariables = baseRequest.EnvironmentVariables
        };

        // Act
        var result = await launcher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        result.ExitCode.Should().Be(0);

        // Normalize line endings + trailing newlines
        var output = result.StandardOutput.Replace("\r\n", "\n").Trim();

        // On Windows "cd" prints the directory; on Unix "pwd" prints absolute path.
        output.Should().Be(Path.GetFullPath(dir));
    }

    /// <summary>
    /// Verifies that an exception is thrown
    /// when an invalid executable name is provided.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Process startup fails due to an invalid executable.</item>
    /// <item>An exception is propagated to the caller.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldThrow_WhenFileNameIsInvalid.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldThrow_WhenFileNameIsInvalid()
    {
        // Arrange
        var launcher = CreateLauncher();

        var request = new ProcessRequest
        {
            FileName = "__definitely_not_a_real_executable__",
            Arguments = string.Empty
        };

        // Act
        var act = async () => await launcher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>(); // Win32Exception typically, but varies by OS/runtime
    }

    /// <summary>
    /// Verifies that a null request is rejected.
    /// </summary>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldThrow_WhenRequestIsNull.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldThrow_WhenRequestIsNull()
    {
        // Arrange
        var launcher = CreateLauncher();

        // Act
        var act = async () => await launcher.RunAsync(null!, output: null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Verifies that an empty file name is rejected.
    /// </summary>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldThrow_WhenFileNameIsEmpty.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldThrow_WhenFileNameIsEmpty()
    {
        // Arrange
        var launcher = CreateLauncher();

        var request = new ProcessRequest
        {
            FileName = " ",
            Arguments = string.Empty
        };

        // Act
        var act = async () => await launcher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    /// <summary>
    /// Verifies that a non-existent working directory is rejected.
    /// </summary>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldThrow_WhenWorkingDirectoryDoesNotExist.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldThrow_WhenWorkingDirectoryDoesNotExist()
    {
        // Arrange
        var launcher = CreateLauncher();

        var request = CreateEchoRequest("hello");
        request = new ProcessRequest
        {
            FileName = request.FileName,
            Arguments = request.Arguments,
            WorkingDirectory = Path.Combine(Path.GetTempPath(), "missing", Guid.NewGuid().ToString("N"))
        };

        // Act
        var act = async () => await launcher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    
    /// <summary>
    /// Verifies that empty output streams are returned
    /// when the executed process produces no output.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process completes successfully.</item>
    /// <item>Standard output is empty.</item>
    /// <item>Standard error output is empty.</item>
    /// <item>The result is marked as successful.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldReturnEmptyOutput_WhenProcessProducesNoOutput.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldReturnEmptyOutput_WhenProcessProducesNoOutput()
    {
        // Arrange
        var processLauncher = CreateLauncher();

        ProcessRequest request;

        if (OperatingSystem.IsWindows())
        {
            request = new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c exit 0"
            };
        }
        else
        {
            request = new ProcessRequest
            {
                FileName = "/bin/sh",
                Arguments = "-c \"exit 0\""
            };
        }

        // Act
        var result = await processLauncher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        result.ExitCode.Should().Be(0);
        result.IsSuccess().Should().BeTrue();
        result.StandardOutput.Should().BeEmpty();
        result.StandardError.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that large amounts of standard output
    /// are fully captured without truncation.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process completes successfully.</item>
    /// <item>All produced standard output is captured.</item>
    /// <item>The operation does not hang or fail.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldCaptureLargeStandardOutput.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldCaptureLargeStandardOutput()
    {
        // Arrange
        var processLauncher = CreateLauncher();

        ProcessRequest request;

        if (OperatingSystem.IsWindows())
        {
            // Produce multiple lines of output
            request = new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c for /L %i in (1,1,1000) do @echo line-%i"
            };
        }
        else
        {
            request = new ProcessRequest
            {
                FileName = "/bin/sh",
                Arguments = "-c \"for i in $(seq 1 1000); do echo line-$i; done\""
            };
        }

        // Act
        var result = await processLauncher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        result.ExitCode.Should().Be(0);
        result.IsSuccess().Should().BeTrue();
        result.StandardOutput.Should().Contain("line-1");
        result.StandardOutput.Should().Contain("line-1000");
        result.StandardOutput.Length.Should().BeGreaterThan(1000);
    }

    /// <summary>
    /// Verifies that standard output and standard error
    /// are captured independently during process execution.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Standard output is captured correctly.</item>
    /// <item>Standard error output is captured correctly.</item>
    /// <item>Neither stream interferes with the other.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldCaptureStandardOutputAndStandardErrorIndependently.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldCaptureStandardOutputAndStandardErrorIndependently()
    {
        // Arrange
        var processLauncher = CreateLauncher();

        ProcessRequest request;

        if (OperatingSystem.IsWindows())
        {
            request = new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo out & echo err 1>&2"
            };
        }
        else
        {
            request = new ProcessRequest
            {
                FileName = "/bin/sh",
                Arguments = "-c \"echo out; echo err 1>&2\""
            };
        }

        // Act
        var result = await processLauncher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Contain("out");
        result.StandardError.Should().Contain("err");
    }

    /// <summary>
    /// Verifies that the process is executed successfully
    /// when no environment variables are provided.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process starts without additional environment variables.</item>
    /// <item>No exception is thrown.</item>
    /// <item>The process completes successfully.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldWork_WhenEnvironmentVariablesAreNull.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldWork_WhenEnvironmentVariablesAreNull()
    {
        // Arrange
        var processLauncher = CreateLauncher();

        ProcessRequest request;

        if (OperatingSystem.IsWindows())
        {
            request = new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo ok",
                EnvironmentVariables = null
            };
        }
        else
        {
            request = new ProcessRequest
            {
                FileName = "/bin/sh",
                Arguments = "-c \"echo ok\"",
                EnvironmentVariables = null
            };
        }

        // Act
        var result = await processLauncher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        result.ExitCode.Should().Be(0);
        result.IsSuccess().Should().BeTrue();
        result.StandardOutput.Should().Contain("ok");
    }

    /// <summary>
    /// Verifies that the default working directory is used
    /// when no working directory is specified.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process starts without an explicitly defined working directory.</item>
    /// <item>The process completes successfully.</item>
    /// <item>No exception is thrown due to a missing working directory.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldUseDefaultWorkingDirectory_WhenNotSpecified.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldUseDefaultWorkingDirectory_WhenNotSpecified()
    {
        // Arrange
        var processLauncher = CreateLauncher();

        ProcessRequest request;

        if (OperatingSystem.IsWindows())
        {
            request = new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c cd"
            };
        }
        else
        {
            request = new ProcessRequest
            {
                FileName = "/bin/sh",
                Arguments = "-c \"pwd\""
            };
        }

        // Act
        var result = await processLauncher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that command-line arguments containing spaces
    /// are passed to the process without modification.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Arguments with spaces are received correctly by the process.</item>
    /// <item>The process output reflects the original argument value.</item>
    /// <item>The process completes successfully.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldPassArguments_WithSpacesCorrectly.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldPassArguments_WithSpacesCorrectly()
    {
        // Arrange
        var processLauncher = CreateLauncher();
        const string valueWithSpaces = "hello world";

        ProcessRequest request;

        if (OperatingSystem.IsWindows())
        {
            request = new ProcessRequest
            {
                FileName = "cmd",
                Arguments = $"/c echo {valueWithSpaces}"
            };
        }
        else
        {
            request = new ProcessRequest
            {
                FileName = "/bin/sh",
                Arguments = $"-c \"echo {valueWithSpaces}\""
            };
        }

        // Act
        var result = await processLauncher.RunAsync(request, output: null, CancellationToken.None);

        // Assert
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Contain(valueWithSpaces);
    }

    /// <summary>
    /// Verifies that the output observer is notified
    /// for each line written to standard output.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Each standard output line is forwarded to the observer.</item>
    /// <item>Lines are reported for each output line produced by the process.</item>
    /// <item>Standard error output does not trigger output notifications.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldNotifyObserver_OnStandardOutputLines.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldNotifyObserver_OnStandardOutputLines()
    {
        // Arrange
        var launcher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = Substitute.For<IProcessOutputBasicObserver>();
        var output = new ProcessOutputHandling(observer);

        ProcessRequest request;

        if (OperatingSystem.IsWindows())
        {
            request = new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo first & echo second"
            };
        }
        else
        {
            request = new ProcessRequest
            {
                FileName = "/bin/sh",
                Arguments = "-c \"echo first; echo second\""
            };
        }

        // Act
        await launcher.RunAsync(request, output, CancellationToken.None);

        // Assert
        observer.Received(1)
            .OnOutputLine(Arg.Is<string>(line => line.Contains("first")));

        observer.Received(1)
            .OnOutputLine(Arg.Is<string>(line => line.Contains("second")));

        observer.DidNotReceiveWithAnyArgs().OnErrorLine(default!);
    }

    /// <summary>
    /// Verifies that the output observer is notified
    /// for each line written to standard error.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Each standard error line is forwarded to the observer.</item>
    /// <item>Lines are reported for each error line produced by the process.</item>
    /// <item>Standard output does not trigger error notifications.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldNotifyObserver_OnStandardErrorLines.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldNotifyObserver_OnStandardErrorLines()
    {
        // Arrange
        var launcher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = Substitute.For<IProcessOutputBasicObserver>();
        var output = new ProcessOutputHandling(observer);

        ProcessRequest request;

        if (OperatingSystem.IsWindows())
        {
            request = new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo error1 1>&2 & echo error2 1>&2"
            };
        }
        else
        {
            request = new ProcessRequest
            {
                FileName = "/bin/sh",
                Arguments = "-c \"echo error1 1>&2; echo error2 1>&2\""
            };
        }

        // Act
        await launcher.RunAsync(request, output, CancellationToken.None);

        // Assert
        observer.Received(1)
            .OnErrorLine(Arg.Is<string>(line => line.Contains("error1")));

        observer.Received(1)
            .OnErrorLine(Arg.Is<string>(line => line.Contains("error2")));

        observer.DidNotReceiveWithAnyArgs().OnOutputLine(default!);
    }

    /// <summary>
    /// Verifies that providing an output observer
    /// does not affect the returned process result.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process result is identical with and without an observer.</item>
    /// <item>The exit code is not affected by observer presence.</item>
    /// <item>Captured standard output and error remain unchanged.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldReturnSameResult_WithAndWithoutObserver.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldReturnSameResult_WithAndWithoutObserver()
    {
        // Arrange
        var processLauncher = CreateLauncher();
        var observer = Substitute.For<IProcessOutputBasicObserver>();
        var output = new ProcessOutputHandling(observer);

        ProcessRequest request;

        if (OperatingSystem.IsWindows())
        {
            request = new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo out & echo err 1>&2"
            };
        }
        else
        {
            request = new ProcessRequest
            {
                FileName = "/bin/sh",
                Arguments = "-c \"echo out; echo err 1>&2\""
            };
        }

        // Act
        var resultWithoutObserver =
            await processLauncher.RunAsync(request, output: null, CancellationToken.None);

        var resultWithObserver =
            await processLauncher.RunAsync(request, output, CancellationToken.None);

        // Assert
        resultWithObserver.ExitCode.Should().Be(resultWithoutObserver.ExitCode);
        resultWithObserver.StandardOutput.Should().Be(resultWithoutObserver.StandardOutput);
        resultWithObserver.StandardError.Should().Be(resultWithoutObserver.StandardError);
    }

    /// <summary>
    /// Verifies that standard output and standard error
    /// are accumulated by default.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process produces standard output.</item>
    /// <item>The process produces standard output.</item>
    /// <item>Standard output is accumulated in <see cref="ProcessResult.StandardOutput"/>.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldCaptureOutput_ByDefault.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldCaptureOutput_ByDefault()

    {
        // Arrange
        var launcher = CreateLauncher();

        var request = CreateEchoRequest("test");

        // Act
        var result = await launcher.RunAsync(request);

        // Assert
        result.StandardOutput.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that process execution succeeds when
    /// output capturing is disabled and no output
    /// observer is provided.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process produces standard output.</item>
    /// <item>No output observer is configured.</item>
    /// <item>Standard output is not accumulated.</item>
    /// <item>The process completes successfully.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldSucceed_WhenCaptureIsDisabled_AndObserverIsNull.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldSucceed_WhenCaptureIsDisabled_AndObserverIsNull()
    {
        // Arrange
        var launcher = CreateLauncher();
        var output = new ProcessOutputHandling(captureOutput: false);

        var request = CreateEchoRequest();

        // Act
        var result = await launcher.RunAsync(request, output);

        // Assert
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().BeEmpty();
        result.StandardError.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that standard output is not accumulated
    /// when output capturing is disabled.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process produces standard output.</item>
    /// <item>The output observer receives output lines.</item>
    /// <item><see cref="ProcessResult.StandardOutput"/> is empty.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldNotCaptureStandardOutput_WhenCaptureIsDisabled.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldNotCaptureStandardOutput_WhenCaptureIsDisabled()
    {
        // Arrange
        var launcher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = Substitute.For<IProcessOutputBasicObserver>();
        var output = new ProcessOutputHandling(observer, captureOutput: false);

        var request = CreateEchoRequest();

        // Act
        var result = await launcher.RunAsync(request, output);

        // Assert
        result.StandardOutput.Should().BeEmpty();
        result.StandardError.Should().BeEmpty();

        observer.Received()
            .OnOutputLine(Arg.Any<string>());
    }

    /// <summary>
    /// Verifies that standard error is not accumulated
    /// when output capturing is disabled.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process writes to standard error.</item>
    /// <item>The output observer receives error lines.</item>
    /// <item><see cref="ProcessResult.StandardError"/> is empty.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldNotCaptureStandardError_WhenCaptureIsDisabled.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldNotCaptureStandardError_WhenCaptureIsDisabled()
    {
        // Arrange
        var launcher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = Substitute.For<IProcessOutputBasicObserver>();
        var output = new ProcessOutputHandling(observer, captureOutput: false);

        var request = CreateErrorRequest();

        // Act
        var result = await launcher.RunAsync(request, output);

        // Assert
        result.StandardError.Should().BeEmpty();

        observer.Received()
            .OnErrorLine(Arg.Any<string>());
    }

    /// <summary>
    /// Verifies that the process exit code is returned
    /// correctly when output capturing is disabled.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process completes successfully.</item>
    /// <item>The exit code is preserved.</item>
    /// <item>No output is accumulated.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldReturnExitCode_WhenCaptureIsDisabled.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldReturnExitCode_WhenCaptureIsDisabled()
    {
        // Arrange
        var launcher = CreateLauncher();
        var output = new ProcessOutputHandling(captureOutput: false);

        var request = CreateEchoRequest();

        // Act
        var result = await launcher.RunAsync(request, output);

        // Assert
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().BeEmpty();
        result.StandardError.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that cancellation requested after the process
    /// has completed does not override a successful result.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The process completes successfully.</item>
    /// <item>Cancellation is requested after completion.</item>
    /// <item>The result is still returned and no cancellation is thrown.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldReturnResult_WhenCancellationIsRequestedAfterCompletion.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldReturnResult_WhenCancellationIsRequestedAfterCompletion()
    {
        // Arrange
        var launcher = CreateLauncher();
        using var cts = new CancellationTokenSource();

        var request = CreateEchoRequest("done");

        // Act
        var result = await launcher.RunAsync(request, output: null, cts.Token);

        // Request cancellation after completion
        cts.Cancel();

        // Assert
        result.ExitCode.Should().Be(0);
        result.StandardOutput.Should().Contain("done");
    }

    /// <summary>
    /// Verifies that cancellation before start prevents execution.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The token is cancelled before the call.</item>
    /// <item>The operation throws <see cref="OperationCanceledException"/>.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldThrow_WhenCancelledBeforeStart.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldThrow_WhenCancelledBeforeStart()
    {
        // Arrange
        var launcher = CreateLauncher();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var request = CreateEchoRequest("never");

        // Act
        var act = async () => await launcher.RunAsync(request, output: null, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Verifies that observers receive output from both streams
    /// when standard output and error are produced concurrently.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>All standard output lines are observed.</item>
    /// <item>All standard error lines are observed.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests RunAsync_ShouldNotifyObserver_ForParallelOutputStreams.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task RunAsync_ShouldNotifyObserver_ForParallelOutputStreams()
    {
        // Arrange
        var launcher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new CountingObserver();
        var output = new ProcessOutputHandling(observer, captureOutput: false);

        var request = CreateParallelOutputRequest(lineCount: 50);

        // Act
        await launcher.RunAsync(request, output, CancellationToken.None);

        // Assert
        observer.StandardOutputLines.Should().HaveCount(50);
        observer.StandardErrorLines.Should().HaveCount(50);
    }

    // ---------------- helpers ----------------

    /// <summary>
    /// Creates a request that echoes the provided text to standard output.
    /// </summary>
    /// <param name="text">The text to echo.</param>
    /// <returns>A process request configured for the current OS shell.</returns>
    private static ProcessRequest CreateEchoRequest(string text)
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = $"/c echo {EscapeForCmd(text)}"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"echo {EscapeForSh(text)}\""
        };
    }

    /// <summary>
    /// Creates a request that echoes a default message to standard output.
    /// </summary>
    /// <returns>A process request configured for the current OS shell.</returns>
    private static ProcessRequest CreateEchoRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo Hello"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = "-c \"echo Hello\""
        };
    }

    /// <summary>
    /// Creates a request that writes a single line to standard error.
    /// </summary>
    /// <returns>A process request configured for the current OS shell.</returns>
    private static ProcessRequest CreateErrorRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c echo Error 1>&2"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = "-c \"echo Error 1>&2\""
        };
    }

    /// <summary>
    /// Creates a request that writes the specified text to standard error
    /// and exits with a non-zero code.
    /// </summary>
    /// <param name="text">The text to write to standard error.</param>
    /// <returns>A process request configured for the current OS shell.</returns>
    private static ProcessRequest CreateWriteToStderrRequest(string text)
    {
        if (OperatingSystem.IsWindows())
        {
            // Writes to stderr and exits with non-zero.
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = $"/c echo {EscapeForCmd(text)} 1>&2 & exit /b 5"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"echo {EscapeForSh(text)} 1>&2; exit 5\""
        };
    }

    /// <summary>
    /// Creates a request that echoes the value of the specified
    /// environment variable.
    /// </summary>
    /// <param name="varName">The environment variable name.</param>
    /// <returns>A process request configured for the current OS shell.</returns>
    private static ProcessRequest CreateEchoEnvVarRequest(string varName)
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = $"/c echo %{varName}%"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"echo ${varName}\""
        };
    }

    /// <summary>
    /// Creates a request that prints the current working directory.
    /// </summary>
    /// <returns>A process request configured for the current OS shell.</returns>
    private static ProcessRequest CreatePrintWorkingDirectoryRequest()
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = "/c cd"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = "-c \"pwd\""
        };
    }

    /// <summary>
    /// Creates a request that writes multiple lines to both
    /// standard output and standard error.
    /// </summary>
    /// <param name="lineCount">Number of lines to emit to each stream.</param>
    /// <returns>A process request configured for the current OS shell.</returns>
    private static ProcessRequest CreateParallelOutputRequest(int lineCount)
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = $"/c for /L %i in (1,1,{lineCount}) do @echo out-%i & echo err-%i 1>&2"
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"for i in $(seq 1 {lineCount}); do echo out-$i; echo err-$i 1>&2; done\""
        };
    }

    /// <summary>
    /// Escapes characters that are significant to <c>cmd.exe</c>.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>The escaped command text.</returns>
    private static string EscapeForCmd(string text) =>
        text.Replace("^", "^^").Replace("&", "^&").Replace("|", "^|").Replace("<", "^<").Replace(">", "^>");

    /// <summary>
    /// Escapes characters that are significant to <c>/bin/sh</c>.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>The escaped shell text.</returns>
    private static string EscapeForSh(string text) =>
        text.Replace("\"", "\\\"");

/// <summary>
/// Tests for CountingObserver.
/// </summary>
    private sealed class CountingObserver : IProcessOutputBasicObserver
    {
/// <summary>
/// Tests new.
/// </summary>
        private readonly ConcurrentQueue<string> _stdout = new();
/// <summary>
/// Tests new.
/// </summary>
        private readonly ConcurrentQueue<string> _stderr = new();

        public IReadOnlyCollection<string> StandardOutputLines => _stdout.ToArray();

        public IReadOnlyCollection<string> StandardErrorLines => _stderr.ToArray();

/// <summary>
/// Tests OnOutputLine.
/// </summary>
        public void OnOutputLine(string line) => _stdout.Enqueue(line);

/// <summary>
/// Tests OnErrorLine.
/// </summary>
        public void OnErrorLine(string line) => _stderr.Enqueue(line);
    }
}

