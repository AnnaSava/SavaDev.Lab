using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Demo.Observers;

/// <summary>
/// A custom process output observer that counts
/// standard output and standard error lines produced
/// by an external process.
/// </summary>
/// <remarks>
/// This observer demonstrates a simple aggregation
/// strategy for process output.
///
/// The observer may receive callbacks from multiple
/// threads concurrently, as standard output and
/// standard error streams can be read in parallel.
/// For this reason, atomic operations from
/// <see cref="Interlocked"/> are used to update
/// internal counters in a thread-safe manner.
///
/// This implementation is intentionally minimal and
/// serves as an example of how consumers can implement
/// <see cref="IProcessOutputBasicObserver"/> to collect
/// custom statistics without affecting the process
/// launcher itself.
/// </remarks>
public sealed class CountingProcessObserver : IProcessOutputBasicObserver
{
    private int _stdoutCount;
    private int _stderrCount;

    /// <summary>
    /// Gets the total number of standard output lines
    /// observed during process execution.
    /// </summary>
    /// <remarks>
    /// The returned value is updated using atomic
    /// operations and can be safely accessed after
    /// the process has completed.
    /// </remarks>
    public int StandardOutputLineCount => _stdoutCount;

    /// <summary>
    /// Gets the total number of standard error lines
    /// observed during process execution.
    /// </summary>
    /// <remarks>
    /// The returned value is updated using atomic
    /// operations and can be safely accessed after
    /// the process has completed.
    /// </remarks>
    public int StandardErrorLineCount => _stderrCount;

    /// <summary>
    /// Called for each line written to the standard
    /// output stream of the process.
    /// </summary>
    /// <param name="line">
    /// A single line of text produced by the process.
    /// </param>
    /// <remarks>
    /// This method may be invoked concurrently with
    /// <see cref="OnErrorLine"/> and potentially from
    /// different threads. The internal counter is
    /// incremented using <see cref="Interlocked"/>
    /// to ensure thread safety without explicit locks.
    /// </remarks>
    public void OnOutputLine(string line)
        => Interlocked.Increment(ref _stdoutCount);

    /// <summary>
    /// Called for each line written to the standard
    /// error stream of the process.
    /// </summary>
    /// <param name="line">
    /// A single line of text produced by the process.
    /// </param>
    /// <remarks>
    /// This method may be invoked concurrently with
    /// <see cref="OnOutputLine"/> and potentially from
    /// different threads. The internal counter is
    /// incremented using <see cref="Interlocked"/>
    /// to ensure thread safety without explicit locks.
    /// </remarks>
    public void OnErrorLine(string line)
        => Interlocked.Increment(ref _stderrCount);
}

