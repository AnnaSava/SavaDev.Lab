using System.Diagnostics;
using System.Linq;
using SavaDev.Lab.Processes.Helpers;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes;

/// <summary>
/// Default implementation of <see cref="IProcessLauncher"/>
/// based on <see cref="System.Diagnostics.Process"/>.
/// </summary>
/// <remarks>
/// This implementation starts an external process without
/// shell execution, streams standard output and standard error
/// asynchronously, and waits for the process to exit.
///
/// During execution, optional output handling options can be
/// provided to receive standard output and error lines
/// in real time via an observer.
///
/// Output buffering behavior is controlled via
/// <see cref="ProcessOutputHandling"/>. Depending on the
/// configured options, process output may be accumulated
/// in memory and returned as part of the resulting
/// <see cref="ProcessResult"/>, or processed only in
/// streaming mode.
///
/// If the provided <see cref="CancellationToken"/> is cancelled
/// while the process is running, the launched process is
/// terminated as a best-effort operation.
///
/// If cancellation is requested after the process has already
/// completed, the result is still returned and no cancellation
/// is surfaced.
///
/// The launcher performs basic validation of the provided
/// <see cref="ProcessRequest"/> (for example, ensuring a
/// non-empty executable name and an existing working directory),
/// but does not enforce any domain-specific constraints.
/// </remarks>
public sealed class ProcessLauncher : IProcessLauncher
{
    private readonly IProcessOutputObserverResolver _observerResolver;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ProcessLauncher"/> class with the specified
    /// process observer resolver.
    /// </summary>
    /// <param name="observerResolver">
    /// A resolver responsible for translating configured
    /// process observers into output and error handlers
    /// used during process execution.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="observerResolver"/> is <c>null</c>.
    /// </exception>
    public ProcessLauncher(IProcessOutputObserverResolver observerResolver)
    {
        _observerResolver = observerResolver
            ?? throw new ArgumentNullException(nameof(observerResolver));
    }

    /// <inheritdoc />
    public async Task<ProcessResult> RunAsync(
        ProcessRequest request,
        ProcessOutputHandling? output = null,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        ValidateRequest(request);

        var observers = output?.Observers?.ToArray()
            ?? Array.Empty<IProcessOutputObserver>();
        var capture = output?.CaptureOutput ?? true;

        var startInfo = CreateStartInfo(request);
        ApplyEnvironmentVariables(startInfo, request);

        using var process = CreateProcess(startInfo);

        process.Start();

        var (cancellationTask, cancellationRegistration) =
            RegisterProcessTerminationOnCancellation(process, ct);

        try
        {
            var (stdoutLines, stderrLines) = CreateOutputBuffers(capture);

            var (outputTask, errorTask) = StartOutputReaders(
                process,
                request,
                observers,
                stdoutLines,
                stderrLines,
                ct);

            var waitForExitTask = process.WaitForExitAsync(CancellationToken.None);

            await Task.WhenAny(waitForExitTask, cancellationTask);

            // If cancellation was requested, ensure semantic cancellation
            if (!waitForExitTask.IsCompleted)
            {
                ct.ThrowIfCancellationRequested();
            }

            // Ensure the process has fully exited
            await waitForExitTask;

            // Ensure all output has been consumed
            await Task.WhenAll(outputTask, errorTask);

            return ProcessResultHelpers.CreateResult(
                request,
                process.ExitCode,
                stdoutLines,
                stderrLines);
        }
        finally
        {
            cancellationRegistration.Dispose();
        }
    }

