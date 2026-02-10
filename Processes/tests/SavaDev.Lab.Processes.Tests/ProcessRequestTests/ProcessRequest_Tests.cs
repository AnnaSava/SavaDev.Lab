using AwesomeAssertions;
using SavaDev.Lab.Processes.Models;

namespace SavaDev.Lab.Processes.Tests.ProcessRequestTests;

/// <summary>
/// Contains tests that verify the behavior of
/// <see cref="ProcessRequest"/> related to request
/// identification.
/// </summary>
/// <remarks>
/// These tests ensure that each process request
/// is assigned a unique, non-empty identifier
/// upon creation.
///
/// The request identifier is intended to be used
/// for correlating process execution, output, and
/// diagnostic information across different stages
/// of process handling.
/// </remarks>
public class ProcessRequest_Tests
{
    /// <summary>
    /// Verifies that a <see cref="ProcessRequest"/> automatically
    /// generates a non-empty identifier.
    /// </summary>
    /// <remarks>
    /// Expected behavior:
    /// <list type="bullet">
    /// <item>The request identifier is generated automatically.</item>
    /// <item>The identifier is not equal to <see cref="Guid.Empty"/>.</item>
    /// </list>
    /// </remarks>
    [Fact]
/// <summary>
/// Tests ProcessRequest_ShouldGenerateNonEmptyId.
/// </summary>
    public void ProcessRequest_ShouldGenerateNonEmptyId()
    {
        // Act
        var request = new ProcessRequest();

        // Assert
        request.Id.Should().NotBe(Guid.Empty);
    }
}

