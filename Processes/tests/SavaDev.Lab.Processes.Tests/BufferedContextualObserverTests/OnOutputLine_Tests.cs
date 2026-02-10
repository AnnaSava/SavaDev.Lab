using AwesomeAssertions;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.BufferedContextualObserverTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="BufferedContextualObserver"/> when handling
/// standard output lines with execution context.
/// </summary>
/// <remarks>
/// These tests ensure that output lines are buffered
/// together with their originating <see cref="ProcessRequest"/>.
/// </remarks>
public sealed class OnOutputLine_Tests
{
    /// <summary>
    /// Verifies that a standard output line is added
    /// to the internal output buffer together with
    /// the originating request.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The line is stored in the standard output buffer.</item>
    /// <item>The request reference is preserved.</item>
    /// <item>The buffer contains exactly one entry.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnOutputLine_ShouldAddLineToStandardOutputBuffer.
/// </summary>
    public void OnOutputLine_ShouldAddLineToStandardOutputBuffer()
    {
        // Arrange
        var observer = new BufferedContextualObserver();
        var request = new ProcessRequest();

        // Act
        observer.OnOutputLine(request, "hello");

        // Assert
        observer.StandardOutputLines.Should().ContainSingle();
        observer.StandardOutputLines[0].Request.Should().BeSameAs(request);
        observer.StandardOutputLines[0].Line.Should().Be("hello");
    }

    /// <summary>
    /// Verifies that multiple output lines are stored
    /// in the order they were received.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>All output lines are preserved.</item>
    /// <item>The original order of lines is maintained.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnOutputLine_ShouldPreserveOrderOfLines.
/// </summary>
    public void OnOutputLine_ShouldPreserveOrderOfLines()
    {
        // Arrange
        var observer = new BufferedContextualObserver();
        var request = new ProcessRequest();

        // Act
        observer.OnOutputLine(request, "first");
        observer.OnOutputLine(request, "second");
        observer.OnOutputLine(request, "third");

        // Assert
        observer.StandardOutputLines
            .Select(item => item.Line)
            .Should().Equal(
                "first",
                "second",
                "third");
    }

    /// <summary>
    /// Verifies that calling <c>OnOutputLine</c>
    /// does not affect the standard error buffer.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Standard error buffer remains empty.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnOutputLine_ShouldNotAffectStandardErrorBuffer.
/// </summary>
    public void OnOutputLine_ShouldNotAffectStandardErrorBuffer()
    {
        // Arrange
        var observer = new BufferedContextualObserver();
        var request = new ProcessRequest();

        // Act
        observer.OnOutputLine(request, "output");

        // Assert
        observer.StandardErrorLines.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that <c>OnOutputLine</c> can be invoked
    /// concurrently without data loss or corruption.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>No exceptions are thrown under concurrent execution.</item>
    /// <item>All output lines are recorded.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnOutputLine_ShouldHandleConcurrentCalls.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task OnOutputLine_ShouldHandleConcurrentCalls()
    {
        // Arrange
        var observer = new BufferedContextualObserver();
        const int callCount = 100;

        // Act
        var tasks = Enumerable.Range(0, callCount)
            .Select(i =>
            {
                var request = new ProcessRequest();
                return Task.Run(() => observer.OnOutputLine(request, $"line-{i}"));
            })
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        observer.StandardOutputLines.Count.Should().Be(callCount);
    }

    /// <summary>
    /// Verifies that an empty output line is still
    /// stored in the standard output buffer.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>An empty string is accepted as a valid output line.</item>
    /// <item>The empty line is stored without modification.</item>
    /// <item>The buffer contains exactly one entry.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnOutputLine_WithEmptyLine_ShouldStoreEmptyLine.
/// </summary>
    public void OnOutputLine_WithEmptyLine_ShouldStoreEmptyLine()
    {
        // Arrange
        var observer = new BufferedContextualObserver();
        var request = new ProcessRequest();

        // Act
        observer.OnOutputLine(request, string.Empty);

        // Assert
        observer.StandardOutputLines.Should().ContainSingle();
        observer.StandardOutputLines[0].Line.Should().Be(string.Empty);
    }

    /// <summary>
    /// Verifies that <see cref="BufferedContextualObserver.StandardOutputLines"/>
    /// returns a snapshot of the buffered output rather than
    /// a live view of the internal collection.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The returned collection represents a snapshot at the time of access.</item>
    /// <item>Subsequent calls to <c>OnOutputLine</c> do not modify previously retrieved collections.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests StandardOutputLines_ShouldReturnSnapshot.
/// </summary>
    public void StandardOutputLines_ShouldReturnSnapshot()
    {
        // Arrange
        var observer = new BufferedContextualObserver();
        var request = new ProcessRequest();
        observer.OnOutputLine(request, "first");

        // Act
        var snapshot = observer.StandardOutputLines;

        observer.OnOutputLine(request, "second");

        // Assert
        snapshot.Should().ContainSingle();
        snapshot[0].Line.Should().Be("first");

        observer.StandardOutputLines.Should().HaveCount(2);
    }
}

