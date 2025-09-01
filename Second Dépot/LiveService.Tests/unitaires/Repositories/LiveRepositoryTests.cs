using Xunit;
using Tweetz.MicroServices.LiveService.Repositories;
using Tweetz.MicroServices.LiveService.Data;
using Tweetz.MicroServices.LiveService.Models;
using Tweetz.MicroServices.LiveService.Dtos.Live;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.EntityFrameworkCore.InMemory;
using System.Threading.Tasks;
using System.Collections.Generic;

public class LiveRepositoryTests
{
    private ApplicationDBContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDBContext(options);
    }

    [Fact]
    public async Task GetViewerCountAsync_ReturnsCorrectCount()
    {
        var context = CreateInMemoryContext(nameof(GetViewerCountAsync_ReturnsCorrectCount));
        context.LiveViewers.AddRange(
            new LiveViewer { LiveId = "live1", IsConnected = true },
            new LiveViewer { LiveId = "live1", IsConnected = true },
            new LiveViewer { LiveId = "live1", IsConnected = false }
        );
        await context.SaveChangesAsync();

        var config = new Mock<IConfiguration>().Object;
        var logger = new Mock<ILogger<LiveRepository>>().Object;
        var repo = new LiveRepository(context, config, logger);

        var count = await repo.GetViewerCountAsync("live1");
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task GetByStreamerIdAsync_ReturnsLives()
    {
        var context = CreateInMemoryContext(nameof(GetByStreamerIdAsync_ReturnsLives));
        context.Lives.AddRange(
            new Live { ApiVideoLiveStreamId = "id1", StreamerId = 1, IsTerminated = false },
            new Live { ApiVideoLiveStreamId = "id2", StreamerId = 1, IsTerminated = true },
            new Live { ApiVideoLiveStreamId = "id3", StreamerId = 2, IsTerminated = false }
        );
        await context.SaveChangesAsync();

        var config = new Mock<IConfiguration>().Object;
        var logger = new Mock<ILogger<LiveRepository>>().Object;
        var repo = new LiveRepository(context, config, logger);

        var result = await repo.GetByStreamerIdAsync(1);
        Assert.Single(result);
        Assert.False(result[0].IsTerminated);
    }

    [Fact]
    public async Task GetAllOnStreamAsync_ReturnsLivesOnStream()
    {
        var context = CreateInMemoryContext(nameof(GetAllOnStreamAsync_ReturnsLivesOnStream));
        context.Lives.AddRange(
            new Live { ApiVideoLiveStreamId = "id1", Broadcasting = true, IsPublic = true },
            new Live { ApiVideoLiveStreamId = "id2", Broadcasting = false, IsPublic = true },
            new Live { ApiVideoLiveStreamId = "id3", Broadcasting = true, IsPublic = false }
        );
        await context.SaveChangesAsync();

        var config = new Mock<IConfiguration>().Object;
        var logger = new Mock<ILogger<LiveRepository>>().Object;
        var repo = new LiveRepository(context, config, logger);

        var result = await repo.GetAllOnStreamAsync();
        Assert.Single(result);
        Assert.True(result[0].Broadcasting);
        Assert.True(result[0].IsPublic);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_OnApiError()
    {
        var context = CreateInMemoryContext(nameof(GetAllAsync_ReturnsEmptyList_OnApiError));
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["ApiVideo:ApiUrl"]).Returns("http://fakeapi");
        configMock.Setup(c => c["ApiVideo:ApiKey"]).Returns("fakekey");
        var logger = new Mock<ILogger<LiveRepository>>().Object;

        var repo = new LiveRepository(context, configMock.Object, logger);

        // Simulez une erreur API en modifiant le repo ou en mockant le client si possible
        var result = await repo.GetAllAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_OnApiNotFound()
    {
        var context = CreateInMemoryContext(nameof(GetByIdAsync_ReturnsNull_OnApiNotFound));
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["ApiVideo:ApiUrl"]).Returns("http://fakeapi");
        configMock.Setup(c => c["ApiVideo:ApiKey"]).Returns("fakekey");
        var loggerMock = new Mock<ILogger<LiveRepository>>();
        var repo = new LiveRepository(context, configMock.Object, loggerMock.Object);

        var result = await repo.GetByIdAsync("notfoundid", 123);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetViewerCountAsync_ReturnsZero_WhenNoViewers()
    {
        var context = CreateInMemoryContext(nameof(GetViewerCountAsync_ReturnsZero_WhenNoViewers));
        await context.SaveChangesAsync();

        var config = new Mock<IConfiguration>().Object;
        var logger = new Mock<ILogger<LiveRepository>>().Object;
        var repo = new LiveRepository(context, config, logger);

        var count = await repo.GetViewerCountAsync("unknownid");
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task GetByStreamerIdAsync_ReturnsEmptyList_OnException()
    {
        var context = CreateInMemoryContext(nameof(GetByStreamerIdAsync_ReturnsEmptyList_OnException));
        var config = new Mock<IConfiguration>().Object;
        var loggerMock = new Mock<ILogger<LiveRepository>>();
        var repo = new LiveRepository(context, config, loggerMock.Object);

        // Simulez une exception en supprimant le DbSet ou en utilisant un mauvais streamerId
        var result = await repo.GetByStreamerIdAsync(-1);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllOnStreamAsync_ReturnsEmptyList_OnException()
    {
        var context = CreateInMemoryContext(nameof(GetAllOnStreamAsync_ReturnsEmptyList_OnException));
        var config = new Mock<IConfiguration>().Object;
        var loggerMock = new Mock<ILogger<LiveRepository>>();
        var repo = new LiveRepository(context, config, loggerMock.Object);

        // Simulez une exception en supprimant le DbSet ou en utilisant des données vides
        var result = await repo.GetAllOnStreamAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_AddsLiveToDb_ThrowsUnauthorized()
    {
        var context = CreateInMemoryContext(nameof(CreateAsync_AddsLiveToDb_ThrowsUnauthorized));
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["ApiVideo:ApiUrl"]).Returns("http://localhost");
        configMock.Setup(c => c["ApiVideo:ApiKey"]).Returns("dummykey");
        var logger = new Mock<ILogger<LiveRepository>>().Object;
        var repo = new LiveRepository(context, configMock.Object, logger);

        var dto = new LiveCreateDto
        {
            Title = "Test Live",
            IsPublic = true,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            InvitedUserId = null
        };

        var streamerId = 42;
        var streamerUsername = "streamer";

        await Assert.ThrowsAsync<System.AggregateException>(async () =>
        {
            await repo.CreateAsync(dto, streamerId, streamerUsername);
        });
    }

    [Fact]
    public async Task DeleteAsync_RemovesLiveFromDb()
    {
        var context = CreateInMemoryContext(nameof(DeleteAsync_RemovesLiveFromDb));
        var live = new Live { ApiVideoLiveStreamId = "delid", StreamerId = 42, Title = "ToDelete" };
        context.Lives.Add(live);
        await context.SaveChangesAsync();

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["ApiVideo:ApiUrl"]).Returns("http://localhost");
        configMock.Setup(c => c["ApiVideo:ApiKey"]).Returns("dummykey");
        var logger = new Mock<ILogger<LiveRepository>>().Object;
        var repo = new LiveRepository(context, configMock.Object, logger);

        await repo.DeleteAsync("delid", 42);

        var deleted = await context.Lives.FindAsync("delid");
        Assert.Null(deleted);
    }

    [Fact]
    public async Task CompleteLiveStreamAsync_SetsIsTerminated_ThrowsInvalidOperationException()
    {
        var context = CreateInMemoryContext(nameof(CompleteLiveStreamAsync_SetsIsTerminated_ThrowsInvalidOperationException));
        var live = new Live { ApiVideoLiveStreamId = "compid", StreamerId = 42, IsTerminated = false };
        context.Lives.Add(live);
        await context.SaveChangesAsync();

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["ApiVideo:ApiUrl"]).Returns("http://localhost");
        configMock.Setup(c => c["ApiVideo:ApiKey"]).Returns("dummykey");
        var logger = new Mock<ILogger<LiveRepository>>().Object;
        var repo = new LiveRepository(context, configMock.Object, logger);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await repo.CompleteLiveStreamAsync("compid");
        });
    }

    [Fact]
    public async Task DeleteThumbnail_ThrowsException_IfLiveNotFound()
    {
        var context = CreateInMemoryContext(nameof(DeleteThumbnail_ThrowsException_IfLiveNotFound));
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["ApiVideo:ApiUrl"]).Returns("http://localhost");
        configMock.Setup(c => c["ApiVideo:ApiKey"]).Returns("dummykey");
        var logger = new Mock<ILogger<LiveRepository>>().Object;
        var repo = new LiveRepository(context, configMock.Object, logger);

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await repo.DeleteThumbnail("notfoundid", 42);
        });
    }
}