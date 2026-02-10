using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Models;

/// <summary>
/// Defines how process output is handled during execution.
/// </summary>
/// <remarks>
/// This class encapsulates output-related execution policies,
/// including whether standard output and standard error streams
/// are captured in memory and which observers receive output
/// notifications during process execution.
///
/// The <see cref="Observers"/> collection may contain zero or
/// more observer instances implementing various process
/// observer interfaces. Each observer is notified of output
/// events according to the interfaces it implements.
///
/// Dispatching of output events to the appropriate observer
/// callbacks is performed by an external resolver component,
/// allowing output handling behavior to evolve independently
/// from process execution logic.
///
/// By separating output handling concerns from
/// <see cref="ProcessRequest"/>, this type allows process
/// execution parameters and output policies to evolve
/// independently.
/// </remarks>
public sealed class ProcessOutputHandling
{
    /// <summary>
    /// Gets the collection of observers that receive process
    /// output notifications during execution.
    /// </summary>
    /// <remarks>
    /// The collection may contain observers implementing
    /// different observer interfaces (for example, contextual
    /// or non-contextual output observers).
    ///
    /// If the collection is <c>null</c> or empty, process output
    /// is not observed, but may still be captured in memory
    /// depending on <see cref="CaptureOutput"/>.
    /// </remarks>
    public IReadOnlyList<IProcessOutputObserver>? Observers { get; }

    /// <summary>
    /// Gets a value indicating whether the standard output
    /// and standard error streams should be captured and
    /// returned in <see cref="ProcessResult"/>.
    /// </summary>
    /// <remarks>
    /// When set to <c>false</c>, process output is still
    /// streamed to the configured <see cref="Observers"/>,
    /// but is not accumulated in memory.
    ///
    /// The default value is <c>true</c>.
    /// </remarks>
    public bool CaptureOutput { get; } = true;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ProcessOutputHandling"/> class
    /// with the specified output capture behavior.
    /// </summary>
    /// <param name="captureOutput">
    /// A value indicating whether process output
    /// should be captured in memory.
    /// </param>
    public ProcessOutputHandling(bool captureOutput)
    {
        CaptureOutput = captureOutput;
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ProcessOutputHandling"/> class
    /// with a single process observer.
    /// </summary>
    /// <param name="observer">
    /// An observer that receives process output
    /// notifications during execution.
    /// </param>
    public ProcessOutputHandling(IProcessOutputObserver observer)
    {
        if (observer is not null)
            Observers = [observer];
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ProcessOutputHandling"/> class
    /// with a single process observer and the specified
    /// output capture behavior.
    /// </summary>
    /// <param name="observer">
    /// An observer that receives process output
    /// notifications during execution.
    /// </param>
    /// <param name="captureOutput">
    /// A value indicating whether process output
    /// should be captured in memory.
    /// </param>
    public ProcessOutputHandling(
        IProcessOutputObserver observer,
        bool captureOutput)
        : this(observer)
    {
        CaptureOutput = captureOutput;
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ProcessOutputHandling"/> class
    /// with the specified collection of process observers.
    /// </summary>
    /// <param name="observers">
    /// A collection of observers that receive process
    /// output notifications during execution.
    /// </param>
    public ProcessOutputHandling(IReadOnlyList<IProcessOutputObserver> observers)
    {
        if (observers is not null)
            Observers = observers;
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ProcessOutputHandling"/> class
    /// with the specified collection of process observers
    /// and output capture behavior.
    /// </summary>
    /// <param name="observers">
    /// A collection of observers that receive process
    /// output notifications during execution.
    /// </param>
    /// <param name="captureOutput">
    /// A value indicating whether process output
    /// should be captured in memory.
    /// </param>
    public ProcessOutputHandling(
        IReadOnlyList<IProcessOutputObserver> observers,
        bool captureOutput)
        : this(observers)
    {
        CaptureOutput = captureOutput;
    }
}

