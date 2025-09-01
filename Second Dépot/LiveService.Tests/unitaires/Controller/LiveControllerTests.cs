namespace LiveService.Tests.Controller;

using Xunit;
using Moq;
using Tweetz.MicroServices.LiveService.Controllers;
using Tweetz.MicroServices.LiveService.Interfaces;
using Tweetz.MicroServices.LiveService.Repositories;
using Tweetz.MicroServices.LiveService.Models; // or .Entities, .Domain, etc.
using Tweetz.MicroServices.LiveService.Dtos.Live;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LiveControllerTests
{
    private readonly Mock<ILiveRepository> _mockRepo;
    private readonly LiveController _controller;

    public LiveControllerTests()
    {
        _mockRepo = new Mock<ILiveRepository>();
        _controller = new LiveController(_mockRepo.Object);

        // Setup fake user claims
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "testuser")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOk()
    {
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Live>());
        var result = await _controller.GetAllAsync();
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetAllOnStreamAsync_ReturnsOk()
    {
        _mockRepo.Setup(r => r.GetAllOnStreamAsync()).ReturnsAsync(new List<Live>());
        var result = await _controller.GetAllOnStreamAsync();
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetByStreamerIdAsync_ReturnsOk()
    {
        _mockRepo.Setup(r => r.GetByStreamerIdAsync(It.IsAny<int>())).ReturnsAsync(new List<Live>());
        var result = await _controller.GetByStreamerIdAsync(1);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFound_IfLiveNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync((Live)null);
        var result = await _controller.GetByIdAsync("streamId");
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOk_IfLiveExists()
    {
        var live = new Live();
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(live);
        var result = await _controller.GetByIdAsync("streamId");
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNoContent()
    {
        _mockRepo.Setup(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        var result = await _controller.DeleteAsync("streamId");
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsBadRequest_IfModelStateInvalid()
    {
        _controller.ModelState.AddModelError("error", "error");
        var result = await _controller.UpdateAsync("streamId", new LiveUpdateDto());
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFound_IfLiveNull()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync((Live)null);
        var result = await _controller.UpdateAsync("streamId", new LiveUpdateDto());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNoContent_IfLiveExists()
    {
        var live = new Live();
        _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(live);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<string>(), It.IsAny<LiveUpdateDto>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        var result = await _controller.UpdateAsync("streamId", new LiveUpdateDto());
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsBadRequest_IfModelStateInvalid()
    {
        _controller.ModelState.AddModelError("error", "error");
        var result = await _controller.CreateAsync(new LiveCreateDto());
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsOk_IfSuccess()
    {
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<LiveCreateDto>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync("newId");
        var dto = new LiveCreateDto { Title = "Test", IsPublic = true };
        var result = await _controller.CreateAsync(dto);
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UploadThumbnailAsync_ReturnsBadRequest_IfFileNull()
    {
        var result = await _controller.UploadThumbnailAsync("streamId", null);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Aucun fichier reçu.", badRequest.Value);
    }

    [Fact]
    public async Task UploadThumbnailAsync_ReturnsBadRequest_IfMimeTypeInvalid()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        var result = await _controller.UploadThumbnailAsync("streamId", fileMock.Object);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Format non supporté.", badRequest.Value);
    }

    [Fact]
    public async Task UploadThumbnailAsync_ReturnsNoContent_IfSuccess()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        fileMock.Setup(f => f.ContentType).Returns("image/png");
        _mockRepo.Setup(r => r.UploadThumbnailAsync(It.IsAny<string>(), It.IsAny<IFormFile>())).Returns(Task.CompletedTask);
        var result = await _controller.UploadThumbnailAsync("streamId", fileMock.Object);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteThumbnailAsync_ReturnsBadRequest_IfIdInvalid()
    {
        var result = await _controller.DeleteThumbnailAsync("");
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid live stream ID.", badRequest.Value);
    }

    [Fact]
    public async Task DeleteThumbnailAsync_ReturnsNoContent_IfSuccess()
    {
        _mockRepo.Setup(r => r.DeleteThumbnail(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        var result = await _controller.DeleteThumbnailAsync("streamId");
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CompleteLiveStreamAsync_ReturnsAccepted()
    {
        _mockRepo.Setup(r => r.CompleteLiveStreamAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        var result = await _controller.CompleteLiveStreamAsync("streamId");
        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task GetViewerCountAsync_ReturnsOk()
    {
        _mockRepo.Setup(r => r.GetViewerCountAsync(It.IsAny<string>())).ReturnsAsync(5);
        var result = await _controller.GetViewerCountAsync("streamId");
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(5, (int)okResult.Value.GetType().GetProperty("viewers")!.GetValue(okResult.Value, null));
    }
}
