using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Launchers.Shell;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates executing a simple shell command using
/// <see cref="IShellProcessLauncher"/>.
/// </summary>
/// <remarks>
/// This scenario shows how a shell process launcher can be
/// used to execute a command via the default shell of the
/// current operating system.
///
/// The scenario runs a minimal, safe shell command that
/// produces standard output and exits successfully. This
/// makes it suitable for demonstration purposes without
/// introducing side effects.
///
/// The scenario highlights that:
/// <list type="bullet">
/// <item>The command is executed as a single shell invocation.</item>
/// <item>Shell-specific syntax is passed through verbatim.</item>
/// <item>Output handling behaves the same as for regular process execution.</item>
/// </list>
/// </remarks>
public sealed class ShellCommandScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Shell command";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var baseLauncher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var shellLauncher = new ShellProcessLauncher(baseLauncher);

        var observer = new ConsoleContextualObserver();
        var output = new ProcessOutputHandling(observer);

        var command = CreateCommand();

        Console.WriteLine("Executing shell command...");
        Console.WriteLine($"Command: {command}");
        Console.WriteLine();

        var result = await shellLauncher.RunAsync(
            command,
            output: output,
            ct: ct);

        Console.WriteLine();
        Console.WriteLine("=== Shell command completed ===");
        Console.WriteLine($"Exit code: {result.ExitCode}");
    }

    /// <summary>
    /// Creates a simple, safe shell command suitable
    /// for cross-platform demonstration.
    /// </summary>
    /// <remarks>
    /// The command prints a short message to the standard
    /// output stream using shell syntax. It does not read
    /// from or write to the file system and has no side effects.
    ///
    /// The exact command string is shell-agnostic and is
    /// expected to work in the default shells provided
    /// by common operating systems.
    /// </remarks>
    /// <returns>
    /// A shell command string that produces standard output.
    /// </returns>
    private static string CreateCommand() => "echo Hello from the shell launcher";
}
