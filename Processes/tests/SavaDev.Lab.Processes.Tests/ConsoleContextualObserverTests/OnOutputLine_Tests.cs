using AwesomeAssertions;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.ConsoleContextualObserverTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="ConsoleContextualObserver"/> when handling
/// standard output lines together with execution context.
/// </summary>
/// <remarks>
/// These tests ensure that the <see cref="ConsoleContextualObserver"/>
/// correctly writes standard output lines to the console and
/// includes the identifier of the originating
/// <see cref="ProcessRequest"/> in each output entry.
///
/// The test suite validates both functional behavior
/// (such as correct routing and identifier propagation)
/// and basic concurrency characteristics, ensuring that
/// <c>OnOutputLine</c> can be invoked concurrently when
/// execution context information is provided.
/// </remarks>
[Collection("Console tests")]
/// <summary>
/// Tests for OnOutputLine_Tests.
/// </summary>
public sealed class OnOutputLine_Tests
{
    /// <summary>
    /// Verifies that a standard output line is written
    /// to the console and includes the request identifier.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The line is written to <see cref="Console.Out"/>.</item>
    /// <item>The output contains the request identifier.</item>
    /// <item>The original line content is preserved.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnOutputLine_ShouldWriteLineWithRequestId.
/// </summary>
    public void OnOutputLine_ShouldWriteLineWithRequestId()
    {
        // Arrange
        var observer = new ConsoleContextualObserver();
        var request = new ProcessRequest();
        var originalOut = Console.Out;

        using var buffer = new StringWriter();
        Console.SetOut(TextWriter.Synchronized(buffer));

        try
        {
            // Act
            observer.OnOutputLine(request, "hello");

            // Assert
            var output = buffer.ToString();

            output.Should().Contain("[OUT]");
            output.Should().Contain(request.Id.ToString());
            output.Should().Contain("hello");
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Verifies that the observer can handle multiple
    /// concurrent output notifications with execution
    /// context without throwing exceptions.
    /// </summary>
    /// <remarks>
    /// This is a smoke test that validates basic thread-safety:
    /// <list type="bullet">
    /// <item>Method can be called concurrently.</item>
    /// <item>No exceptions are thrown.</item>
    /// <item>Each call produces console output.</item>
    /// </list>
    ///
    /// Output ordering is not asserted.
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnOutputLine_ShouldHandleConcurrentCallsWithContext.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task OnOutputLine_ShouldHandleConcurrentCallsWithContext()
    {
        // Arrange
        var observer = new ConsoleContextualObserver();
        var originalOut = Console.Out;

        using var buffer = new StringWriter();
        var synchronizedWriter = TextWriter.Synchronized(buffer);
        Console.SetOut(synchronizedWriter);

        const int callCount = 100;

        try
        {
            // Act
            var tasks = Enumerable.Range(0, callCount)
                .Select(_ =>
                {
                    var request = new ProcessRequest();
                    return Task.Run(() =>
                        observer.OnOutputLine(request, "parallel"));
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
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Verifies that the same process request identifier
    /// is consistently used across multiple output
    /// notifications for a single request.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>All output lines contain the same request identifier.</item>
    /// <item>The identifier matches <see cref="ProcessRequest.Id"/>.</item>
    /// <item>Multiple invocations do not alter or replace the identifier.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnOutputLine_ShouldUseSameRequestIdAcrossMultipleCalls.
/// </summary>
    public void OnOutputLine_ShouldUseSameRequestIdAcrossMultipleCalls()
    {
        // Arrange
        var observer = new ConsoleContextualObserver();
        var request = new ProcessRequest();
        var originalOut = Console.Out;

        using var buffer = new StringWriter();
        Console.SetOut(TextWriter.Synchronized(buffer));

        try
        {
            // Act
            observer.OnOutputLine(request, "first");
            observer.OnOutputLine(request, "second");
            observer.OnOutputLine(request, "third");

            // Assert
            var output = buffer.ToString();

            output.Should().Contain(request.Id.ToString());

            var occurrences = output
                .Split(request.Id.ToString(), StringSplitOptions.None)
                .Length - 1;

            occurrences.Should().Be(3);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

}
