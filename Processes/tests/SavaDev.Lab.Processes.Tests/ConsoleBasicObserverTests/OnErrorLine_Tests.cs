using AwesomeAssertions;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.ConsoleBasicObserverTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="ConsoleBasicObserver"/> when handling
/// standard error output lines.
/// </summary>
/// <remarks>
/// These tests ensure that the <see cref="ConsoleBasicObserver"/>
/// correctly writes error output lines to the console error
/// stream, using the expected formatting and prefixes.
///
/// The test suite also includes basic smoke tests that
/// validate the observer's ability to handle concurrent
/// invocations of <c>OnErrorLine</c> without throwing
/// exceptions, which is important when error output is
/// produced in parallel.
/// </remarks>
[Collection("Console tests")]
/// <summary>
/// Tests for OnErrorLine_Tests.
/// </summary>
public class OnErrorLine_Tests
{
    /// <summary>
    /// Verifies that a standard error line is written
    /// to the error console stream with the expected prefix.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The line is written to <see cref="Console.Error"/>.</item>
    /// <item>The output is prefixed with <c>[ERR]</c>.</item>
    /// <item>The original line content is preserved.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnErrorLine_ShouldWritePrefixedLineToConsoleError.
/// </summary>
    public void OnErrorLine_ShouldWritePrefixedLineToConsoleError()
    {
        // Arrange
        var observer = new ConsoleBasicObserver();
        var originalError = Console.Error;

        using var writer = new StringWriter();
        Console.SetError(TextWriter.Synchronized(writer));

        try
        {
            // Act
            observer.OnErrorLine("error");

            // Assert
            var output = writer.ToString();

            output.Should().Contain("[ERR] error");
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    /// Verifies that writing an empty error line
    /// still produces a prefixed entry in the error stream.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>An error line is written even if the content is empty.</item>
    /// <item>The prefix <c>[ERR]</c> is still present.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnErrorLine_WithEmptyLine_ShouldStillWritePrefix.
/// </summary>
    public void OnErrorLine_WithEmptyLine_ShouldStillWritePrefix()
    {
        // Arrange
        var observer = new ConsoleBasicObserver();
        var originalError = Console.Error;

        using var buffer = new StringWriter();
        Console.SetError(TextWriter.Synchronized(buffer));

        try
        {
            // Act
            observer.OnErrorLine(string.Empty);

            // Assert
            var output = buffer.ToString();

            output.Should().Contain("[ERR]");
        }
        finally
        {
            Console.SetError(originalError);
        }
    }
}