    /// <summary>
    /// Creates and configures <see cref="ProcessStartInfo"/>
    /// based on the provided process request.
    /// </summary>
    /// <param name="request">
    /// A <see cref="ProcessRequest"/> containing the executable name,
    /// command-line arguments, and optional working directory
    /// used to configure the process start information.
    /// </param>
    /// <returns>
    /// A configured <see cref="ProcessStartInfo"/> instance
    /// ready to be used for process creation.
    /// </returns>
    private static ProcessStartInfo CreateStartInfo(ProcessRequest request)
    {
        return new ProcessStartInfo
        {
            FileName = request.FileName,
            Arguments = request.Arguments,
            WorkingDirectory = request.WorkingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
    }

    /// <summary>
    /// Applies environment variables from the request
    /// to the provided <see cref="ProcessStartInfo"/>.
    /// </summary>
    /// <param name="startInfo">
    /// The <see cref="ProcessStartInfo"/> to which environment
    /// variables should be applied.
    /// </param>
    /// <param name="request">
    /// A <see cref="ProcessRequest"/> containing optional
    /// environment variables to be added to the process environment.
    /// </param>
    private static void ApplyEnvironmentVariables(
        ProcessStartInfo startInfo,
        ProcessRequest request)
    {
        if (request.EnvironmentVariables == null)
        {
            return;
        }

        foreach (var (key, value) in request.EnvironmentVariables)
        {
            startInfo.Environment[key] = value;
        }
    }

    /// <summary>
    /// Creates a <see cref="Process"/> instance
    /// configured with the specified start information.
    /// </summary>
    /// <param name="startInfo">
    /// The <see cref="ProcessStartInfo"/> used to configure
    /// the created process instance.
    /// </param>
    /// <returns>
    /// A <see cref="Process"/> configured with the provided
    /// start information.
    /// </returns>
    private static Process CreateProcess(ProcessStartInfo startInfo)
    {
        return new Process
        {
            StartInfo = startInfo
        };
    }

    /// <summary>
    /// Validates required fields of a process request.
    /// </summary>
    /// <param name="request">The process request to validate.</param>
    private static void ValidateRequest(ProcessRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new ArgumentException(
                "Process request must specify a non-empty file name.",
                nameof(request));
        }

        if (!string.IsNullOrWhiteSpace(request.WorkingDirectory)
            && !Directory.Exists(request.WorkingDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Working directory was not found: '{request.WorkingDirectory}'.");
        }
    }

    /// <summary>
    /// Registers termination of the specified process
    /// when the provided cancellation token is cancelled.
    /// </summary>
    /// <param name="process">
    /// The process to be terminated on cancellation.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that triggers process termination.
    /// </param>
    /// <returns>
    /// A task that completes when cancellation is requested.
    /// </returns>
    private static (Task CancellationTask, CancellationTokenRegistration Registration)
        RegisterProcessTerminationOnCancellation(
        Process process,
        CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var registration = ct.Register(() =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Best-effort termination
            }
            finally
            {
                tcs.TrySetCanceled(ct);
            }
        });

