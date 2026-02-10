namespace SavaDev.Lab.Processes.Observers;

/// <summary>
/// Represents a marker interface for process-related
/// observer components.
/// </summary>
/// <remarks>
/// This interface does not define any members and is used
/// to group different kinds of process observers under a
/// common type.
///
/// Implementations may observe various aspects of process
/// execution, such as standard output, standard error,
/// execution context, lifecycle events, or metrics, by
/// implementing more specific observer interfaces.
///
/// The marker interface allows observer instances to be
/// collected, stored, and passed through infrastructure
/// components in a type-safe manner without coupling those
/// components to specific observer contracts.
/// </remarks>
public interface IProcessOutputObserver
{
}
