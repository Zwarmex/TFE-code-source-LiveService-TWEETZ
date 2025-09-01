using Xunit;
using Moq;
using Tweetz.MicroServices.LiveService.Controllers;
using Tweetz.MicroServices.LiveService.Models;
using Tweetz.MicroServices.LiveService.Repositories;
using Tweetz.MicroServices.LiveService.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public class StatisticControllerTests
{
    private readonly Mock<IStatisticRepository> _mockRepo;
    private readonly Mock<ILogger<StatisticController>> _mockLogger;
    private readonly StatisticController _controller;

    public StatisticControllerTests()
    {
        _mockRepo = new Mock<IStatisticRepository>();
        _mockLogger = new Mock<ILogger<StatisticController>>();
        _controller = new StatisticController(_mockRepo.Object, _mockLogger.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    private List<LiveStatistic> GetSampleStats()
    {
        return new List<LiveStatistic>
        {
            new LiveStatistic
            {
                StartTime = DateTime.UtcNow.AddDays(-1),
                UniqueViewers = 10,
                TotalDuration = TimeSpan.FromMinutes(60)
            },
            new LiveStatistic
            {
                StartTime = DateTime.UtcNow.AddDays(-2),
                UniqueViewers = 5,
                TotalDuration = TimeSpan.FromMinutes(30)
            }
        };
    }

    [Theory]
    [InlineData("day")]
    [InlineData("yesterday")]
    [InlineData("7d")]
    [InlineData("month")]
    [InlineData("3m")]
    [InlineData("year")]
    public async Task GetStatistics_ValidRange_ReturnsOk(string range)
    {
        var stats = GetSampleStats();
        _mockRepo.Setup(r => r.GetStatisticsAsync(42, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(stats);

        var result = await _controller.GetStatistics(range);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetStatistics_InvalidRange_ReturnsBadRequest()
    {
        var result = await _controller.GetStatistics("invalid");
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid range. Use 'day', '7d', 'month', or 'year'.", badRequest.Value);
    }

    [Fact]
    public async Task GetStatistics_DefaultRange_ReturnsOk()
    {
        var stats = GetSampleStats();
        _mockRepo.Setup(r => r.GetStatisticsAsync(42, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(stats);

        var result = await _controller.GetStatistics();
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
}