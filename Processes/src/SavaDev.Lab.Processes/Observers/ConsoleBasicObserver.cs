namespace SavaDev.Lab.Processes.Observers;

/// <summary>
/// A simple <see cref="IProcessOutputBasicObserver"/> implementation
/// that writes process output to the console.
/// </summary>
/// <remarks>
/// This observer forwards standard output and standard error
/// lines to <see cref="Console.Out"/> using simple textual prefixes.
/// 
/// It is intended for debugging, development, and command-line
/// scenarios where real-time visibility of process output
/// is required.
/// </remarks>
public sealed class ConsoleBasicObserver : IProcessOutputBasicObserver
{
    /// <summary>
    /// Writes a line produced by the process standard output
    /// stream to the console.
    /// </summary>
    /// <param name="line">
    /// A single line read from the process standard output stream.
    /// </param>
    /// <remarks>
    /// The line is written to <see cref="Console.Out"/> and
    /// prefixed with a short marker to distinguish standard
    /// output from standard error.
    /// </remarks>
    public void OnOutputLine(string line)
        => Console.WriteLine($"[OUT] {line}");

    /// <summary>
    /// Writes a line produced by the process standard error
    /// stream to the console.
    /// </summary>
    /// <param name="line">
    /// A single line read from the process standard error stream.
    /// </param>
    /// <remarks>
    /// The line is written to <see cref="Console.Error"/> and
    /// prefixed with a short marker to distinguish error output
    /// from standard output.
    /// </remarks>
    public void OnErrorLine(string line)
        => Console.Error.WriteLine($"[ERR] {line}");
}
