using AwesomeAssertions;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;

namespace SavaDev.Lab.Processes.Tests.ConsoleContextualObserverTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="ConsoleContextualObserver"/> when handling
/// interleaved standard output and standard error notifications
/// for a single process request.
/// </summary>
/// <remarks>
/// These tests ensure that the <see cref="ConsoleContextualObserver"/>
/// can correctly process mixed output streams, where standard
/// output and standard error lines are produced concurrently
/// by the same process.
///
/// The test suite focuses on validating that:
/// <list type="bullet">
/// <item>Output and error callbacks can be invoked concurrently.</item>
/// <item>Both output streams consistently include the same
/// <see cref="ProcessRequest"/> identifier.</item>
/// <item>No exceptions are thrown during mixed execution.</item>
/// </list>
///
/// Output ordering is not asserted, as concurrent execution
/// does not guarantee deterministic ordering.
/// </remarks>
[Collection("Console tests")]
/// <summary>
/// Tests for MixedOutput_Tests.
/// </summary>
public sealed class MixedOutput_Tests
{
    /// <summary>
    /// Verifies that the observer can handle interleaved
    /// standard output and standard error notifications
    /// for a single process request without throwing
    /// exceptions.
    /// </summary>
    /// <remarks>
    /// This is a smoke test that validates the following behavior:
    /// <list type="bullet">
    /// <item>Standard output and error callbacks can be invoked concurrently.</item>
    /// <item>Both output types reference the same process request identifier.</item>
    /// <item>No exceptions are thrown during mixed execution.</item>
    /// </list>
    ///
    /// Output ordering is not asserted, as concurrent execution
    /// does not guarantee deterministic ordering.
    /// </remarks>
    [Fact]
/// <summary>
/// Tests Observer_ShouldHandleMixedOutputAndErrorForSingleRequest.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task Observer_ShouldHandleMixedOutputAndErrorForSingleRequest()
    {
        // Arrange
        var observer = new ConsoleContextualObserver();
        var request = new ProcessRequest();

        var originalOut = Console.Out;
        var originalError = Console.Error;

        using var outBuffer = new StringWriter();
        using var errBuffer = new StringWriter();

        Console.SetOut(TextWriter.Synchronized(outBuffer));
        Console.SetError(TextWriter.Synchronized(errBuffer));

        const int callCount = 100;

        try
        {
            // Act
            var tasks = Enumerable.Range(0, callCount)
                .Select(i =>
                    Task.Run(() =>
                    {
                        if (i % 2 == 0)
                        {
                            observer.OnOutputLine(request, $"out {i}");
                        }
                        else
                        {
                            observer.OnErrorLine(request, $"err {i}");
                        }
                    }))
                .ToArray();

            await Task.WhenAll(tasks);

            // Assert
            var outText = outBuffer.ToString();
            var errText = errBuffer.ToString();

            outText.Should().NotBeNullOrEmpty();
            errText.Should().NotBeNullOrEmpty();

            outText.Should().Contain(request.Id.ToString());
            errText.Should().Contain(request.Id.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }
}

