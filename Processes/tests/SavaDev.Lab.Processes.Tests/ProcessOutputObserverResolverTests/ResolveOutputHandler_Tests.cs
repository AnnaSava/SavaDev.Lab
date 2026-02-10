using AwesomeAssertions;
using NSubstitute;
using SavaDev.Lab.Processes.Models;
using SavaDev.Lab.Processes.Observers;
using SavaDev.Lab.Processes.Observers.Resolver;
using SavaDev.Lab.Processes.Tests.ProcessOutputObserverResolverTests.Setup;

namespace SavaDev.Lab.Processes.Tests.ProcessOutputObserverResolverTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="ProcessOutputObserverResolver.ResolveOutputHandler(ProcessRequest, IReadOnlyList{IProcessOutputObserver})"/>.
/// </summary>
/// <remarks>
/// These tests ensure that output handlers dispatch lines
/// to the appropriate observer interfaces.
/// </remarks>
public sealed class ResolveOutputHandler_Tests
{
    /// <summary>
    /// Creates a resolver instance for the current test.
    /// </summary>
    private static ProcessOutputObserverResolver CreateResolver()
        => new();

    /// <summary>
    /// Verifies that resolving an output handler with
    /// an empty observer list produces a no-op handler.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The returned handler is not <c>null</c>.</item>
    /// <item>Invoking the handler does not throw exceptions.</item>
    /// <item>No output is dispatched to any observers.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveOutputHandler_WithNoObservers_ShouldReturnNoOpHandler.
/// </summary>
    public void ResolveOutputHandler_WithNoObservers_ShouldReturnNoOpHandler()
    {
        // Arrange
        var resolver = CreateResolver();
        var request = new ProcessRequest();
        var observers = Array.Empty<IProcessOutputObserver>();

        // Act
        var handler = resolver.ResolveOutputHandler(request, observers);

        // Assert
        handler.Should().NotBeNull();
        handler.Invoking(h => h("test")).Should().NotThrow();
    }

    /// <summary>
    /// Verifies that a basic process output observer
    /// receives standard output lines without execution context.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The output line is dispatched to <see cref="IProcessOutputBasicObserver.OnOutputLine(string)"/>.</item>
    /// <item>The execution context (<see cref="ProcessRequest"/>) is not passed.</item>
    /// <item>The output line is delivered exactly once.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveOutputHandler_WithBasicObserver_ShouldDispatchOutputLine.
/// </summary>
    public void ResolveOutputHandler_WithBasicObserver_ShouldDispatchOutputLine()
    {
        // Arrange
        var resolver = CreateResolver();
        var request = new ProcessRequest();

        var observer = Substitute.For<IProcessOutputBasicObserver>();
        var observers = new IProcessOutputObserver[] { observer };

        var handler = resolver.ResolveOutputHandler(request, observers);

        // Act
        handler("hello");

        // Assert
        observer.Received(1).OnOutputLine("hello");
    }

    /// <summary>
    /// Verifies that a process-aware output observer
    /// receives standard output lines together with
    /// the originating process request.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The output line is dispatched to <see cref="IProcessOutputContextualObserver.OnOutputLine(ProcessRequest, string)"/>.</item>
    /// <item>The provided <see cref="ProcessRequest"/> instance is passed unchanged.</item>
    /// <item>No non-contextual output callbacks are invoked.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveOutputHandler_WithAwareObserver_ShouldDispatchOutputWithRequest.
/// </summary>
    public void ResolveOutputHandler_WithAwareObserver_ShouldDispatchOutputWithRequest()
    {
        // Arrange
        var resolver = CreateResolver();
        var request = new ProcessRequest();

        var observer = Substitute.For<IProcessOutputContextualObserver>();
        var observers = new IProcessOutputObserver[] { observer };

        var handler = resolver.ResolveOutputHandler(request, observers);

        // Act
        handler("contextual");

        // Assert
        observer.Received(1).OnOutputLine(request, "contextual");
    }

    /// <summary>
    /// Verifies that standard output lines are dispatched
    /// to all applicable observers when multiple observer
    /// types are registered.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Basic output observers receive non-contextual output.</item>
    /// <item>Process-aware observers receive output together with execution context.</item>
    /// <item>All observers are notified for a single output line.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveOutputHandler_WithMixedObservers_ShouldDispatchToAll.
/// </summary>
    public void ResolveOutputHandler_WithMixedObservers_ShouldDispatchToAll()
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

        var handler = resolver.ResolveOutputHandler(request, observers);

        // Act
        handler("mixed");

        // Assert
        basic.Received(1).OnOutputLine("mixed");
        aware.Received(1).OnOutputLine(request, "mixed");
    }

    /// <summary>
    /// Verifies that the resolved output handler can be
    /// invoked concurrently from multiple threads.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The handler can be invoked concurrently without throwing exceptions.</item>
    /// <item>All output invocations are forwarded to the registered observers.</item>
    /// <item>No output notifications are lost under concurrent execution.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveOutputHandler_ShouldHandleConcurrentInvocations.
