using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Observers;

/// <summary>
/// A console-based process observer that writes standard
/// output and standard error lines together with execution
/// context information.
/// </summary>
/// <remarks>
/// This observer writes process output to the console,
/// prefixing each line with the identifier of the
/// originating <see cref="ProcessRequest"/>.
///
/// Including the request identifier allows output produced
/// by different processes to be distinguished when multiple
/// processes are executed concurrently.
///
/// Output notifications may be delivered concurrently from
/// multiple processes. Writing to the console relies on the
/// thread-safety guarantees of the underlying
/// <see cref="Console"/> implementation and does not perform
/// additional synchronization.
/// </remarks>
public sealed class ConsoleContextualObserver
    : IProcessOutputContextualObserver
{
    /// <summary>
    /// Writes a line produced by the standard output stream of a
    /// process to the console, including the originating request
    /// identifier.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ProcessRequest"/> that produced the output.
    /// - used to correlate the line with a specific process execution.
    /// </param>
    /// <param name="line">
    /// A single line read from the process standard output stream.
    /// </param>
    /// <remarks>
    /// The output line is written to the console with a prefix
    /// that includes the request identifier, allowing output
    /// from multiple concurrent processes to be distinguished
    /// easily during execution.
    /// </remarks>
    public void OnOutputLine(ProcessRequest request, string line)
        => Console.WriteLine($"[OUT] [{request.Id}] {line}");

    /// <summary>
    /// Writes a line produced by the standard error stream of a
    /// process to the console, including the originating request
    /// identifier.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ProcessRequest"/> that produced the error
    /// output and identifies the process execution.
    /// - used to correlate the line with a specific process execution.
    /// </param>
    /// <param name="line">
    /// A single line read from the process standard error stream.
    /// </param>
    /// <remarks>
    /// The error line is written to <see cref="Console.Error"/>
    /// with a prefix that includes the request identifier,
    /// making it possible to distinguish error output from
    /// multiple concurrent processes.
    /// </remarks>
    public void OnErrorLine(ProcessRequest request, string line)
        => Console.Error.WriteLine($"[ERR] [{request.Id}] {line}");
}
