using Xunit;
using Moq;
using Tweetz.MicroServices.LiveService.Controllers;
using Tweetz.MicroServices.LiveService.Repositories;
using Tweetz.MicroServices.LiveService.Interfaces;
using Tweetz.MicroServices.LiveService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public class ModerationControllerTests
{
    private readonly Mock<IModerationRepository> _mockRepo;
    private readonly ModerationController _controller;

    public ModerationControllerTests()
    {
        _mockRepo = new Mock<IModerationRepository>();
        _controller = new ModerationController(_mockRepo.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task BanUser_ReturnsListOfModerationLog()
    {
        var logs = new List<ModerationLog> { new ModerationLog(), new ModerationLog() };
        _mockRepo.Setup(r => r.GetBanUserAsync(42)).ReturnsAsync(logs);

        var result = await _controller.BanUser();

        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.IsType<ModerationLog>(item));
    }

    [Fact]
    public async Task UnbanUser_ReturnsNoContent()
    {
        var logId = Guid.NewGuid();
        _mockRepo.Setup(r => r.UnbanUserAsync(logId, 42)).Returns(Task.CompletedTask);

        var result = await _controller.UnbanUser(logId);

        Assert.IsType<NoContentResult>(result);
    }
}