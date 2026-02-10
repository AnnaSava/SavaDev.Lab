using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.ProcessOutputObserverResolverTests.Setup;

/// <summary>
/// A test spy that records standard output notifications.
/// </summary>
internal sealed class OutputObserverSpy : IProcessOutputBasicObserver
{
    /// <summary>
    /// Gets the number of output callbacks received.
    /// </summary>
    public int OutputCalls { get; private set; }
    /// <summary>
    /// Gets the last output line observed.
    /// </summary>
    public string? LastLine { get; private set; }

    /// <summary>
    /// Records a standard output notification.
    /// </summary>
    /// <param name="line">The output line.</param>
    public void OnOutputLine(string line)
    {
        OutputCalls++;
        LastLine = line;
    }

    /// <summary>
    /// Ignores standard error notifications for this spy.
    /// </summary>
    /// <param name="line">The error line.</param>
    public void OnErrorLine(string line)
    {
        // Not used in this test
    }
}

