using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Observers;

/// <summary>
/// Observes process output together with execution
/// context information.
/// </summary>
/// <remarks>
/// Implementations of this interface receive standard output
/// and standard error lines along with the originating
/// <see cref="ProcessRequest"/>.
///
/// The additional execution context allows implementations
/// to correlate output with a specific process instance,
/// which is especially useful when multiple processes are
/// executed concurrently or when aggregating output from
/// different sources.
///
/// The provided <see cref="ProcessRequest"/> represents
/// execution metadata and must be treated as read-only.
/// Implementations must not modify its state.
///
/// This interface is intended for contextual observation
/// scenarios and is resolved separately from non-contextual
/// output observers by the process observer resolver.
/// </remarks>
public interface IProcessOutputContextualObserver : IProcessOutputObserver
{
    /// <summary>
    /// Called when a line is written to the standard output
    /// stream of a process.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ProcessRequest"/> that produced the output.
    /// </param>
    /// <param name="line">
    /// The output line written to standard output.
    /// </param>
    void OnOutputLine(ProcessRequest request, string line);

    /// <summary>
    /// Called when a line is written to the standard error
    /// stream of a process.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ProcessRequest"/> that produced the error output.
    /// </param>
    /// <param name="line">
    /// The error line written to standard error.
    /// </param>
    void OnErrorLine(ProcessRequest request, string line);
}

