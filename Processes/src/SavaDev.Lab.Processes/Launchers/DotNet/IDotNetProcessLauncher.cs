using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Launchers.DotNet;

/// <summary>
/// Provides an abstraction for launching the <c>dotnet</c> CLI
/// with the specified command-line arguments.
/// </summary>
/// <remarks>
/// This interface represents a convenience wrapper over a generic
/// process launcher, focused specifically on executing
/// <c>dotnet</c> commands.
/// 
/// It does not interpret command results and delegates all
/// execution details to the underlying process launcher.
/// </remarks>
public interface IDotNetProcessLauncher
{
    /// <summary>
    /// Runs a <c>dotnet</c> command using the specified arguments.
    /// </summary>
    /// <param name="arguments">
    /// Command-line arguments passed to the <c>dotnet</c> executable.
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
    /// A cancellation token that can be used to cancel the command
    /// execution and associated I/O operations.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a <see cref="ProcessResult"/> with
    /// the exit code and captured output of the executed command.
    /// </returns>
    Task<ProcessResult> RunAsync(
        string arguments,
        string? workingDirectory = null,
        ProcessOutputHandling? output = null,
        CancellationToken ct = default);
}
