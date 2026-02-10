using System.Diagnostics;

namespace SavaDev.Lab.Processes.Models;

/// <summary>
/// Describes parameters required to start an external process.
/// </summary>
/// <remarks>
/// This class is a simple data container used by
/// <see cref="IProcessLauncher"/> implementations to configure
/// process execution. It does not perform validation itself.
/// Basic validation is performed by the launcher.
/// </remarks>
public class ProcessRequest
{
    /// <summary>
    /// Gets the unique identifier of this process request.
    /// </summary>
    /// <remarks>
    /// This identifier can be used to correlate process
    /// execution, output, and diagnostic information,
    /// especially when multiple processes are executed
    /// concurrently.
    /// </remarks>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the name or path of the executable to run.
    /// </summary>
    /// <remarks>
    /// This value is assigned to <see cref="ProcessStartInfo.FileName"/>.
    /// It can be either an executable name available in PATH
    /// or a full path to the executable file.
    /// </remarks>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the command-line arguments to pass to the executable.
    /// </summary>
    /// <remarks>
    /// This value is assigned to <see cref="ProcessStartInfo.Arguments"/>.
    /// The string is passed as-is and is not escaped or validated.
    /// </remarks>
    public string Arguments { get; init; } = string.Empty;

    /// <summary>
    /// Gets the working directory for the process.
    /// </summary>
    /// <remarks>
    /// When specified, this value is assigned to
    /// <see cref="ProcessStartInfo.WorkingDirectory"/>.
    /// If <c>null</c>, the default working directory is used.
    /// </remarks>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    /// Gets a collection of environment variables to be applied
    /// to the process.
    /// </summary>
    /// <remarks>
    /// Each key-value pair is added to the process environment
    /// before the process is started. When <c>null</c>,
    /// no additional environment variables are applied.
    /// </remarks>
    public IReadOnlyDictionary<string, string>? EnvironmentVariables { get; init; }
}