/// </summary>
/// <returns>A task that represents the asynchronous test execution.</returns>
    public async Task ResolveOutputHandler_ShouldHandleConcurrentInvocations()
    {
        // Arrange
        var resolver = CreateResolver();
        var request = new ProcessRequest();

        var observer = Substitute.For<IProcessOutputBasicObserver>();
        var observers = new IProcessOutputObserver[] { observer };

        var handler = resolver.ResolveOutputHandler(request, observers);

        const int callCount = 100;

        // Act
        var tasks = Enumerable.Range(0, callCount)
            .Select(_ => Task.Run(() => handler("parallel")))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        observer.Received(callCount).OnOutputLine("parallel");
    }

    /// <summary>
    /// Verifies that the resolved output handler passes
    /// the same <see cref="ProcessRequest"/> instance to
    /// all observer invocations.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The same <see cref="ProcessRequest"/> instance is used for every invocation.</item>
    /// <item>The request instance is not replaced or recreated between calls.</item>
    /// <item>All output notifications reference the original request object.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveOutputHandler_ShouldPassSameRequestInstanceToAllInvocations.
/// </summary>
    public void ResolveOutputHandler_ShouldPassSameRequestInstanceToAllInvocations()
    {
        // Arrange
        var resolver = new ProcessOutputObserverResolver();
        var request = new ProcessRequest();

        var receivedRequests = new List<ProcessRequest>();

        var observer = Substitute.For<IProcessOutputContextualObserver>();
        observer
            .When(o => o.OnOutputLine(Arg.Any<ProcessRequest>(), Arg.Any<string>()))
            .Do(ci => receivedRequests.Add(ci.Arg<ProcessRequest>()));

        var observers = new IProcessOutputObserver[] { observer };

        var handler = resolver.ResolveOutputHandler(request, observers);

        // Act
        handler("first");
        handler("second");
        handler("third");

        // Assert
        receivedRequests.Should().NotBeEmpty();
        receivedRequests.Should().AllBeEquivalentTo(request);
    }

    /// <summary>
    /// Verifies that standard output lines are dispatched
    /// to observers in the order they are provided.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Observers are invoked in the same order as they appear in the observer list.</item>
    /// <item>Each observer receives the output line exactly once.</item>
    /// <item>No observer is skipped or invoked out of order.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveOutputHandler_ShouldInvokeObserversInOrder.
/// </summary>
    public void ResolveOutputHandler_ShouldInvokeObserversInOrder()
    {
        // Arrange
        var resolver = new ProcessOutputObserverResolver();
        var request = new ProcessRequest();

        var callOrder = new List<string>();

        var first = Substitute.For<IProcessOutputBasicObserver>();
        first
            .When(o => o.OnOutputLine("ordered"))
            .Do(_ => callOrder.Add("first"));

        var second = Substitute.For<IProcessOutputBasicObserver>();
        second
            .When(o => o.OnOutputLine("ordered"))
            .Do(_ => callOrder.Add("second"));

        var third = Substitute.For<IProcessOutputBasicObserver>();
        third
            .When(o => o.OnOutputLine("ordered"))
            .Do(_ => callOrder.Add("third"));

        var observers = new IProcessOutputObserver[]
        {
        first,
        second,
        third
        };

        var handler = resolver.ResolveOutputHandler(request, observers);

        // Act
        handler("ordered");

        // Assert
        callOrder.Should().Equal("first", "second", "third");
    }

    /// <summary>
    /// Verifies that when an observer implements both
    /// <see cref="IProcessOutputBasicObserver"/> and
    /// <see cref="IProcessOutputContextualObserver"/>,
    /// the contextual output callback is used.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The observer receives output via the aware callback.</item>
    /// <item>The basic output callback is not invoked.</item>
    /// <item>The same <see cref="ProcessRequest"/> instance is passed through.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveOutputHandler_WithObserverImplementingBothInterfaces_ShouldUseAwareCallback.
/// </summary>
    public void ResolveOutputHandler_WithObserverImplementingBothInterfaces_ShouldUseAwareCallback()
    {
        // Arrange
        var resolver = new ProcessOutputObserverResolver();
        var request = new ProcessRequest();

        var observer = new DualObserverSpy();
        var observers = new IProcessOutputObserver[] { observer };

        var handler = resolver.ResolveOutputHandler(request, observers);

        // Act
        handler("test-output");

        // Assert
        observer.AwareOutputCalls.Should().Be(1);
        observer.BasicOutputCalls.Should().Be(0);

        observer.LastRequest.Should().BeSameAs(request);
        observer.LastLine.Should().Be("test-output");
    }

    /// <summary>
    /// Verifies that observers which do not implement
    /// any supported output observer interfaces are ignored
    /// when resolving an output handler.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>Observers not implementing supported interfaces are ignored.</item>
    /// <item>No exceptions are thrown during handler invocation.</item>
    /// <item>Supported observers still receive output notifications.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ResolveOutputHandler_WithUnsupportedObserverTypes_ShouldIgnoreThem.
/// </summary>
    public void ResolveOutputHandler_WithUnsupportedObserverTypes_ShouldIgnoreThem()
    {
        // Arrange
        var resolver = new ProcessOutputObserverResolver();
        var request = new ProcessRequest();

        var supportedObserver = new OutputObserverSpy();
        var unsupportedObserver = new UnsupportedObserver();

        var observers = new IProcessOutputObserver[]
        {
        unsupportedObserver,
        supportedObserver
        };

        var handler = resolver.ResolveOutputHandler(request, observers);

        // Act
        handler("test-output");

        // Assert
        supportedObserver.OutputCalls.Should().Be(1);
        supportedObserver.LastLine.Should().Be("test-output");
    }
}

