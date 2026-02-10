using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Launchers.Shell;

/// <summary>
/// Default implementation of <see cref="IShellProcessLauncher"/>
/// that executes commands using the default shell of the
/// current operating system.
/// </summary>
/// <remarks>
/// On Windows, commands are executed using <c>cmd.exe</c>.
/// On Unix-like systems, commands are executed using <c>/bin/sh</c>.
/// 
/// This class acts as a thin facade over <see cref="IProcessLauncher"/>
/// and is intended for simple, text-based shell command execution.
/// </remarks>
public sealed class ShellProcessLauncher : IShellProcessLauncher
{
    private readonly IProcessLauncher _processLauncher;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellProcessLauncher"/>
    /// class.
    /// </summary>
    /// <param name="processLauncher">
    /// The underlying <see cref="IProcessLauncher"/> used to
    /// execute shell commands.
    /// </param>
    public ShellProcessLauncher(IProcessLauncher processLauncher)
    {
        _processLauncher = processLauncher
            ?? throw new ArgumentNullException(nameof(processLauncher));
    }

    /// <inheritdoc />
    public Task<ProcessResult> RunAsync(
        string command,
        string? workingDirectory = null,
        ProcessOutputHandling? output = null,
        CancellationToken ct = default)
    {
        var (fileName, arguments) = BuildShellCommand(command);

        return _processLauncher.RunAsync(
            new ProcessRequest
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory
            },
            output,
            ct);
    }

    /// <summary>
    /// Builds a shell-specific command invocation
    /// for the current operating system.
    /// </summary>
    /// <param name="command">
    /// The raw shell command to be executed.
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// The shell executable file name.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The arguments required to execute the command
    /// via the selected shell.
    /// </description>
    /// </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// On Windows, the command is executed via
    /// <c>cmd.exe</c> using the <c>/c</c> switch,
    /// which runs the specified command and then exits.
    ///
    /// On Unix-like systems, the command is executed
    /// via <c>/bin/sh</c> using the <c>-c</c> option,
    /// which evaluates the provided command string.
    ///
    /// This method does not perform validation or
    /// escaping of the provided command and assumes
    /// that it is safe and correctly formatted
    /// for the target shell.
    /// </remarks>
    private static (string fileName, string arguments) BuildShellCommand(string command)
    {
        if (OperatingSystem.IsWindows())
        {
            return ("cmd.exe", $"/c \"{command}\"");
        }

        return ("/bin/sh", $"-c \"{command}\"");
    }
}
