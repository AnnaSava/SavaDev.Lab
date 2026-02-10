using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Launchers.Shell;

/// <summary>
/// Provides an abstraction for executing shell commands
/// in the default shell of the current operating system.
/// </summary>
/// <remarks>
/// The launcher selects an appropriate shell based on the
/// underlying platform and executes the provided command
/// as a single shell invocation.
/// 
/// It is intended for simple command execution scenarios
/// and does not interpret or validate shell-specific syntax.
/// </remarks>
public interface IShellProcessLauncher
{
    /// <summary>
    /// Executes the specified command using the default
    /// shell of the current operating system.
    /// </summary>
    /// <param name="command">
    /// A shell command to be executed.
    /// </param>
    /// <param name="workingDirectory">
    /// An optional working directory used when executing the command.
    /// </param>
    /// <param name="output">
    /// Optional output handling options that control whether
    /// process output is captured in memory and how output
    /// lines are observed during execution.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that can be used to cancel the
    /// command execution and associated I/O operations.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="ProcessResult"/>
    /// describing the execution outcome.
    /// </returns>
    Task<ProcessResult> RunAsync(
        string command,
        string? workingDirectory = null,
        ProcessOutputHandling? output = null,
        CancellationToken ct = default);
}

