using SavaDev.DemoKit.ConsoleEngine;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;

namespace SavaDev.Lab.Processes.Demo.Scenarios;

/// <summary>
/// Demonstrates passing environment variables
/// to an external process.
/// </summary>
/// <remarks>
/// This scenario launches a simple shell command
/// that reads and prints an environment variable
/// provided via <see cref="ProcessRequest.EnvironmentVariables"/>.
///
/// The scenario verifies that:
/// <list type="bullet">
/// <item>Environment variables are available inside the launched process.</item>
/// <item>Variables are scoped to the process and do not affect the host.</item>
/// <item>The behavior is consistent across operating systems.</item>
/// </list>
/// </remarks>
public sealed class EnvironmentVariablesScenario : IConsoleDemoScenario
{
    /// <inheritdoc />
    public string Name => "Environment variables";

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken ct)
    {
        var processLauncher = new ProcessLauncher(new ProcessOutputObserverResolver());
        var observer = new ConsoleContextualObserver();
        var output = new ProcessOutputHandling(observer);

        const string variableName = "SAVADEV_DEMO_VAR";
        const string variableValue = "Hello from environment!";

        var request = CreateRequest(variableName, variableValue);

        Console.WriteLine("Starting process with environment variables...");
        Console.WriteLine($"{variableName} = \"{variableValue}\"");
        Console.WriteLine();

        var result = await processLauncher.RunAsync(
            request,
            output,
            ct);

        Console.WriteLine();
        Console.WriteLine("=== Process completed ===");
        Console.WriteLine($"Exit code: {result.ExitCode}");
    }

    /// <summary>
    /// Creates a <see cref="ProcessRequest"/> that demonstrates
    /// passing environment variables to an external process.
    /// </summary>
    /// <param name="variableName">
    /// The name of the environment variable to be provided
    /// to the launched process.
    /// </param>
    /// <param name="variableValue">
    /// The value of the environment variable to be provided
    /// to the launched process.
    /// </param>
    /// <remarks>
    /// The request is constructed differently depending on
    /// the operating system in order to use a native shell:
    /// <list type="bullet">
    /// <item>
    /// On Windows, <c>cmd</c> is used and the variable is
    /// accessed using the <c>%VAR_NAME%</c> syntax.
    /// </item>
    /// <item>
    /// On Unix-like systems, <c>/bin/sh</c> is used and the
    /// variable is accessed using the <c>$VAR_NAME</c> syntax.
    /// </item>
    /// </list>
    ///
    /// The environment variable is supplied via
    /// <see cref="ProcessRequest.EnvironmentVariables"/>,
    /// which ensures that the variable is available only
    /// to the launched process and does not affect the
    /// environment of the demo host application.
    ///
    /// This scenario demonstrates that environment variables
    /// are correctly applied and isolated per process,
    /// regardless of the underlying operating system.
    /// </remarks>
    /// <returns>
    /// A <see cref="ProcessRequest"/> configured to launch
    /// a shell command that prints the provided environment
    /// variable value.
    /// </returns>
    private static ProcessRequest CreateRequest(
        string variableName,
        string variableValue)
    {
        if (OperatingSystem.IsWindows())
        {
            return new ProcessRequest
            {
                FileName = "cmd",
                Arguments = $"/c echo %{variableName}%",
                EnvironmentVariables = new Dictionary<string, string>
                {
                    [variableName] = variableValue
                }
            };
        }

        return new ProcessRequest
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"echo ${variableName}\"",
            EnvironmentVariables = new Dictionary<string, string>
            {
                [variableName] = variableValue
            }
        };
    }
}
