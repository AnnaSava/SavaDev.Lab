using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.ProcessOutputObserverResolverTests.Setup;

/// <summary>
/// Represents an observer that does not implement
/// any output handling interfaces.
/// </summary>
internal sealed class UnsupportedObserver : IProcessOutputObserver
{
    // Intentionally empty.
    // Represents an observer that does not handle output.
}

