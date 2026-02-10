using AwesomeAssertions;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.BufferedBasicObserverTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="BufferedBasicObserver"/> when handling
/// mixed standard output and error lines.
/// </summary>
/// <remarks>
/// These tests ensure that output and error buffers
/// remain isolated when lines are produced concurrently.
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
    /// reported concurrently.
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
        var observer = new BufferedBasicObserver();
        const int callCount = 100;

        // Act
        var tasks = Enumerable.Range(0, callCount)
            .SelectMany(i => new[]
            {
        Task.Run(() => observer.OnOutputLine($"out-{i}")),
        Task.Run(() => observer.OnErrorLine($"err-{i}"))
            })
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        observer.StandardOutputLines.Count.Should().Be(callCount);
        observer.StandardErrorLines.Count.Should().Be(callCount);

        observer.StandardOutputLines.Should().OnlyContain(
            line => line.StartsWith("out-"));

        observer.StandardErrorLines.Should().OnlyContain(
            line => line.StartsWith("err-"));
    }
}

