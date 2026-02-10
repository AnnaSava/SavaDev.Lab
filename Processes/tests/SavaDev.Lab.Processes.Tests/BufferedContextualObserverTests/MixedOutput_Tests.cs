using AwesomeAssertions;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.BufferedContextualObserverTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="BufferedContextualObserver"/> when handling
/// mixed standard output and error lines with context.
/// </summary>
/// <remarks>
/// These tests ensure that output and error buffers
/// remain isolated while preserving request context.
/// </remarks>
public class MixedOutput_Tests
{
    /// <summary>
    /// Verifies that <c>OnOutputLine</c> and <c>OnErrorLine</c>
    /// can be invoked concurrently without interfering
    /// with each other.
    /// </summary>
    /// <remarks>
    /// This is a smoke test that validates basic thread-safety
    /// when standard output and standard error lines are
    /// reported concurrently with execution context.
    ///
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>No exceptions are thrown during concurrent execution.</item>
    /// <item>All output lines are recorded in the output buffer.</item>
    /// <item>All error lines are recorded in the error buffer.</item>
    /// <item>Standard output and standard error buffers remain isolated.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests OutputAndError_ShouldHandleConcurrentMixedCalls.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task OutputAndError_ShouldHandleConcurrentMixedCalls()
    {
        // Arrange
        var observer = new BufferedContextualObserver();
        const int callCount = 100;

        // Act
        var tasks = Enumerable.Range(0, callCount)
            .SelectMany(i => new[]
            {
                Task.Run(() =>
                    observer.OnOutputLine(new ProcessRequest(), $"out-{i}")),
                Task.Run(() =>
                    observer.OnErrorLine(new ProcessRequest(), $"err-{i}"))
            })
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        observer.StandardOutputLines.Count.Should().Be(callCount);
        observer.StandardErrorLines.Count.Should().Be(callCount);

        observer.StandardOutputLines.Should().OnlyContain(
            entry => entry.Line.StartsWith("out-"));

        observer.StandardErrorLines.Should().OnlyContain(
            entry => entry.Line.StartsWith("err-"));
    }
}

