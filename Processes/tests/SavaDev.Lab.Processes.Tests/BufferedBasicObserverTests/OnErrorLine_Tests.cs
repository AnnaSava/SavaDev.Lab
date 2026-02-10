using AwesomeAssertions;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.BufferedBasicObserverTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="BufferedBasicObserver"/> when handling
/// standard error lines.
/// </summary>
/// <remarks>
/// These tests ensure that buffered error lines are
/// stored correctly and that error buffering behavior
/// remains thread-safe and deterministic.
/// </remarks>
public class OnErrorLine_Tests
{
    /// <summary>
    /// Verifies that a standard error line is added
    /// to the internal error output buffer.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The line is stored in the standard error buffer.</item>
    /// <item>The buffer contains exactly one entry.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnErrorLine_ShouldAddLineToStandardErrorBuffer.
/// </summary>
    public void OnErrorLine_ShouldAddLineToStandardErrorBuffer()
    {
        // Arrange
        var observer = new BufferedBasicObserver();

        // Act
        observer.OnErrorLine("error");

        // Assert
        observer.StandardErrorLines.Should().ContainSingle();
        observer.StandardErrorLines[0].Should().Be("error");
    }

    /// <summary>
    /// Verifies that multiple error output lines are stored
    /// in the order they were received.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>All error lines are preserved.</item>
    /// <item>The original order of lines is maintained.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnErrorLine_ShouldPreserveOrderOfLines.
/// </summary>
    public void OnErrorLine_ShouldPreserveOrderOfLines()
    {
        // Arrange
        var observer = new BufferedBasicObserver();

        // Act
        observer.OnErrorLine("first-error");
        observer.OnErrorLine("second-error");
        observer.OnErrorLine("third-error");

        // Assert
        observer.StandardErrorLines.Should().Equal(
            "first-error",
            "second-error",
            "third-error");
    }

    /// <summary>
    /// Verifies that calling <c>OnErrorLine</c>
    /// does not affect the standard output buffer.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Standard output buffer remains empty.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnErrorLine_ShouldNotAffectStandardOutputBuffer.
/// </summary>
    public void OnErrorLine_ShouldNotAffectStandardOutputBuffer()
    {
        // Arrange
        var observer = new BufferedBasicObserver();

        // Act
        observer.OnErrorLine("error");

        // Assert
        observer.StandardOutputLines.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that <c>OnErrorLine</c> can be invoked
    /// concurrently without data loss or corruption.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>No exceptions are thrown under concurrent execution.</item>
    /// <item>All error lines are recorded.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnErrorLine_ShouldHandleConcurrentCalls.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task OnErrorLine_ShouldHandleConcurrentCalls()
    {
        // Arrange
        var observer = new BufferedBasicObserver();
        const int callCount = 100;

        // Act
        var tasks = Enumerable.Range(0, callCount)
            .Select(i =>
                Task.Run(() => observer.OnErrorLine($"error-{i}")))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        observer.StandardErrorLines.Count.Should().Be(callCount);
    }

    /// <summary>
    /// Verifies that an empty error line is still
    /// stored in the standard error buffer.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>An empty string is accepted as a valid error line.</item>
    /// <item>The empty line is stored without modification.</item>
    /// <item>The buffer contains exactly one entry.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OnErrorLine_WithEmptyLine_ShouldStoreEmptyLine.
/// </summary>
    public void OnErrorLine_WithEmptyLine_ShouldStoreEmptyLine()
    {
        // Arrange
        var observer = new BufferedBasicObserver();

        // Act
        observer.OnErrorLine(string.Empty);

        // Assert
        observer.StandardErrorLines.Should().ContainSingle();
        observer.StandardErrorLines[0].Should().Be(string.Empty);
    }

    /// <summary>
    /// Verifies that <see cref="BufferedBasicObserver.StandardErrorLines"/>
    /// returns a snapshot of the buffered error output rather than
    /// a live view of the internal collection.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The returned collection represents a snapshot at the time of access.</item>
    /// <item>Subsequent calls to <c>OnErrorLine</c> do not modify previously retrieved collections.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests StandardErrorLines_ShouldReturnSnapshot.
/// </summary>
    public void StandardErrorLines_ShouldReturnSnapshot()
    {
        // Arrange
        var observer = new BufferedBasicObserver();
        observer.OnErrorLine("first-error");

        // Act
        var snapshot = observer.StandardErrorLines;

        observer.OnErrorLine("second-error");

        // Assert
        snapshot.Should().ContainSingle();
        snapshot[0].Should().Be("first-error");

        observer.StandardErrorLines.Should().HaveCount(2);
    }

}

