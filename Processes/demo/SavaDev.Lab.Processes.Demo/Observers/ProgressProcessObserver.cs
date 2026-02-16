using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Demo.Observers;

/// <summary>
/// A process output observer that tracks progress
/// when a large number of output lines is produced.
/// </summary>
public sealed class ProgressProcessObserver : IProcessOutputBasicObserver
{
    private int _lineCount;

    /// <summary>
    /// Gets the total number of lines observed.
    /// </summary>
    public int LineCount => _lineCount;

    /// <inheritdoc />
    public void OnOutputLine(string line)
    {
        var count = Interlocked.Increment(ref _lineCount);

        if (count % 100 == 0)
        {
            Console.WriteLine($"[progress] {count} lines received...");
        }
    }

    /// <inheritdoc />
    public void OnErrorLine(string line)
    {
        // No-op for this scenario
    }
}
