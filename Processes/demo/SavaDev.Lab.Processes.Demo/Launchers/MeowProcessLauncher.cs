using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Demo.Launchers;

/// <summary>
/// A process launcher that simulates a cat
/// instead of launching real operating system processes.
/// </summary>
/// <remarks>
/// This launcher does not start external processes.
/// Instead, it enters a playful, infinite "cat behavior"
/// loop and periodically emits cat-related output
/// events to the configured process observers.
///
/// Execution continues until cancellation is requested,
/// at which point the launcher exits gracefully and
/// returns a completed <see cref="ProcessResult"/>.
///
/// This class was designed by Marta the cat during one
/// of her spontaneous keyboard walks. The implementation
/// reflects authentic feline behavior patterns and is
/// included with great respect to Marta and her creative
/// contribution.
/// </remarks>
public sealed class MeowProcessLauncher : IProcessLauncher
{
    /// <summary>
    /// Contains a predefined set of basic cat sound
    /// representations emitted during the simulated
    /// cat process execution.
    /// </summary>
    /// <remarks>
    /// Each entry represents a simple, frequently occurring
    /// feline vocalization or action that can be reported as
    /// standard output.
    ///
    /// These sounds are intended to model common background
    /// cat behavior and are selected randomly to provide
    /// a sense of liveliness and unpredictability during
    /// execution.
    ///
    /// More expressive or disruptive behaviors are handled
    /// separately by dedicated cat action routines.
    /// </remarks>
    private static readonly string[] Sounds =
    {
        "meow",
        "mrrp",
        "purr",
        "purrr",
        "scratch",
        "hiss",
        "*knocks something off the table*"
    };

    /// <summary>
    /// Defines a catalog of cat-specific behavior actions
    /// that can be randomly emitted during the simulated
    /// cat process execution.
    /// </summary>
    /// <remarks>
    /// Each entry represents a single, self-contained
    /// behavior expressed as an action applied to an
    /// <see cref="IProcessOutputBasicObserver"/>.
    ///
    /// Actions may report events either as standard output
    /// or standard error to reflect the unpredictable nature
    /// of feline behavior.
    ///
    /// The index of an action in this array directly
    /// corresponds to its selection probability when chosen
    /// via a random index. Adding or removing actions
    /// automatically adjusts the behavior distribution.
    ///
    /// This structure replaces conditional branching with
    /// data-driven behavior selection for improved clarity,
    /// extensibility, and feline approval.
    /// </remarks>
    private static readonly Action<IProcessOutputBasicObserver>[] CatActions =
    {
        o => o.OnErrorLine("😾 Cat is displeased for no reason."),
        o => o.OnOutputLine("🐈 Cat stares intensely at nothing."),
        o => o.OnOutputLine("🧶 Cat sits directly on the keyboard."),
        o => o.OnOutputLine("💤 Cat falls asleep mid-execution."),
        o => o.OnOutputLine("💖 Cat purrs, headbutts gently, and is full of love."),
        o => o.OnOutputLine("💦 Cat meticulously licks its fur in front of the monitor."),
        o => o.OnOutputLine("🐾 Cat desperately asks to be picked up right now."),
        o => o.OnOutputLine("🐦 Cat sees a bird outside and forgets everything."),
        o => o.OnErrorLine("🔔 Cat pretends not to hear you calling.")
    };

    private static readonly Random Random = new();

