using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Observers.Resolver;

/// <summary>
/// Default implementation of <see cref="IProcessOutputObserverResolver"/>
/// that dispatches process output to registered observers
/// based on their supported observer interfaces.
/// </summary>
/// <remarks>
/// This resolver examines the provided collection of
/// <see cref="IProcessOutputObserver"/> instances and builds
/// output and error handlers that route lines to the
/// appropriate observer callbacks.
///
/// Observers that implement <see cref="IProcessOutputContextualObserver"/>
/// receive output together with the originating
/// <see cref="ProcessRequest"/>. Observers that implement
/// only <see cref="IProcessOutputBasicObserver"/> receive
/// non-contextual output notifications.
///
/// The resolver performs no synchronization and assumes
/// that observer implementations are responsible for
/// their own thread safety. The produced handlers are
/// safe to invoke concurrently.
/// </remarks>
public sealed class ProcessOutputObserverResolver : IProcessOutputObserverResolver
{
    /// <summary>
    /// Resolves a handler that dispatches standard output
    /// lines to the provided process observers.
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
    /// standard output line and dispatches it to all
    /// applicable observers.
    /// </returns>
    /// <remarks>
    /// If the observer collection is empty, the returned
    /// handler performs no action.
    ///
    /// Observers implementing
    /// <see cref="IProcessOutputContextualObserver"/> are notified
    /// with execution context information, while observers
    /// implementing <see cref="IProcessOutputBasicObserver"/>
    /// receive non-contextual notifications.
    ///
    /// The returned handler may be invoked concurrently
    /// from multiple threads.
    /// </remarks>
    public Action<string> ResolveOutputHandler(
        ProcessRequest request,
        IReadOnlyList<IProcessOutputObserver> observers)
    {
        if (observers.Count == 0)
        {
            return static _ => { };
        }

        return line =>
        {
            foreach (var observer in observers)
            {
                if (observer is IProcessOutputContextualObserver aware)
                {
                    aware.OnOutputLine(request, line);
                }
                else if (observer is IProcessOutputBasicObserver basic)
                {
                    basic.OnOutputLine(line);
                }
            }
        };
    }

    /// <summary>
    /// Resolves a handler that dispatches standard error
    /// lines to the provided process observers.
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
    /// standard error line and dispatches it to all
    /// applicable observers.
    /// </returns>
    /// <remarks>
    /// If the observer collection is empty, the returned
    /// handler performs no action.
    ///
    /// Observers implementing
    /// <see cref="IProcessOutputContextualObserver"/> are notified
    /// with execution context information, while observers
    /// implementing <see cref="IProcessOutputBasicObserver"/>
    /// receive non-contextual notifications.
    ///
    /// The returned handler may be invoked concurrently
    /// from multiple threads.
    /// </remarks>
    public Action<string> ResolveErrorHandler(
        ProcessRequest request,
        IReadOnlyList<IProcessOutputObserver> observers)
    {
        if (observers.Count == 0)
        {
            return static _ => { };
        }

        return line =>
        {
            foreach (var observer in observers)
            {
                if (observer is IProcessOutputContextualObserver aware)
                {
                    aware.OnErrorLine(request, line);
                }
                else if (observer is IProcessOutputBasicObserver basic)
                {
                    basic.OnErrorLine(line);
                }
            }
        };
    }
}
