using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Observers;

/// <summary>
/// Observes line-by-line standard output and standard error
/// produced by a running process.
/// </summary>
/// <remarks>
/// Implementations of this interface receive process output
/// without execution context information and are intended
/// for simple, non-contextual scenarios such as logging,
/// progress reporting, or streaming output to the console.
///
/// More advanced scenarios that require access to the
/// originating <see cref="ProcessRequest"/> should use
/// a contextual observer interface instead.
/// </remarks>
public interface IProcessOutputBasicObserver : IProcessOutputObserver
{
    /// <summary>
    /// Called when a line is written to standard output.
    /// </summary>
    /// <param name="line">The output line.</param>
    void OnOutputLine(string line);

    /// <summary>
    /// Called when a line is written to standard error.
    /// </summary>
    /// <param name="line">The error line.</param>
    void OnErrorLine(string line);
}
