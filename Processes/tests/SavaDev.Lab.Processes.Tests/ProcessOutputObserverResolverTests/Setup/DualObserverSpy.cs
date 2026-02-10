using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.ProcessOutputObserverResolverTests.Setup;

/// <summary>
/// A test spy that implements both basic and contextual
/// output observer interfaces and records received calls.
/// </summary>
internal sealed class DualObserverSpy
: IProcessOutputBasicObserver,
  IProcessOutputContextualObserver
{
    /// <summary>
    /// Gets the number of basic output callbacks received.
    /// </summary>
    public int BasicOutputCalls { get; private set; }
    /// <summary>
    /// Gets the number of basic error callbacks received.
    /// </summary>
    public int BasicErrorCalls { get; private set; }

    /// <summary>
    /// Gets the number of contextual output callbacks received.
    /// </summary>
    public int AwareOutputCalls { get; private set; }
    /// <summary>
    /// Gets the number of contextual error callbacks received.
    /// </summary>
    public int AwareErrorCalls { get; private set; }

    /// <summary>
    /// Gets the last request observed by a contextual callback.
    /// </summary>
    public ProcessRequest? LastRequest { get; private set; }
    /// <summary>
    /// Gets the last line observed by any contextual callback.
    /// </summary>
    public string? LastLine { get; private set; }

    /// <summary>
    /// Records a non-contextual standard output notification.
    /// </summary>
    /// <param name="line">The output line.</param>
    public void OnOutputLine(string line)
        => BasicOutputCalls++;

    /// <summary>
    /// Records a non-contextual standard error notification.
    /// </summary>
    /// <param name="line">The error line.</param>
    public void OnErrorLine(string line)
        => BasicErrorCalls++;

    /// <summary>
    /// Records a contextual standard output notification.
    /// </summary>
    /// <param name="request">The originating process request.</param>
    /// <param name="line">The output line.</param>
    public void OnOutputLine(ProcessRequest request, string line)
    {
        AwareOutputCalls++;
        LastRequest = request;
        LastLine = line;
    }

    /// <summary>
    /// Records a contextual standard error notification.
    /// </summary>
    /// <param name="request">The originating process request.</param>
    /// <param name="line">The error line.</param>
    public void OnErrorLine(ProcessRequest request, string line)
    {
        AwareErrorCalls++;
        LastRequest = request;
        LastLine = line;
    }
}

