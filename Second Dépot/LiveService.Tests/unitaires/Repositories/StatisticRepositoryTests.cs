using Xunit;
using Tweetz.MicroServices.LiveService.Repositories;
using Tweetz.MicroServices.LiveService.Data;
using Tweetz.MicroServices.LiveService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

public class StatisticRepositoryTests
{
    private ApplicationDBContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDBContext(options);
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectStats()
    {
        var context = CreateInMemoryContext(nameof(GetStatisticsAsync_ReturnsCorrectStats));
        context.LiveStatistics.AddRange(
            new LiveStatistic { Id = Guid.NewGuid(), UserId = 42, StartTime = DateTime.UtcNow.AddDays(-2), EndTime = DateTime.UtcNow.AddDays(-2).AddHours(1) },
            new LiveStatistic { Id = Guid.NewGuid(), UserId = 42, StartTime = DateTime.UtcNow.AddDays(-1), EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1) },
            new LiveStatistic { Id = Guid.NewGuid(), UserId = 99, StartTime = DateTime.UtcNow.AddDays(-1), EndTime = DateTime.UtcNow.AddDays(-1).AddHours(1) }
        );
        await context.SaveChangesAsync();

        var logger = new Mock<ILogger<StatisticRepository>>().Object;
        var repo = new StatisticRepository(context, logger);

        var start = DateTime.UtcNow.AddDays(-3);
        var end = DateTime.UtcNow;
        var result = await repo.GetStatisticsAsync(42, start, end);

        Assert.Equal(2, result.Count);
        Assert.All(result, stat => Assert.Equal(42, stat.UserId));
        Assert.True(result[0].StartTime <= result[1].StartTime);
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsEmpty_WhenNoStats()
    {
        var context = CreateInMemoryContext(nameof(GetStatisticsAsync_ReturnsEmpty_WhenNoStats));
        await context.SaveChangesAsync();

        var logger = new Mock<ILogger<StatisticRepository>>().Object;
        var repo = new StatisticRepository(context, logger);

        var start = DateTime.UtcNow.AddDays(-3);
        var end = DateTime.UtcNow;
        var result = await repo.GetStatisticsAsync(42, start, end);

        Assert.Empty(result);
    }

}