    /// <inheritdoc />
    public async Task<ProcessResult> RunAsync(
        ProcessRequest request,
        ProcessOutputHandling? output = null,
        CancellationToken ct = default)
    {
        var observers = output?.Observers
            ?.OfType<IProcessOutputBasicObserver>()
            .ToArray()
            ?? Array.Empty<IProcessOutputBasicObserver>();


        NotifyOutputLine(observers, "🐈 Cat process started.");
        NotifyOutputLine(observers, "🐾 Ignoring ProcessRequest. Doing cat things instead.");

        try
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var sound = PickRandomSound();
                NotifyOutputLine(observers, sound);

                await Task.Delay(GetRandomPause(), ct);

                // Occasionally do something VERY cat-like
                MaybeDoCatSpecificAction(observers);
            }
        }
        catch (OperationCanceledException)
        {
            NotifyErrorLine(observers, "🐾 Cat noticed cancellation.");
            NotifyErrorLine(observers, "😼 Leaving with dignity.");
        }

        return new ProcessResult
        {
            ExitCode = 0,
            StandardOutput = string.Empty,
            StandardError = string.Empty
        };
    }

    /// <summary>
    /// Dispatches a standard output line to all configured
    /// process output observers.
    /// </summary>
    /// <param name="observers">
    /// A collection of process observers that may include
    /// implementations capable of handling standard output.
    /// </param>
    /// <param name="line">
    /// The output line to be dispatched.
    /// </param>
    /// <remarks>
    /// Only observers that implement
    /// <see cref="IProcessOutputBasicObserver"/> receive the
    /// output notification. Other observer types are ignored.
    ///
    /// This method performs no synchronization and may be
    /// invoked concurrently from multiple threads. Observer
    /// implementations are responsible for ensuring their
    /// own thread safety.
    ///
    /// The method is intentionally tolerant of <c>null</c>
    /// or empty observer collections and performs no action
    /// in such cases.
    /// </remarks>
    private static void NotifyOutputLine(
        IReadOnlyList<IProcessOutputObserver>? observers,
        string line)
    {
        if (observers is null)
            return;

        foreach (var observer in observers)
        {
            if (observer is IProcessOutputBasicObserver outputObserver)
            {
                outputObserver.OnOutputLine(line);
            }
        }
    }

    /// <summary>
    /// Dispatches a standard error output line to all configured
    /// process output observers.
    /// </summary>
    /// <param name="observers">
    /// A collection of process observers that may include
    /// implementations capable of handling standard error output.
    /// </param>
    /// <param name="line">
    /// The error output line to be dispatched.
    /// </param>
    /// <remarks>
    /// Only observers that implement
    /// <see cref="IProcessOutputBasicObserver"/> receive the
    /// error output notification. Other observer types are ignored.
    ///
    /// This method performs no synchronization and may be
    /// invoked concurrently from multiple threads. Observer
    /// implementations are responsible for ensuring their
    /// own thread safety.
    ///
    /// The method is intentionally tolerant of <c>null</c>
    /// or empty observer collections and performs no action
    /// in such cases.
    /// </remarks>
    private static void NotifyErrorLine(
        IReadOnlyList<IProcessOutputObserver>? observers,
        string line)
    {
        if (observers is null)
            return;

        foreach (var observer in observers)
        {
            if (observer is IProcessOutputBasicObserver outputObserver)
            {
                outputObserver.OnErrorLine(line);
            }
        }
    }

    /// <summary>
    /// Selects a random cat sound to be emitted
    /// as standard output.
    /// </summary>
    /// <remarks>
    /// The returned value represents a single
    /// cat-related sound or action description.
    /// Selection is intentionally non-deterministic
    /// to simulate unpredictable feline behavior.
    /// </remarks>
    private static string PickRandomSound()
        => Sounds[Random.Next(Sounds.Length)];

    /// <summary>
    /// Generates a random delay between
    /// consecutive cat actions.
    /// </summary>
    /// <remarks>
    /// The returned time span represents a pause
    /// between emitted outputs and is used to
    /// simulate irregular and spontaneous
    /// cat activity patterns.
    /// </remarks>
    private static TimeSpan GetRandomPause()
        => TimeSpan.FromMilliseconds(Random.Next(300, 2000));

    /// <summary>
    /// Occasionally (with a low random probability)
    /// emits additional cat-specific behavior
    /// events to the configured process observers.
    /// </summary>
    /// <param name="observers">
    /// A collection of process observers that may receive
    /// cat behavior notifications.
    /// </param>
    /// <remarks>
    /// This method introduces low-probability, behavior-rich
    /// events that go beyond simple sound output, such as
    /// affection, grooming, attention-seeking, or sudden
    /// disengagement.
    ///
    /// Some events are intentionally reported as standard
    /// error output to reflect unpredictable or disruptive
    /// feline states.
    ///
    /// Only observers that implement
    /// <see cref="IProcessOutputBasicObserver"/> receive notifications.
    /// The method is tolerant of <c>null</c> or empty observer
    /// collections and performs no action in such cases.
    /// </remarks>
    private static void MaybeDoCatSpecificAction(
        IReadOnlyList<IProcessOutputObserver>? observers)
    {
        if (observers is null)
            return;

        var roll = Random.Shared.Next(CatActions.Length + 5);

        var action = CatActions[roll];

        foreach (var observer in observers)
        {
            if (observer is IProcessOutputBasicObserver outputObserver)
            {
                action(outputObserver);
            }
        }
    }
}
