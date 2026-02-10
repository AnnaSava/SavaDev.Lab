using AwesomeAssertions;

namespace SavaDev.Lab.Processes.Tests.ProcessLauncherTests;

/// <summary>
/// Contains tests verifying constructor behavior of
/// <see cref="ProcessLauncher"/>.
/// </summary>
/// <remarks>
/// These tests ensure that required dependencies are validated
/// and that invalid constructor arguments are rejected.
/// </remarks>
public sealed class Ctor_Tests
{
    /// <summary>
    /// Verifies that passing a <c>null</c> observer resolver
    /// throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The constructor rejects <c>null</c> dependencies.</item>
    /// <item>The exception parameter name is <c>observerResolver</c>.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests Ctor_WithNullObserverResolver_ShouldThrowArgumentNullException.
/// </summary>
    public void Ctor_WithNullObserverResolver_ShouldThrowArgumentNullException()
    {
        // Act
        var action = () => new ProcessLauncher(null!);

        // Assert
        var exception = action.Should().Throw<ArgumentNullException>()
            .Which;

        exception.ParamName.Should().Be("observerResolver");
    }
}

