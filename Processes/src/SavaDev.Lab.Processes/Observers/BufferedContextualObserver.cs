using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Observers;

/// <summary>
/// A process output observer that buffers standard output
/// and standard error lines together with execution
/// context information.
/// </summary>
/// <remarks>
/// This observer accumulates output lines in memory while
/// the process is running and exposes them for inspection
/// after execution completes, preserving the originating
/// <see cref="ProcessRequest"/> for each entry.
///
/// Unlike <see cref="ProcessResult"/>, buffering performed
/// by this observer is entirely controlled by the consumer
/// and is independent of the process launcher's output
/// capture configuration.
///
/// The observer is thread-safe and can be used with
/// concurrently read process streams.
/// </remarks>
public sealed class BufferedContextualObserver : IProcessOutputContextualObserver
{
    /// <summary>
    /// Synchronization object for thread-safe buffer access.
    /// </summary>
    private readonly object _sync = new();
    /// <summary>
    /// In-memory buffer for contextual standard output lines.
    /// </summary>
    private readonly List<(ProcessRequest Request, string Line)> _stdout = new();
    /// <summary>
    /// In-memory buffer for contextual standard error lines.
    /// </summary>
    private readonly List<(ProcessRequest Request, string Line)> _stderr = new();

    /// <summary>
    /// Gets the buffered standard output lines observed
    /// during process execution together with their
    /// originating requests.
    /// </summary>
    /// <remarks>
    /// The returned collection represents a snapshot of
    /// the buffered output at the time of access.
    /// Modifications to the returned collection do not
    /// affect the internal observer state.
    /// </remarks>
    public IReadOnlyList<(ProcessRequest Request, string Line)> StandardOutputLines
    {
        get
        {
            lock (_sync)
            {
                return _stdout.ToArray();
            }
        }
    }

    /// <summary>
    /// Gets the buffered standard error lines observed
    /// during process execution together with their
    /// originating requests.
    /// </summary>
    /// <remarks>
    /// The returned collection represents a snapshot of
    /// the buffered error output at the time of access.
    /// Modifications to the returned collection do not
    /// affect the internal observer state.
    /// </remarks>
    public IReadOnlyList<(ProcessRequest Request, string Line)> StandardErrorLines
    {
        get
        {
            lock (_sync)
            {
                return _stderr.ToArray();
            }
        }
    }

    /// <summary>
    /// Records a standard output line produced by the process
    /// together with the originating request.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ProcessRequest"/> that produced the output.
    /// </param>
    /// <param name="line">
    /// A single line read from the process standard output stream.
    /// </param>
    /// <remarks>
    /// The line and request are added to the internal output buffer
    /// in a thread-safe manner. Synchronization is required because
    /// output and error streams may be read concurrently.
    /// </remarks>
    public void OnOutputLine(ProcessRequest request, string line)
    {
        lock (_sync)
        {
            _stdout.Add((request, line));
        }
    }

    /// <summary>
    /// Records a standard error line produced by the process
    /// together with the originating request.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ProcessRequest"/> that produced the error output.
    /// </param>
    /// <param name="line">
    /// A single line read from the process standard error stream.
    /// </param>
    /// <remarks>
    /// The line and request are added to the internal error buffer
    /// in a thread-safe manner. Synchronization is required because
    /// output and error streams may be read concurrently.
    /// </remarks>
    public void OnErrorLine(ProcessRequest request, string line)
    {
        lock (_sync)
        {
            _stderr.Add((request, line));
        }
    }
}
