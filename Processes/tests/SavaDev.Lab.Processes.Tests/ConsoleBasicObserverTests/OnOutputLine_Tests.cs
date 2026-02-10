using AwesomeAssertions;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.ConsoleBasicObserverTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="ConsoleBasicObserver"/> when handling
/// standard output lines.
/// </summary>
/// <remarks>
/// These tests ensure that the <see cref="ConsoleBasicObserver"/>
/// correctly writes standard output lines to the console,
/// using the expected formatting and prefixes.
///
/// The test suite also includes basic smoke tests that
/// validate the observer's ability to handle concurrent
/// invocations of <c>OnOutputLine</c> without throwing
/// exceptions, which is important when process output
/// is produced in parallel.
/// </remarks>
[Collection("Console tests")]
/// <summary>
/// Tests for OnOutputLine_Tests.
/// </summary>
public sealed class OnOutputLine_Tests
{
    /// <summary>
    /// Verifies that a standard output line is written
    /// to the console with the expected prefix.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The line is written to <see cref="Console.Out"/>.</item>
    /// <item>The output is prefixed with <c>[OUT]</c>.</item>
    /// <item>The original line content is preserved.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnOutputLine_ShouldWritePrefixedLineToConsole.
/// </summary>
    public void OnOutputLine_ShouldWritePrefixedLineToConsole()
    {
        // Arrange
        var observer = new ConsoleBasicObserver();
        var originalOut = Console.Out;

        using var writer = new StringWriter();
        Console.SetOut(TextWriter.Synchronized(writer));

        try
        {
            // Act
            observer.OnOutputLine("hello");

            // Assert
            var output = writer.ToString();

            output.Should().Contain("[OUT] hello");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Verifies that writing an empty output line
    /// still produces a prefixed console entry.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>An output line is written even if the content is empty.</item>
    /// <item>The prefix <c>[OUT]</c> is still present.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnOutputLine_WithEmptyLine_ShouldStillWritePrefix.
/// </summary>
    public void OnOutputLine_WithEmptyLine_ShouldStillWritePrefix()
    {
        // Arrange
        var observer = new ConsoleBasicObserver();
        var originalOut = Console.Out;

        using var writer = new StringWriter();
        Console.SetOut(TextWriter.Synchronized(writer));

        try
        {
            // Act
            observer.OnOutputLine(string.Empty);

            // Assert
            var output = writer.ToString();

            output.Should().Contain("[OUT]");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Verifies that the observer can handle multiple
    /// concurrent output notifications without throwing
    /// exceptions.
    /// </summary>
    /// <remarks>
    /// This is a smoke test that validates basic thread-safety
    /// assumptions:
    /// <list type="bullet">
    /// <item>Methods can be called concurrently.</item>
    /// <item>No exceptions are thrown.</item>
    /// <item>All calls result in some console output.</item>
    /// </list>
    ///
    /// The test does not assert output ordering, as concurrent
    /// execution does not guarantee deterministic ordering.
    /// </remarks>
    [Fact]
/// <summary>
/// Tests Observer_ShouldHandleConcurrentOutputCalls.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task Observer_ShouldHandleConcurrentOutputCalls()
    {
        // Arrange
        var observer = new ConsoleBasicObserver();
        var originalOut = Console.Out;

        using var buffer = new StringWriter();
        var synchronizedWriter = TextWriter.Synchronized(buffer);

        Console.SetOut(synchronizedWriter);

        const int callCount = 100;

        try
        {
            // Act
            var tasks = Enumerable.Range(0, callCount)
                .Select(i => Task.Run(() => observer.OnOutputLine($"line {i}")))
                .ToArray();

            await Task.WhenAll(tasks);

            // Assert
            var output = buffer.ToString();

            output.Should().NotBeNullOrEmpty();

            var lineCount = output
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Length;

            lineCount.Should().BeGreaterThan(callCount - 1);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}

