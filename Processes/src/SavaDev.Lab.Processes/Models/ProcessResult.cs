namespace SavaDev.Lab.Processes.Models;

/// <summary>
/// Represents the result of an external process execution.
/// </summary>
/// <remarks>
/// This class contains the exit code of the completed process
/// and optionally the captured standard output and standard
/// error streams.
///
/// Output capturing behavior is controlled by the
/// <see cref="ProcessOutputHandling.CaptureOutput"/> option
/// provided at execution time.
/// When output capturing is disabled, the corresponding
/// output properties are returned as empty strings.
///
/// The result does not interpret, normalize, or post-process
/// the process output and leaves result evaluation entirely
/// to the caller.
/// </remarks>
public class ProcessResult
{
    /// <summary>
    /// Gets the identifier of the process request
    /// that produced this result.
    /// </summary>
    /// <remarks>
    /// This value corresponds to <see cref="ProcessRequest.Id"/>
    /// and can be used to correlate execution results
    /// with observed output, diagnostics, or telemetry.
    /// </remarks>
    public Guid RequestId { get; init; }

    /// <summary>
    /// Gets the exit code returned by the process.
    /// </summary>
    /// <remarks>
    /// By convention, an exit code of <c>0</c> usually indicates
    /// successful execution. The exact meaning of the value
    /// is defined by the executed process and is not interpreted
    /// by the launcher.
    /// </remarks>
    public int ExitCode { get; init; }

    /// <summary>
    /// Gets the captured standard output of the process.
    /// </summary>
    /// <remarks>
    /// This value contains the full contents of the standard output
    /// stream as produced by the process during execution, if output
    /// capturing was enabled.
    ///
    /// When output capturing is disabled via
    /// <see cref="ProcessOutputHandling.CaptureOutput"/>, this property
    /// is returned as an empty string, even if the process
    /// produced standard output.
    /// </remarks>
    public string StandardOutput { get; init; } = string.Empty;

    /// <summary>
    /// Gets the captured standard error output of the process.
    /// </summary>
    /// <remarks>
    /// This value contains the full contents of the standard error
    /// stream as produced by the process during execution, if output
    /// capturing was enabled.
    ///
    /// When output capturing is disabled via
    /// <see cref="ProcessOutputHandling.CaptureOutput"/>, this property
    /// is returned as an empty string, even if the process
    /// produced error output.
    /// </remarks>
    public string StandardError { get; init; } = string.Empty;

}