        return (tcs.Task, registration);
    }

    /// <summary>
    /// Creates collections for capturing process standard
    /// output and error streams based on the specified
    /// capture configuration.
    /// </summary>
    /// <param name="captureOutput">
    /// A value indicating whether process output
    /// should be accumulated in memory.
    /// </param>
    /// <returns>
    /// A tuple containing collections for standard output
    /// and standard error lines, or <c>null</c> values when
    /// output capturing is disabled.
    /// </returns>
    /// <remarks>
    /// When <paramref name="captureOutput"/> is set to
    /// <c>false</c>, no output is accumulated in memory
    /// and both returned collections are <c>null</c>.
    ///
    /// This method centralizes output buffer creation
    /// and allows the caller to uniformly handle both
    /// buffered and streaming-only execution modes.
    /// </remarks>
    private static (List<string>? Stdout, List<string>? Stderr)
        CreateOutputBuffers(bool captureOutput)
    {
        var stdoutLines = captureOutput
            ? new List<string>()
            : null;

        var stderrLines = captureOutput
            ? new List<string>()
            : null;

        return (stdoutLines, stderrLines);
    }

    /// <summary>
    /// Starts asynchronous reading of the process standard
    /// output and standard error streams and dispatches
    /// produced lines to the configured process observers.
    /// </summary>
    /// <param name="process">
    /// The running <see cref="Process"/> whose output streams
    /// are being read.
    /// </param>
    /// <param name="request">
    /// The <see cref="ProcessRequest"/> associated with the
    /// currently executing process.
    /// </param>
    /// <param name="observers">
    /// An optional collection of process observers that receive
    /// output and error notifications produced during execution.
    /// </param>
    /// <param name="stdoutLines">
    /// A collection used to accumulate standard output lines,
    /// or <c>null</c> if output capturing is disabled.
    /// </param>
    /// <param name="stderrLines">
    /// A collection used to accumulate standard error lines,
    /// or <c>null</c> if output capturing is disabled.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that can be used to cancel
    /// the stream reading operations.
    /// </param>
    /// <returns>
    /// A tuple containing the tasks responsible for asynchronously
    /// reading the standard output and standard error streams.
    /// </returns>
    /// <remarks>
    /// The returned tasks may run concurrently and may invoke
    /// observer callbacks from multiple threads. Implementations
    /// of process observer interfaces are expected to be
    /// thread-safe.
    ///
    /// Dispatching of output lines to observer callbacks is
    /// performed via the configured observer resolver, allowing
    /// different observer types to be supported without coupling
    /// this method to specific observer interfaces.
    /// </remarks>
    private (Task OutputTask, Task ErrorTask) StartOutputReaders(
        Process process,
        ProcessRequest request,
        IReadOnlyList<IProcessOutputObserver> observers,
        List<string>? stdoutLines,
        List<string>? stderrLines,
        CancellationToken ct)
    {
        var outputObserverHandler = CreateOutputHandler(request, observers);
        var errorObserverHandler = CreateErrorHandler(request, observers);

        var outputTask = ReadStreamAsync(
            process.StandardOutput,
            outputObserverHandler,
            stdoutLines,
            ct);

        var errorTask = ReadStreamAsync(
            process.StandardError,
            errorObserverHandler,
            stderrLines,
            ct);

        return (outputTask, errorTask);
    }

    /// <summary>
    /// Creates a handler that dispatches standard output
    /// lines produced by the process to the configured
    /// process observers.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ProcessRequest"/> associated with the
    /// currently executing process.
    /// </param>
    /// <param name="observers">
    /// An optional collection of process observers that
    /// receive standard output notifications.
    /// </param>
    /// <returns>
    /// An <see cref="Action{T}"/> that forwards standard
    /// output lines to all applicable observer callbacks.
    /// </returns>
    /// <remarks>
    /// The returned handler dispatches output lines using
    /// the configured <see cref="IProcessOutputObserverResolver"/>,
    /// which determines how each observer is notified based
    /// on the interfaces it implements.
    ///
    /// If <paramref name="observers"/> is <c>null</c> or empty,
    /// the returned handler performs no action.
    ///
    /// The returned handler is thread-safe and may be
    /// invoked concurrently from multiple threads.
    /// </remarks>
    private Action<string> CreateOutputHandler(
        ProcessRequest request,
        IReadOnlyList<IProcessOutputObserver> observers)
    {
        return _observerResolver.ResolveOutputHandler(request, observers);
    }

    /// <summary>
    /// Creates a handler that dispatches standard error
    /// lines produced by the process to the configured
    /// process observers.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ProcessRequest"/> associated with the
    /// currently executing process.
    /// </param>
    /// <param name="observers">
    /// An optional collection of process observers that
    /// receive standard error notifications.
    /// </param>
    /// <returns>
    /// An <see cref="Action{T}"/> that forwards standard
    /// error lines to all applicable observer callbacks.
    /// </returns>
    /// <remarks>
    /// The returned handler dispatches error lines using
    /// the configured <see cref="IProcessOutputObserverResolver"/>,
    /// which determines how each observer is notified based
    /// on the interfaces it implements.
    ///
    /// If <paramref name="observers"/> is <c>null</c> or empty,
    /// the returned handler performs no action.
    ///
    /// The returned handler is thread-safe and may be
    /// invoked concurrently from multiple threads.
    /// </remarks>
    private Action<string> CreateErrorHandler(
        ProcessRequest request,
        IReadOnlyList<IProcessOutputObserver> observers)
    {
        return _observerResolver.ResolveErrorHandler(request, observers);
    }

    /// <summary>
    /// Reads a redirected process stream line by line,
    /// forwarding each line to the provided callback
    /// and storing it in the specified buffer.
    /// </summary>
    /// <param name="reader">
    /// The <see cref="StreamReader"/> associated with a redirected
    /// process output or error stream.
    /// </param>
    /// <param name="observerHandler">
    /// A callback invoked for each line read from the stream.
    /// This handler represents the observer dispatch method.
    /// </param>
    /// <param name="buffer">
    /// A collection used to store all lines read from the stream
    /// for later aggregation.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that can be used to cancel the
    /// stream reading operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous stream reading
    /// operation.
    /// </returns>
    private static async Task ReadStreamAsync(
        StreamReader reader,
        Action<string> observerHandler,
        List<string>? buffer,
        CancellationToken ct)
    {
        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(ct);
            if (line == null)
            {
                break;
            }

            if (buffer is not null)
            {
                buffer.Add(line);
            }

            observerHandler(line);
        }
    }
}
