using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Observers.Resolver;

/// <summary>
/// Resolves process output dispatch handlers based on
/// the provided process request and registered observers.
/// </summary>
/// <remarks>
/// This interface encapsulates the logic required to map
/// a collection of <see cref="IProcessOutputObserver"/> instances
/// to concrete output and error handlers used during
/// process execution.
///
/// Implementations are responsible for determining which
/// observers are capable of handling standard output or
/// standard error events and for producing handler
/// delegates that correctly dispatch those events.
///
/// By centralizing this logic, the process launcher
/// remains decoupled from specific observer interfaces
/// and dispatch strategies.
/// </remarks>
public interface IProcessOutputObserverResolver
{
    /// <summary>
    /// Creates a handler that dispatches standard output
    /// lines produced by a process.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ProcessRequest"/> associated with the
    /// currently executing process.
    /// </param>
    /// <param name="observers">
    /// A collection of process observers that may receive
    /// standard output notifications.
    /// </param>
    /// <returns>
    /// An <see cref="Action{T}"/> that accepts a single
    /// output line and dispatches it to the appropriate
    /// observers.
    /// </returns>
    /// <remarks>
    /// The returned handler may be invoked concurrently
    /// from multiple threads. Observer implementations
    /// are expected to ensure their own thread safety.
    ///
    /// If no observers are capable of handling standard
    /// output, the returned handler performs no action.
    /// </remarks>
    Action<string> ResolveOutputHandler(
        ProcessRequest request,
        IReadOnlyList<IProcessOutputObserver> observers);

    /// <summary>
    /// Creates a handler that dispatches standard error
    /// lines produced by a process.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ProcessRequest"/> associated with the
    /// currently executing process.
    /// </param>
    /// <param name="observers">
    /// A collection of process observers that may receive
    /// standard error notifications.
    /// </param>
    /// <returns>
    /// An <see cref="Action{T}"/> that accepts a single
    /// error output line and dispatches it to the appropriate
    /// observers.
    /// </returns>
    /// <remarks>
    /// The returned handler may be invoked concurrently
    /// from multiple threads. Observer implementations
    /// are expected to ensure their own thread safety.
    ///
    /// If no observers are capable of handling standard
    /// error output, the returned handler performs no action.
    /// </remarks>
    Action<string> ResolveErrorHandler(
        ProcessRequest request,
        IReadOnlyList<IProcessOutputObserver> observers);
}
