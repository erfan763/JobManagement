using FluentAssertions;
using JobManagement.Abstractions.Persistence;
using JobManagement.Domain.Identifiers;
using Xunit;

namespace JobManagement.Tests.Unit.Abstractions;

public sealed class LeaseTokenTests
{
    [Fact]
    public void LeaseTokenShouldBeNonEmptyString()
    {
        var lease = new JobLease
        {
            JobId = JobId.New(),
            LeaseToken = Guid.NewGuid().ToString("N"),
            WorkerId = "w1",
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(1),
        };

        lease.LeaseToken.Should().NotBeNullOrWhiteSpace();
    }
}