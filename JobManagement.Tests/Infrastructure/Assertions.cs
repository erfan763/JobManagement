using FluentAssertions;
using JobManagement.Domain.Enums;
using JobManagement.Domain.Models;

namespace JobManagement.Tests.Infrastructure;

public static class Assertions
{
    public static void ShouldBeStatus(this Job job, JobStatus expected)
    {
        job.Should().NotBeNull();
        job!.State.Status.Should().Be(expected);
    }
}