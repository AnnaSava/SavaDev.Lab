using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Extensions;

/// <summary>
/// Extension methods for <see cref="ProcessResult"/>.
/// </summary>
public static class ProcessResultExtensions
{
    /// <summary>
    /// Returns <c>true</c> when the process exit code is <c>0</c>.
    /// </summary>
    /// <param name="result">The process result.</param>
    /// <returns><c>true</c> if the exit code equals <c>0</c>; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is <c>null</c>.</exception>
    public static bool IsSuccess(this ProcessResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.ExitCode == 0;
    }
}
