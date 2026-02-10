using AwesomeAssertions;
using NSubstitute;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;
using SavaDev.Lab.Processes.Tests.ProcessOutputObserverResolverTests.Setup;

namespace SavaDev.Lab.Processes.Tests.ProcessOutputObserverResolverTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="ProcessOutputObserverResolver.ResolveErrorHandler(ProcessRequest, IReadOnlyList{IProcessOutputObserver})"/>.
/// </summary>
/// <remarks>
/// These tests ensure that error handlers dispatch lines
/// to the appropriate observer interfaces.
/// </remarks>
public sealed class ResolveErrorHandler_Tests
{
    /// <summary>
    /// Creates a resolver instance for the current test.
    /// </summary>
    private static ProcessOutputObserverResolver CreateResolver()
        => new();

    /// <summary>
    /// Verifies that resolving an error handler with
    /// an empty observer list produces a no-op handler.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The returned handler is not <c>null</c>.</item>
    /// <item>Invoking the handler does not throw exceptions.</item>
    /// <item>No error output is dispatched to any observers.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveErrorHandler_WithNoObservers_ShouldReturnNoOpHandler.
/// </summary>
    public void ResolveErrorHandler_WithNoObservers_ShouldReturnNoOpHandler()
    {
        // Arrange
        var resolver = CreateResolver();
        var request = new ProcessRequest();
        var observers = Array.Empty<IProcessOutputObserver>();

        // Act
        var handler = resolver.ResolveErrorHandler(request, observers);

        // Assert
        handler.Should().NotBeNull();
        handler.Invoking(h => h("error")).Should().NotThrow();
    }

    /// <summary>
    /// Verifies that a basic process output observer
    /// receives standard error output without execution context.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The error line is dispatched to <see cref="IProcessOutputBasicObserver.OnErrorLine(string)"/>.</item>
    /// <item>The execution context (<see cref="ProcessRequest"/>) is not passed.</item>
    /// <item>The error line is delivered exactly once.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveErrorHandler_WithBasicObserver_ShouldDispatchErrorLine.
/// </summary>
    public void ResolveErrorHandler_WithBasicObserver_ShouldDispatchErrorLine()
    {
        // Arrange
        var resolver = CreateResolver();
        var request = new ProcessRequest();

        var observer = Substitute.For<IProcessOutputBasicObserver>();
        var observers = new IProcessOutputObserver[] { observer };

        var handler = resolver.ResolveErrorHandler(request, observers);

        // Act
        handler("boom");

        // Assert
        observer.Received(1).OnErrorLine("boom");
    }

    /// <summary>
    /// Verifies that a process-aware output observer
    /// receives standard error output together with
    /// the originating process request.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The error line is dispatched to <see cref="IProcessOutputContextualObserver.OnErrorLine(ProcessRequest, string)"/>.</item>
    /// <item>The provided <see cref="ProcessRequest"/> instance is passed unchanged.</item>
    /// <item>No non-contextual error callbacks are invoked.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveErrorHandler_WithAwareObserver_ShouldDispatchErrorWithRequest.
/// </summary>
    public void ResolveErrorHandler_WithAwareObserver_ShouldDispatchErrorWithRequest()
    {
        // Arrange
        var resolver = CreateResolver();
        var request = new ProcessRequest();

        var observer = Substitute.For<IProcessOutputContextualObserver>();
        var observers = new IProcessOutputObserver[] { observer };

        var handler = resolver.ResolveErrorHandler(request, observers);

        // Act
        handler("contextual error");

        // Assert
        observer.Received(1).OnErrorLine(request, "contextual error");
    }

    /// <summary>
    /// Verifies that standard error lines are dispatched
    /// to all applicable observers when multiple observer
    /// types are registered.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Basic output observers receive non-contextual error output.</item>
    /// <item>Process-aware observers receive error output together with execution context.</item>
    /// <item>All observers are notified for a single error line.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveErrorHandler_WithMixedObservers_ShouldDispatchToAll.
/// </summary>
    public void ResolveErrorHandler_WithMixedObservers_ShouldDispatchToAll()
    {
        // Arrange
        var resolver = CreateResolver();
        var request = new ProcessRequest();

        var basic = Substitute.For<IProcessOutputBasicObserver>();
        var aware = Substitute.For<IProcessOutputContextualObserver>();

        var observers = new IProcessOutputObserver[]
        {
            basic,
            aware
        };

        var handler = resolver.ResolveErrorHandler(request, observers);

        // Act
        handler("mixed error");

        // Assert
        basic.Received(1).OnErrorLine("mixed error");
        aware.Received(1).OnErrorLine(request, "mixed error");
    }

    /// <summary>
    /// Verifies that the resolved error handler can be
    /// invoked concurrently from multiple threads.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The handler can be invoked concurrently without throwing exceptions.</item>
    /// <item>All error output invocations are forwarded to the registered observers.</item>
    /// <item>No error notifications are lost under concurrent execution.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveErrorHandler_ShouldHandleConcurrentInvocations.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task ResolveErrorHandler_ShouldHandleConcurrentInvocations()
    {
        // Arrange
        var resolver = CreateResolver();
        var request = new ProcessRequest();

        var observer = Substitute.For<IProcessOutputBasicObserver>();
        var observers = new IProcessOutputObserver[] { observer };

        var handler = resolver.ResolveErrorHandler(request, observers);

        const int callCount = 100;

        // Act
        var tasks = Enumerable.Range(0, callCount)
            .Select(_ => Task.Run(() => handler("parallel error")))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        observer.Received(callCount).OnErrorLine("parallel error");
    }

