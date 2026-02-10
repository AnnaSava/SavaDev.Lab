namespace SavaDev.Lab.Processes.Tests.Setup;

/// <summary>
/// Defines a test collection for tests that interact with
/// the global <see cref="Console"/> state.
/// </summary>
/// <remarks>
/// Tests that modify <see cref="Console.Out"/> or
/// <see cref="Console.Error"/> affect global process-wide
/// state and cannot be safely executed in parallel.
///
/// This collection disables parallel execution for all
/// contained tests to ensure deterministic behavior and
/// to prevent interference between tests that redirect
/// console output.
///
/// Only tests that directly interact with the console
/// should be placed in this collection.
/// </remarks>
[CollectionDefinition("Console tests", DisableParallelization = true)]
/// <summary>
/// Tests for ConsoleTestsCollection.
/// </summary>
public class ConsoleTestsCollection
{
}

