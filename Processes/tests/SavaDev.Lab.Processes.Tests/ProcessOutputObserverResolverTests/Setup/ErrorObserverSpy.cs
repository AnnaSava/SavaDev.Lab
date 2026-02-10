using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.ProcessOutputObserverResolverTests.Setup;

/// <summary>
/// A test spy that records standard error notifications.
/// </summary>
internal sealed class ErrorObserverSpy : IProcessOutputBasicObserver
{
    /// <summary>
    /// Gets the number of error callbacks received.
    /// </summary>
    public int ErrorCalls { get; private set; }
    /// <summary>
    /// Gets the last error line observed.
    /// </summary>
    public string? LastLine { get; private set; }

    /// <summary>
    /// Ignores standard output notifications for this spy.
    /// </summary>
    /// <param name="line">The output line.</param>
    public void OnOutputLine(string line)
    {
        // Not used in this test
    }

    /// <summary>
    /// Records a standard error notification.
    /// </summary>
    /// <param name="line">The error line.</param>
    public void OnErrorLine(string line)
    {
        ErrorCalls++;
        LastLine = line;
    }
}

