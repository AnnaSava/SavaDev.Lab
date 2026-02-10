using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Launchers.DotNet;

/// <summary>
/// Default implementation of <see cref="IDotNetProcessLauncher"/>
/// that executes <c>dotnet</c> commands using an underlying
/// <see cref="IProcessLauncher"/>.
/// </summary>
/// <remarks>
/// This class acts as a thin convenience wrapper that fixes
/// the executable name to <c>dotnet</c> and forwards all execution
/// details to a generic process launcher.
/// 
/// It does not perform validation or interpretation of command
/// arguments and assumes they are supplied correctly by the caller.
/// </remarks>
public sealed class DotNetProcessLauncher : IDotNetProcessLauncher
{
    /// <summary>
    /// The executable name for the .NET CLI.
    /// </summary>
    private const string CommandName = "dotnet";

    private readonly IProcessLauncher _processLauncher;

    /// <summary>
    /// Initializes a new instance of the <see cref="DotNetProcessLauncher"/>
    /// class.
    /// </summary>
    /// <param name="processLauncher">
    /// The underlying <see cref="IProcessLauncher"/> used to execute
    /// <c>dotnet</c> commands.
    /// </param>
    public DotNetProcessLauncher(IProcessLauncher processLauncher)
    {
        _processLauncher = processLauncher
            ?? throw new ArgumentNullException(nameof(processLauncher));
    }

    /// <inheritdoc />
    public Task<ProcessResult> RunAsync(
        string arguments,
        string? workingDirectory = null,
        ProcessOutputHandling? output = null,
        CancellationToken ct = default)
    {
        return _processLauncher.RunAsync(
            new ProcessRequest
            {
                FileName = CommandName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory
            },
            output,
            ct);
    }
}
