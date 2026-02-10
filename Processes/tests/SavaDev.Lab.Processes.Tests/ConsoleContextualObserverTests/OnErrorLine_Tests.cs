using AwesomeAssertions;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.ConsoleContextualObserverTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="ConsoleContextualObserver"/> when handling
/// standard error output lines together with execution
/// context information.
/// </summary>
/// <remarks>
/// These tests ensure that the <see cref="ConsoleContextualObserver"/>
/// correctly writes standard error output lines to the
/// <see cref="Console.Error"/> stream and includes the
/// identifier of the originating <see cref="ProcessRequest"/>
/// in each output entry.
///
/// The test suite validates both functional behavior
/// (such as correct routing to the error stream and
/// identifier propagation) and basic concurrency
/// characteristics, ensuring that <c>OnErrorLine</c>
/// can be invoked concurrently when execution context
/// information is provided.
/// </remarks>
[Collection("Console tests")]
/// <summary>
/// Tests for OnErrorLine_Tests.
/// </summary>
public class OnErrorLine_Tests
{
    /// <summary>
    /// Verifies that a standard error line is written
    /// to the error console stream and includes the
    /// request identifier.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The line is written to <see cref="Console.Error"/>.</item>
    /// <item>The output contains the request identifier.</item>
    /// <item>The original line content is preserved.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnErrorLine_ShouldWriteLineWithRequestId.
/// </summary>
    public void OnErrorLine_ShouldWriteLineWithRequestId()
    {
        // Arrange
        var observer = new ConsoleContextualObserver();
        var request = new ProcessRequest();
        var originalError = Console.Error;

        using var buffer = new StringWriter();
        Console.SetError(TextWriter.Synchronized(buffer));

        try
        {
            // Act
            observer.OnErrorLine(request, "error");

            // Assert
            var output = buffer.ToString();

            output.Should().Contain("[ERR]");
            output.Should().Contain(request.Id.ToString());
            output.Should().Contain("error");
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    /// Verifies that the observer can handle multiple
    /// concurrent error notifications with execution
    /// context without throwing exceptions.
    /// </summary>
    /// <remarks>
    /// This is a smoke test that validates basic thread-safety:
    /// <list type="bullet">
    /// <item>Method can be called concurrently.</item>
    /// <item>No exceptions are thrown.</item>
    /// <item>Each call produces error output.</item>
    /// </list>
    ///
    /// Output ordering is not asserted.
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnErrorLine_ShouldHandleConcurrentCallsWithContext.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task OnErrorLine_ShouldHandleConcurrentCallsWithContext()
    {
        // Arrange
        var observer = new ConsoleContextualObserver();
        var originalError = Console.Error;

        using var buffer = new StringWriter();
        var synchronizedWriter = TextWriter.Synchronized(buffer);
        Console.SetError(synchronizedWriter);

        const int callCount = 100;

        try
        {
            // Act
            var tasks = Enumerable.Range(0, callCount)
                .Select(_ =>
                {
                    var request = new ProcessRequest();
                    return Task.Run(() =>
                        observer.OnErrorLine(request, "parallel error"));
                })
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
            Console.SetError(originalError);
        }
    }
}

