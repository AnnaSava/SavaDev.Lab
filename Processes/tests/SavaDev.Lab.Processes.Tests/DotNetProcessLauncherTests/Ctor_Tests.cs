using AwesomeAssertions;
using SavaDev.Lab.Processes.Launchers.DotNet;

namespace SavaDev.Lab.Processes.Tests.DotNetProcessLauncherTests;

/// <summary>
/// Contains tests verifying constructor behavior of
/// <see cref="DotNetProcessLauncher"/>.
/// </summary>
/// <remarks>
/// These tests ensure that required dependencies are validated
/// and that invalid constructor arguments are rejected.
/// </remarks>
public sealed class Ctor_Tests
{
    /// <summary>
    /// Verifies that passing a <c>null</c> process launcher
    /// throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The constructor rejects <c>null</c> dependencies.</item>
    /// <item>The exception parameter name is <c>processLauncher</c>.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests Ctor_WithNullProcessLauncher_ShouldThrowArgumentNullException.
/// </summary>
    public void Ctor_WithNullProcessLauncher_ShouldThrowArgumentNullException()
    {
        // Act
        var action = () => new DotNetProcessLauncher(null!);

        // Assert
        var exception = action.Should().Throw<ArgumentNullException>()
            .Which;

        exception.ParamName.Should().Be("processLauncher");
    }
}

