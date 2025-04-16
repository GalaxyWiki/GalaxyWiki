using Xunit;
using Moq;
using GalaxyWiki.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using GalaxyWiki.API.Controllers;
using GalaxyWiki.API.DTOs;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class RevisionsControllerTests
{
    private readonly Mock<IContentRevisionService> _mockContentRevisionService;

    public RevisionsControllerTests()
    {
        _mockContentRevisionService = new Mock<IContentRevisionService>();
    }

    private RevisionsController SetupControllerWithUser()
    {
        var controller = new RevisionsController(_mockContentRevisionService.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-sub")
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenRevisionExists()
    {
        _mockContentRevisionService.Setup(service => service.GetRevisionByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new ContentRevisions
            {
                Id = 1,
                Content = "Test Content",
                CreatedAt = DateTime.UtcNow,
                CelestialBody = new CelestialBodies { BodyName = "Earth", BodyType = 161 },
                Author = new Users { DisplayName = "Test User" }
            });

        var controller = new RevisionsController(_mockContentRevisionService.Object);

        var result = await controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<ContentRevisionDto>(okResult.Value);

        Assert.Equal(1, returnValue.Id);
        Assert.Equal("Test Content", returnValue.Content);
        Assert.Equal("Earth", returnValue.CelestialBodyName);
        Assert.Equal("Test User", returnValue.AuthorDisplayName);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenRevisionDoesNotExist()
    {
        _mockContentRevisionService.Setup(service => service.GetRevisionByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((ContentRevisions)null);

        var controller = new RevisionsController(_mockContentRevisionService.Object);

        var result = await controller.GetById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetByCelestialBody_ReturnsOk_WhenRevisionsExist()
    {
        _mockContentRevisionService.Setup(service => service.GetRevisionsByCelestialBodyAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<ContentRevisions>
            {
                new ContentRevisions
                {
                    Id = 1,
                    Content = "Test Content",
                    CreatedAt = DateTime.UtcNow,
                    CelestialBody = new CelestialBodies { BodyName = "Earth", BodyType = 161 },
                    Author = new Users { DisplayName = "Test User" }
                }
            });

        var controller = new RevisionsController(_mockContentRevisionService.Object);

        var result = await controller.GetByCelestialBody("Earth");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<ContentRevisionDto>>(okResult.Value);

        var firstRevision = returnValue.First();

        Assert.Equal(1, firstRevision.Id);
        Assert.Equal("Test Content", firstRevision.Content);
        Assert.Equal("Earth", firstRevision.CelestialBodyName);
        Assert.Equal("Test User", firstRevision.AuthorDisplayName);
    }

    [Fact]
    public async Task GetByCelestialBody_ReturnsNotFound_WhenNoRevisionsExist()
    {
        _mockContentRevisionService.Setup(service => service.GetRevisionsByCelestialBodyAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<ContentRevisions>());

        var controller = new RevisionsController(_mockContentRevisionService.Object);

        var result = await controller.GetByCelestialBody("Earth");

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var returnValue = Assert.IsType<ContentRevisionDto>(notFoundResult.Value);
        Assert.Equal(0, returnValue.Id);
        Assert.Null(returnValue.Content);
        Assert.Null(returnValue.CelestialBodyName);
        Assert.Null(returnValue.AuthorDisplayName);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenRevisionIsCreated()
    {
        var request = new CreateRevisionRequest
        {
            CelestialBodyPath = "andromeda/planet-x",
            Content = "New Content"
        };

        var revision = new ContentRevisions
        {
            Id = 1,
            Content = "New Content",
            CreatedAt = DateTime.UtcNow,
            CelestialBody = new CelestialBodies { BodyName = "Planet X", BodyType = 10 },
            Author = new Users { DisplayName = "Jane Doe" }
        };

        _mockContentRevisionService
            .Setup(s => s.CreateRevision(request, "user-sub"))
            .ReturnsAsync(revision);

        var controller = SetupControllerWithUser();

        var result = await controller.Create(request);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        var dto = Assert.IsType<ContentRevisionDto>(createdAtActionResult.Value);

        Assert.Equal(revision.Id, dto.Id);
        Assert.Equal(revision.Content, dto.Content);
        Assert.Equal(revision.CelestialBody.BodyName, dto.CelestialBodyName);
        Assert.Equal(revision.Author.DisplayName, dto.AuthorDisplayName);
    }

}
