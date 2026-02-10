using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes;

/// <summary>
/// Provides an abstraction for launching external processes
/// and capturing their execution results.
/// </summary>
/// <remarks>
/// This interface defines a minimal contract for executing
/// operating system processes in an asynchronous and
/// cancellation-aware manner.
///
/// If the provided <see cref="CancellationToken"/> is cancelled
/// while the process is running, the process is terminated
/// as a best-effort operation.
///
/// If cancellation is requested after the process has already
/// completed, the result is still returned and no cancellation
/// is surfaced.
///
/// Implementations may optionally provide real-time access
/// to process output during execution, but do not impose
/// any domain-specific semantics.
///
/// Output buffering behavior is controlled via
/// <see cref="ProcessOutputHandling"/>. When output capturing
/// is disabled, standard output and standard error are not
/// accumulated in memory and are available only via the
/// configured <see cref="IProcessOutputObserver"/> instances.
/// </remarks>
public interface IProcessLauncher
{
    /// <summary>
    /// Runs an external process using the specified request parameters.
    /// </summary>
    /// <param name="request">
    /// A <see cref="ProcessRequest"/> describing the executable,
    /// arguments, working directory, and optional environment variables
    /// required to start the process.
    /// </param>
    /// <param name="output">
    /// Optional output handling options that control whether
    /// process output is captured in memory and how output
    /// lines are observed during execution.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that can be used to cancel the process execution.
    /// If cancellation is requested while the process is running,
    /// the process is terminated.
    /// If cancellation is requested after the process has completed,
    /// the result is still returned.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="ProcessResult"/> with
    /// the exit code and any captured standard output and error streams
    /// after the process has completed.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown if the operation is cancelled while the process
    /// is still running.
    /// </exception>
    Task<ProcessResult> RunAsync(
        ProcessRequest request,
        ProcessOutputHandling? output = null,
        CancellationToken ct = default);
}
