using AwesomeAssertions;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.BufferedBasicObserverTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="BufferedBasicObserver"/> when handling
/// standard output lines.
/// </summary>
/// <remarks>
/// These tests ensure that buffered output lines are
/// stored correctly and that output buffering behavior
/// remains thread-safe and deterministic.
/// </remarks>
public sealed class OnOutputLine_Tests
{
    /// <summary>
    /// Verifies that a standard output line is added
    /// to the internal output buffer.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The line is stored in the standard output buffer.</item>
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
        var observer = new BufferedBasicObserver();

        // Act
        observer.OnOutputLine("hello");

        // Assert
        observer.StandardOutputLines.Should().ContainSingle();
        observer.StandardOutputLines[0].Should().Be("hello");
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
        var observer = new BufferedBasicObserver();

        // Act
        observer.OnOutputLine("first");
        observer.OnOutputLine("second");
        observer.OnOutputLine("third");

        // Assert
        observer.StandardOutputLines.Should().Equal(
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
        var observer = new BufferedBasicObserver();

        // Act
        observer.OnOutputLine("output");

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
        var observer = new BufferedBasicObserver();
        const int callCount = 100;

        // Act
        var tasks = Enumerable.Range(0, callCount)
            .Select(i =>
                Task.Run(() => observer.OnOutputLine($"line-{i}")))
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
        var observer = new BufferedBasicObserver();

        // Act
        observer.OnOutputLine(string.Empty);

        // Assert
        observer.StandardOutputLines.Should().ContainSingle();
        observer.StandardOutputLines[0].Should().Be(string.Empty);
    }

    /// <summary>
    /// Verifies that <see cref="BufferedBasicObserver.StandardOutputLines"/>
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
        var observer = new BufferedBasicObserver();
        observer.OnOutputLine("first");

        // Act
        var snapshot = observer.StandardOutputLines;

        observer.OnOutputLine("second");

        // Assert
        snapshot.Should().ContainSingle();
        snapshot[0].Should().Be("first");

        observer.StandardOutputLines.Should().HaveCount(2);
    }
}

