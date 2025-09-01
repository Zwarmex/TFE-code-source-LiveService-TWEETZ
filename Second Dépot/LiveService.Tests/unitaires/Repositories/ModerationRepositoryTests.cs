using Xunit;
using Tweetz.MicroServices.LiveService.Repositories;
using Tweetz.MicroServices.LiveService.Data;
using Tweetz.MicroServices.LiveService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

public class ModerationRepositoryTests
{
    private ApplicationDBContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDBContext(options);
    }

    [Fact]
    public async Task GetBanUserAsync_ReturnsBannedUsers()
    {
        var context = CreateInMemoryContext(nameof(GetBanUserAsync_ReturnsBannedUsers));
        context.ModerationLogs.AddRange(
            new ModerationLog { Id = Guid.NewGuid(), ActionType = "Ban", StreamerId = 42 },
            new ModerationLog { Id = Guid.NewGuid(), ActionType = "Ban", StreamerId = 42 },
            new ModerationLog { Id = Guid.NewGuid(), ActionType = "Mute", StreamerId = 42 },
            new ModerationLog { Id = Guid.NewGuid(), ActionType = "Ban", StreamerId = 99 }
        );
        await context.SaveChangesAsync();

        var config = new Mock<IConfiguration>().Object;
        var logger = new Mock<ILogger<ModerationRepository>>().Object;
        var repo = new ModerationRepository(context, config, logger);

        var result = await repo.GetBanUserAsync(42);

        Assert.Equal(2, result.Count);
        Assert.All(result, log => Assert.Equal("Ban", log.ActionType));
        Assert.All(result, log => Assert.Equal(42, log.StreamerId));
    }

    [Fact]
    public async Task UnbanUserAsync_RemovesBanLog()
    {
        var context = CreateInMemoryContext(nameof(UnbanUserAsync_RemovesBanLog));
        var banLog = new ModerationLog { Id = Guid.NewGuid(), ActionType = "Ban", StreamerId = 42 };
        context.ModerationLogs.Add(banLog);
        await context.SaveChangesAsync();

        var config = new Mock<IConfiguration>().Object;
        var logger = new Mock<ILogger<ModerationRepository>>().Object;
        var repo = new ModerationRepository(context, config, logger);

        await repo.UnbanUserAsync(banLog.Id, 42);

        Assert.Empty(context.ModerationLogs.Where(l => l.Id == banLog.Id));
    }

    [Fact]
    public async Task UnbanUserAsync_DoesNothing_IfLogNotFound()
    {
        var context = CreateInMemoryContext(nameof(UnbanUserAsync_DoesNothing_IfLogNotFound));
        var config = new Mock<IConfiguration>().Object;
        var logger = new Mock<ILogger<ModerationRepository>>().Object;
        var repo = new ModerationRepository(context, config, logger);

        await repo.UnbanUserAsync(Guid.NewGuid(), 42);

        // No exception, nothing to assert (just ensure no crash)
        Assert.True(true);
    }
}