    /// <summary>
    /// Verifies that the resolved error handler passes
    /// the same <see cref="ProcessRequest"/> instance to
    /// all observer invocations.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The same <see cref="ProcessRequest"/> instance is used for every invocation.</item>
    /// <item>The request instance is not replaced or recreated between calls.</item>
    /// <item>All error notifications reference the original request object.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveErrorHandler_ShouldPassSameRequestInstanceToAllInvocations.
/// </summary>
    public void ResolveErrorHandler_ShouldPassSameRequestInstanceToAllInvocations()
    {
        // Arrange
        var resolver = new ProcessOutputObserverResolver();
        var request = new ProcessRequest();

        var receivedRequests = new List<ProcessRequest>();

        var observer = Substitute.For<IProcessOutputContextualObserver>();
        observer
            .When(o => o.OnErrorLine(Arg.Any<ProcessRequest>(), Arg.Any<string>()))
            .Do(ci => receivedRequests.Add(ci.Arg<ProcessRequest>()));

        var observers = new IProcessOutputObserver[] { observer };

        var handler = resolver.ResolveErrorHandler(request, observers);

        // Act
        handler("error 1");
        handler("error 2");
        handler("error 3");

        // Assert
        receivedRequests.Should().NotBeEmpty();
        receivedRequests.Should().AllBeEquivalentTo(request);
    }

    /// <summary>
    /// Verifies that standard error lines are dispatched
    /// to observers in the order they are provided.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Observers are invoked in the same order as they appear in the observer list.</item>
    /// <item>Each observer receives the error line exactly once.</item>
    /// <item>No observer is skipped or invoked out of order.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveErrorHandler_ShouldInvokeObserversInOrder.
/// </summary>
    public void ResolveErrorHandler_ShouldInvokeObserversInOrder()
    {
        // Arrange
        var resolver = new ProcessOutputObserverResolver();
        var request = new ProcessRequest();

        var callOrder = new List<string>();

        var first = Substitute.For<IProcessOutputBasicObserver>();
        first
            .When(o => o.OnErrorLine("ordered error"))
            .Do(_ => callOrder.Add("first"));

        var second = Substitute.For<IProcessOutputBasicObserver>();
        second
            .When(o => o.OnErrorLine("ordered error"))
            .Do(_ => callOrder.Add("second"));

        var third = Substitute.For<IProcessOutputBasicObserver>();
        third
            .When(o => o.OnErrorLine("ordered error"))
            .Do(_ => callOrder.Add("third"));

        var observers = new IProcessOutputObserver[]
        {
        first,
        second,
        third
        };

        var handler = resolver.ResolveErrorHandler(request, observers);

        // Act
        handler("ordered error");

        // Assert
        callOrder.Should().Equal("first", "second", "third");
    }

    /// <summary>
    /// Verifies that when an observer implements both
    /// <see cref="IProcessOutputBasicObserver"/> and
    /// <see cref="IProcessOutputContextualObserver"/>,
    /// the contextual error callback is used.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The observer receives error output via the aware callback.</item>
    /// <item>The basic error callback is not invoked.</item>
    /// <item>The same <see cref="ProcessRequest"/> instance is passed through.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveErrorHandler_WithObserverImplementingBothInterfaces_ShouldUseAwareCallback.
/// </summary>
    public void ResolveErrorHandler_WithObserverImplementingBothInterfaces_ShouldUseAwareCallback()
    {
        // Arrange
        var resolver = new ProcessOutputObserverResolver();
        var request = new ProcessRequest();

        var observer = new DualObserverSpy();
        var observers = new IProcessOutputObserver[] { observer };

        var handler = resolver.ResolveErrorHandler(request, observers);

        // Act
        handler("test-error");

        // Assert
        observer.AwareErrorCalls.Should().Be(1);
        observer.BasicErrorCalls.Should().Be(0);

        observer.LastRequest.Should().BeSameAs(request);
        observer.LastLine.Should().Be("test-error");
    }

    /// <summary>
    /// Verifies that observers which do not implement
    /// any supported output observer interfaces are ignored
    /// when resolving an error handler.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Observers not implementing supported interfaces are ignored.</item>
    /// <item>No exceptions are thrown during handler invocation.</item>
    /// <item>Supported observers still receive error notifications.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveErrorHandler_WithUnsupportedObserverTypes_ShouldIgnoreThem.
/// </summary>
    public void ResolveErrorHandler_WithUnsupportedObserverTypes_ShouldIgnoreThem()
    {
        // Arrange
        var resolver = new ProcessOutputObserverResolver();
        var request = new ProcessRequest();

        var supportedObserver = new ErrorObserverSpy();
        var unsupportedObserver = new UnsupportedObserver();

        var observers = new IProcessOutputObserver[]
        {
        unsupportedObserver,
        supportedObserver
        };

        var handler = resolver.ResolveErrorHandler(request, observers);

        // Act
        handler("test-error");

        // Assert
        supportedObserver.ErrorCalls.Should().Be(1);
        supportedObserver.LastLine.Should().Be("test-error");
    }

}

