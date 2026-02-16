using System.Diagnostics;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Demo.Observers;

/// <summary>
/// A process output observer that measures execution timing
/// based on observed process output events.
/// </summary>
/// <remarks>
/// This observer tracks the time elapsed between the first
/// observed output event and the completion of the process.
/// It does not interfere with process execution and relies
/// solely on observer callbacks to infer timing information.
///
/// The measured values are intended for diagnostics and
/// demonstration purposes and should not be treated as
/// precise process profiling metrics.
/// </remarks>
public sealed class TimingProcessObserver : IProcessOutputBasicObserver
{
    private readonly Stopwatch _stopwatch = new();
    private int _started;

    /// <summary>
    /// Gets the total elapsed time between the first
    /// observed output event and the last one.
    /// </summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Gets a value indicating whether timing has started.
    /// </summary>
    public bool HasStarted => _stopwatch.IsRunning;

    /// <inheritdoc />
    public void OnOutputLine(string line) => StartIfNeeded();

    /// <inheritdoc />
    public void OnErrorLine(string line) => StartIfNeeded();

    private void StartIfNeeded()
    {
        if (Interlocked.Exchange(ref _started, 1) == 0)
        {
            _stopwatch.Start();
        }
    }

    /// <summary>
    /// Stops timing explicitly.
    /// </summary>
    /// <remarks>
    /// This method can be called after process execution
    /// completes to finalize timing measurements.
    /// </remarks>
    public void Stop()
    {
        if (_stopwatch.IsRunning)
        {
            _stopwatch.Stop();
        }
    }
}

