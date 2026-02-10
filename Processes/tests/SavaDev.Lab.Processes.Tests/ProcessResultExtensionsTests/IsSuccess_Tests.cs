using AwesomeAssertions;
using SavaDev.Lab.Processes.Extensions;
using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.ProcessResultExtensionsTests;

/// <summary>
/// Tests for <see cref="ProcessResultExtensions.IsSuccess(ProcessResult)"/>.
/// </summary>
public sealed class IsSuccess_Tests
{
    [Fact]
/// <summary>
/// Tests IsSuccess_ShouldReturnTrue_WhenExitCodeIsZero.
/// </summary>
    public void IsSuccess_ShouldReturnTrue_WhenExitCodeIsZero()
    {
        var result = new ProcessResult { ExitCode = 0 };

        result.IsSuccess().Should().BeTrue();
    }

    [Fact]
/// <summary>
/// Tests IsSuccess_ShouldReturnFalse_WhenExitCodeIsNonZero.
/// </summary>
    public void IsSuccess_ShouldReturnFalse_WhenExitCodeIsNonZero()
    {
        var result = new ProcessResult { ExitCode = 5 };

        result.IsSuccess().Should().BeFalse();
    }

    [Fact]
/// <summary>
/// Tests IsSuccess_ShouldThrow_WhenResultIsNull.
/// </summary>
    public void IsSuccess_ShouldThrow_WhenResultIsNull()
    {
        ProcessResult? result = null;

        var act = () => result!.IsSuccess();

        act.Should().Throw<ArgumentNullException>();
    }
}

