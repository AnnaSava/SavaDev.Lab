using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Observers;

/// <summary>
/// A process output observer that buffers standard output
/// and standard error lines during process execution.
/// </summary>
/// <remarks>
/// This observer accumulates output lines in memory while
/// the process is running and exposes them for inspection
/// after execution completes.
///
/// Unlike <see cref="ProcessResult"/>, buffering performed
/// by this observer is entirely controlled by the consumer
/// and is independent of the process launcher's output
/// capture configuration.
///
/// The observer is thread-safe and can be used with
/// concurrently read process streams.
/// </remarks>
public sealed class BufferedBasicObserver : IProcessOutputBasicObserver
{
    /// <summary>
    /// Synchronization object for thread-safe buffer access.
    /// </summary>
    private readonly object _sync = new();
    /// <summary>
    /// In-memory buffer for standard output lines.
    /// </summary>
    private readonly List<string> _stdout = new();
    /// <summary>
    /// In-memory buffer for standard error lines.
    /// </summary>
    private readonly List<string> _stderr = new();

    /// <summary>
    /// Gets the buffered standard output lines observed
    /// during process execution.
    /// </summary>
    /// <remarks>
    /// The returned collection represents a snapshot of
    /// the buffered output at the time of access.
    /// Modifications to the returned collection do not
    /// affect the internal observer state.
    /// </remarks>
    public IReadOnlyList<string> StandardOutputLines
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
    /// during process execution.
    /// </summary>
    /// <remarks>
    /// The returned collection represents a snapshot of
    /// the buffered error output at the time of access.
    /// Modifications to the returned collection do not
    /// affect the internal observer state.
    /// </remarks>
    public IReadOnlyList<string> StandardErrorLines
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
    /// Records a standard output line produced by the process.
    /// </summary>
    /// <param name="line">
    /// A single line read from the process standard output stream.
    /// </param>
    /// <remarks>
    /// The line is added to the internal output buffer in a
    /// thread-safe manner. Synchronization is required because
    /// output and error streams may be read concurrently.
    /// </remarks>
    public void OnOutputLine(string line)
    {
        lock (_sync)
        {
            _stdout.Add(line);
        }
    }

    /// <summary>
    /// Records a standard error line produced by the process.
    /// </summary>
    /// <param name="line">
    /// A single line read from the process standard error stream.
    /// </param>
    /// <remarks>
    /// The line is added to the internal error buffer in a
    /// thread-safe manner. Synchronization is required because
    /// output and error streams may be read concurrently.
    /// </remarks>
    public void OnErrorLine(string line)
    {
        lock (_sync)
        {
            _stderr.Add(line);
        }
    }
}
