using FluentAssertions;
using JobManagement.Domain.Identifiers;
using Xunit;

namespace JobManagement.Tests.Unit.Abstractions;

public sealed class JobIdTests
{
    [Fact]
    public void NewShouldCreateNonEmptyId()
    {
        var id = JobId.New();
        id.ToString().Should().NotBeNullOrWhiteSpace();
        id.ToString().Should().NotBe("00000000-0000-0000-0000-000000000000");
    }

    [Fact]
    public void TryParseShouldReturnTrueForValidGuid()
    {
        var id = JobId.New();
        JobId.TryParse(id.ToString(), out JobId parsed).Should().BeTrue();
        parsed.Should().Be(id);
    }

    [Fact]
    public void TryParseShouldReturnFalseForInvalidValue()
    {
        JobId.TryParse("not-a-guid", out _).Should().BeFalse();
    }
}