using Xunit;
using Moq;
using GalaxyWiki.API.Controllers;
using GalaxyWiki.API.Services;
using GalaxyWiki.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using GalaxyWiki.API.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class RevisionsControllerTests 
{
    private readonly Mock<ContentRevisionService> _mockService;
    private readonly RevisionsController _controller;

    public RevisionsControllerTests()
    {
        _mockService = new Mock<ContentRevisionService>();
        _controller = new RevisionsController(_mockService.Object);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenRevisionExists()
    {
        var revision = new ContentRevisions
        {
            Id = 1,
            Content = "Sample content",
            CreatedAt = DateTime.UtcNow,
            CelestialBody = new CelestialBodies { BodyName = "Mars", BodyType = 3 },
            Author = new Users { DisplayName = "Jane Doe"}
        };

        _mockService.Setup(s => s.GetRevisionByIdAsync(1)).ReturnsAsync(revision);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        dynamic body = okResult.Value!;
        Assert.Equal(1, (int)body.Id);
        Assert.Equal("Sample content", (string)body.Content);
        Assert.Equal("Mars", (string)body.CelestialBodyName);
        Assert.Equal("Jane Doe", (string)body.AuthorDisplayName);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenRevisionDoesNotExist()
    {
        _mockService.Setup(s => s.GetRevisionByIdAsync(99)).ReturnsAsync((ContentRevisions?)null);

        var result = await _controller.GetById(99);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        var body = notFoundResult.Value as dynamic;
        Assert.NotNull(body);
        Assert.Equal("Revision not found.", (string)body.error);
    }

    [Fact]
    public async Task GetByCelestialBody_ReturnsOk_WhenRevisionsExist()
    {
        var revisions = new List<ContentRevisions>
        {
            new ContentRevisions
            {
                Id = 1,
                Content = "Discovery of water",
                CreatedAt = DateTime.UtcNow,
                CelestialBody = new CelestialBodies { BodyName = "Mars", BodyType = 3 },
                Author = new Users { DisplayName = "Dr. Red" }
            },
            new ContentRevisions
            {
                Id = 2,
                Content = "Ice caps confirmed",
                CreatedAt = DateTime.UtcNow,
                CelestialBody = new CelestialBodies { BodyName = "Mars", BodyType = 3 },
                Author = new Users { DisplayName = "Prof. Frost" }
            }
        };

        _mockService.Setup(s => s.GetRevisionsByCelestialBodyAsync("mars"))
                    .ReturnsAsync(revisions);

        var result = await _controller.GetByCelestialBody("mars");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseList = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value!);

        var responseArray = responseList.ToList();
        Assert.Equal(2, responseArray.Count);
        Assert.Equal("Discovery of water", (string)responseArray[0].Content);
        Assert.Equal("Dr. Red", (string)responseArray[0].AuthorDisplayName);
        Assert.Equal("Ice caps confirmed", (string)responseArray[1].Content);
        Assert.Equal("Prof. Frost", (string)responseArray[1].AuthorDisplayName);
    }

    [Fact]
    public async Task GetByCelestialBody_ReturnsNotFound_WhenNoRevisionsExist()
    {
        _mockService.Setup(s => s.GetRevisionsByCelestialBodyAsync("pluto"))
                    .ReturnsAsync(new List<ContentRevisions>());

        var result = await _controller.GetByCelestialBody("pluto");

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var body = notFound.Value as dynamic;
        Assert.NotNull(body);
        Assert.Equal("No revisions found for the specified celestial body.", (string)body.error);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenValidRequest()
    {
        var request = new CreateRevisionRequest
        {
            CelestialBodyPath = "mars",
            Content = "Mars has dust storms."
        };

        var mockAuthorId = "user-123";

        var createdRevision = new ContentRevisions
        {
            Id = 1,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        _mockService.Setup(s => s.CreateRevision(request, mockAuthorId))
                    .ReturnsAsync(createdRevision);

        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, mockAuthorId)
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var result = await _controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        dynamic response = createdResult.Value!;

        Assert.Equal(1, (int)response.Id);
        Assert.Equal(request.Content, (string)response.Content);
        Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
    }

}