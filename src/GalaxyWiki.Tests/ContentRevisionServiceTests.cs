using Xunit;
using Moq;
using GalaxyWiki.API.Services;
using GalaxyWiki.API.Repositories;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;
using GalaxyWiki.API.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyWiki.Tests
{
    public class ContentRevisionServiceTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IContentRevisionRepository> _mockContentRevisionRepository;
        private readonly Mock<ICelestialBodyRepository> _mockCelestialBodyRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly ContentRevisionService _service;

        public ContentRevisionServiceTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockContentRevisionRepository = new Mock<IContentRevisionRepository>();
            _mockCelestialBodyRepository = new Mock<ICelestialBodyRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _service = new ContentRevisionService(
                _mockAuthService.Object,
                _mockContentRevisionRepository.Object,
                _mockCelestialBodyRepository.Object,
                _mockUserRepository.Object
            );
        }

        [Fact]
        public async Task GetRevisionByIdAsync_ReturnsRevision()
        {
            var revision = new ContentRevisions { Id = 1, Content = "Test" };
            _mockContentRevisionRepository.Setup(r => r.GetById(1)).ReturnsAsync(revision);

            var result = await _service.GetRevisionByIdAsync(1);

            Assert.Equal(revision, result);
        }

        [Fact]
        public async Task GetRevisionsByCelestialBodyAsync_ExistingBody_ReturnsRevisions()
        {
            var cb = new CelestialBodies { Id = 1, BodyName = "Earth", BodyType = 1 };
            var revisions = new List<ContentRevisions> { new ContentRevisions { Id = 1, Content = "Test" } };
            _mockCelestialBodyRepository.Setup(r => r.GetByName("Earth")).ReturnsAsync(cb);
            _mockContentRevisionRepository.Setup(r => r.GetByCelestialBodyId(1)).ReturnsAsync(revisions);

            var result = await _service.GetRevisionsByCelestialBodyAsync("Earth");

            Assert.Single(result);
        }

        [Fact]
        public async Task GetRevisionsByCelestialBodyAsync_NonExistingBody_Throws()
        {
            _mockCelestialBodyRepository.Setup(r => r.GetByName("Earth")).ReturnsAsync((CelestialBodies)null);

            await Assert.ThrowsAsync<CelestialBodyDoesNotExist>(() => _service.GetRevisionsByCelestialBodyAsync("Earth"));
        }

       /* [Fact]
        public async Task CreateRevision_ValidRequest_CreatesRevision()
        {
            var userId = "user1";
            var request = new CreateRevisionRequest { CelestialBodyPath = "Earth", Content = "Test" };
            var user = new Users { Id = userId };
            var cb = new CelestialBodies { Id = 1, BodyName = "Earth", BodyType = 1 };
            var revision = new ContentRevisions { Id = 1, Content = "Test", CelestialBody = cb, Author = user };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(user);
            _mockCelestialBodyRepository.Setup(r => r.GetByName("Earth")).ReturnsAsync(cb);
            _mockContentRevisionRepository.Setup(r => r.Create(It.IsAny<ContentRevisions>())).ReturnsAsync(revision);

            var result = await _service.CreateRevision(request, userId);

            Assert.Equal(revision, result);
        }*/

        [Fact]
        public async Task CreateRevision_InvalidAccess_Throws()
        {
            var userId = "user1";
            var request = new CreateRevisionRequest { CelestialBodyPath = "Earth", Content = "Test" };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(false);

            await Assert.ThrowsAsync<UserDoesNotHaveAccess>(() => _service.CreateRevision(request, userId));
        }

        [Fact]
        public async Task CreateRevision_NonExistingUser_Throws()
        {
            var userId = "user1";
            var request = new CreateRevisionRequest { CelestialBodyPath = "Earth", Content = "Test" };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync((Users)null);

            await Assert.ThrowsAsync<UserDoesNotExist>(() => _service.CreateRevision(request, userId));
        }

        [Fact]
        public async Task CreateRevision_NonExistingCelestialBody_Throws()
        {
            var userId = "user1";
            var request = new CreateRevisionRequest { CelestialBodyPath = "Earth", Content = "Test" };
            var user = new Users { Id = userId };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockUserRepository.Setup(r => r.GetById(userId)).ReturnsAsync(user);
            _mockCelestialBodyRepository.Setup(r => r.GetByName("Earth")).ReturnsAsync((CelestialBodies)null);

            await Assert.ThrowsAsync<CelestialBodyDoesNotExist>(() => _service.CreateRevision(request, userId));
        }
    }
} 