using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Helpers;

/// <summary>
/// Provides internal helper methods for working with
/// <see cref="ProcessResult"/> instances.
/// </summary>
internal static class ProcessResultHelpers
{
    /// <summary>
    /// Creates a <see cref="ProcessResult"/> instance
    /// from the collected execution data.
    /// </summary>
    /// <param name="request">
    /// The <see cref="ProcessRequest"/> that produced the result.
    /// </param>
    /// <param name="exitCode">
    /// The exit code returned by the completed process.
    /// </param>
    /// <param name="stdoutLines">
    /// A collection of lines captured from the process
    /// standard output stream, or <c>null</c> if output
    /// capturing was disabled.
    /// </param>
    /// <param name="stderrLines">
    /// A collection of lines captured from the process
    /// standard error stream, or <c>null</c> if output
    /// capturing was disabled.
    /// </param>
    /// <returns>
    /// A <see cref="ProcessResult"/> containing the exit code
    /// and aggregated standard output and error streams.
    /// </returns>
    public static ProcessResult CreateResult(
        ProcessRequest request,
        int exitCode,
        IReadOnlyCollection<string>? stdoutLines,
        IReadOnlyCollection<string>? stderrLines)
    {
        return new ProcessResult
        {
            RequestId = request.Id,
            ExitCode = exitCode,
            StandardOutput = stdoutLines is not null
                ? string.Join(Environment.NewLine, stdoutLines)
                : string.Empty,

            StandardError = stderrLines is not null
                ? string.Join(Environment.NewLine, stderrLines)
                : string.Empty
        };
    }
}
