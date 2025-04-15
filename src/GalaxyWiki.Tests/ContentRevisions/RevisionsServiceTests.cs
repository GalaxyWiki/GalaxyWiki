using Xunit;
using Moq;
using GalaxyWiki.API.Services;
using GalaxyWiki.API.DTOs;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;
using GalaxyWiki.Api.Repositories;

public class ContentRevisionServiceTests
{
    private readonly Mock<AuthService> _mockAuthService = new();
    private readonly Mock<ContentRevisionRepository> _mockContentRevisionRepo = new();
    private readonly Mock<CelestialBodyRepository> _mockCelestialBodyRepo = new();
    private readonly Mock<UserRepository> _mockUserRepo = new();
    private readonly ContentRevisionService _service;

    public ContentRevisionServiceTests()
    {
        _service = new ContentRevisionService(
            _mockAuthService.Object,
            _mockContentRevisionRepo.Object,
            _mockCelestialBodyRepo.Object,
            _mockUserRepo.Object
        );
    }

    [Fact]
    public async Task GetRevisionByIdAsync_ReturnsRevision_WhenExists()
    {
        var revision = new ContentRevisions { Id = 42, Content = "The red planet" };
        _mockContentRevisionRepo.Setup(r => r.GetById(42)).ReturnsAsync(revision);

        var result = await _service.GetRevisionByIdAsync(42);

        Assert.Equal(42, result!.Id);
        Assert.Equal("The red planet", result.Content);
    }

    [Fact]
    public async Task GetRevisionsByCelestialBodyAsync_ReturnsRevisions_WhenCelestialBodyExists()
    {
        var celestialBody = new CelestialBodies { Id = 1, BodyName = "Mars", BodyType = 3 };
        var revisions = new List<ContentRevisions>
        {
            new ContentRevisions { Id = 1, Content = "First revision", CelestialBody = celestialBody },
            new ContentRevisions { Id = 2, Content = "Second revision", CelestialBody = celestialBody }
        };

        _mockCelestialBodyRepo.Setup(r => r.GetByName("mars")).ReturnsAsync(celestialBody);
        _mockContentRevisionRepo.Setup(r => r.GetByCelestialBodyId(1)).ReturnsAsync(revisions);

        var result = await _service.GetRevisionsByCelestialBodyAsync("mars");

        Assert.Equal(2, result.Count());
        Assert.Contains(result, r => r.Content == "First revision");
        Assert.Contains(result, r => r.Content == "Second revision");
    }

    [Fact]
    public async Task GetRevisionsByCelestialBodyAsync_Throws_WhenCelestialBodyDoesNotExist()
    {
        _mockCelestialBodyRepo.Setup(r => r.GetByName("unknown")).ReturnsAsync((CelestialBodies?)null);

        var ex = await Assert.ThrowsAsync<CelestialBodyDoesNotExist>(() =>
            _service.GetRevisionsByCelestialBodyAsync("unknown"));

        Assert.Equal("Celestial body not found.", ex.Message);
    }

    [Fact]
    public async Task CreateRevision_ReturnsRevision_WhenValid()
    {
        var authorId = "user-123";
        var request = new CreateRevisionRequest
        {
            CelestialBodyPath = "mars",
            Content = "There are dust storms on Mars."
        };

        var author = new Users { Id = authorId, DisplayName = "Jane" };
        var celestialBody = new CelestialBodies { Id = 1, BodyName = "Mars", BodyType = 3 };

        _mockAuthService.Setup(s => s.CheckUserHasAccessRight((UserRole[])It.IsAny<IEnumerable<UserRole>>(), authorId))
                        .ReturnsAsync(true);
        _mockUserRepo.Setup(r => r.GetById(authorId))
                    .ReturnsAsync(author);
        _mockCelestialBodyRepo.Setup(r => r.GetByName("mars"))
                            .ReturnsAsync(celestialBody);
        _mockContentRevisionRepo.Setup(r => r.Create(It.IsAny<ContentRevisions>()))
                                .Returns((Task<ContentRevisions>)Task.CompletedTask);

        var result = await _service.CreateRevision(request, authorId);

        Assert.Equal(request.Content, result.Content);
        Assert.Equal(author, result.Author);
        Assert.Equal(celestialBody, result.CelestialBody);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
    }
    
    [Fact]
    public async Task CreateRevision_Throws_WhenUserHasNoAccess()
    {
        var authorId = "user-123";
        var request = new CreateRevisionRequest
        {
            CelestialBodyPath = "mars",
            Content = "Unauthorized revision"
        };
        _mockAuthService.Setup(s => s.CheckUserHasAccessRight((UserRole[])It.IsAny<IEnumerable<UserRole>>(), authorId))
                        .ReturnsAsync(false);

        var ex = await Assert.ThrowsAsync<UserDoesNotHaveAccess>(() => _service.CreateRevision(request, authorId));
        Assert.Equal("You do not have access to perform this action.", ex.Message);
    }

    [Fact]
    public async Task CreateRevision_Throws_WhenUserNotFound()
    {
        var authorId = "ghost-user";
        var request = new CreateRevisionRequest
        {
            CelestialBodyPath = "mars",
            Content = "Ghost revision"
        };

        _mockAuthService.Setup(s => s.CheckUserHasAccessRight((UserRole[])It.IsAny<IEnumerable<UserRole>>(), authorId))
                        .ReturnsAsync(true);
        _mockUserRepo.Setup(r => r.GetById(authorId))
                    .ReturnsAsync((Users?)null);

        var ex = await Assert.ThrowsAsync<UserDoesNotExist>(() => _service.CreateRevision(request, authorId));
        Assert.Equal("User does not exist.", ex.Message);
    }

    [Fact]
    public async Task CreateRevision_Throws_WhenCelestialBodyNotFound()
    {
        var authorId = "user-123";
        var request = new CreateRevisionRequest
        {
            CelestialBodyPath = "unknown-planet",
            Content = "This body doesn't exist"
        };

        var author = new Users { Id = authorId, DisplayName = "Jane" };

        _mockAuthService.Setup(s => s.CheckUserHasAccessRight((UserRole[])It.IsAny<IEnumerable<UserRole>>(), authorId))
                        .ReturnsAsync(true);
        _mockUserRepo.Setup(r => r.GetById(authorId))
                    .ReturnsAsync(author);
        _mockCelestialBodyRepo.Setup(r => r.GetByName(request.CelestialBodyPath))
                            .ReturnsAsync((CelestialBodies?)null);

        var ex = await Assert.ThrowsAsync<CelestialBodyDoesNotExist>(() => _service.CreateRevision(request, authorId));
        Assert.Equal("Celestial body not found.", ex.Message);
    }
}
