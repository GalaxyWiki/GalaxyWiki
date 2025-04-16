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
    public class StarSystemServiceTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IStarSystemRepository> _mockStarSystemRepository;
        private readonly Mock<ICelestialBodyRepository> _mockCelestialBodyRepository;
        private readonly StarSystemService _service;

        public StarSystemServiceTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockStarSystemRepository = new Mock<IStarSystemRepository>();
            _mockCelestialBodyRepository = new Mock<ICelestialBodyRepository>();
            _service = new StarSystemService(
                _mockAuthService.Object,
                _mockStarSystemRepository.Object,
                _mockCelestialBodyRepository.Object
            );
        }

        [Fact]
        public async Task GetAll_ReturnsStarSystems()
        {
            var systems = new List<StarSystems> { new StarSystems { Id = 1, Name = "Alpha",CenterCb = new CelestialBodies { Id = 2, BodyName = "Earth", BodyType = 1 } } };
            _mockStarSystemRepository.Setup(r => r.GetAll()).ReturnsAsync(systems);

            var result = await _service.getAll();

            Assert.Single(result);
            Assert.Equal(systems[0], Assert.Single(result));
        }

        [Fact]
        public async Task GetStarSystemById_Existing_ReturnsSystem()
        {
            var system = new StarSystems { Id = 1, Name = "Alpha",CenterCb = new CelestialBodies { Id = 2, BodyName = "Earth", BodyType = 1 } };
            _mockStarSystemRepository.Setup(r => r.GetById(1)).ReturnsAsync(system);

            var result = await _service.GetStarSystemById(1);

            Assert.Equal(system, result);
        }

        [Fact]
        public async Task GetCelestialBodiesForStarSystemById_Existing_ReturnsBodies()
        {
            var cb = new CelestialBodies { Id = 2, BodyName = "Earth", BodyType = 1 };
            var system = new StarSystems { Id = 1, Name = "Alpha", CenterCb = cb };
            var orbiting = new List<CelestialBodies> { new CelestialBodies { Id = 3, BodyName = "Mars", BodyType = 1 } };
            _mockStarSystemRepository.Setup(r => r.GetById(1)).ReturnsAsync(system);
            _mockCelestialBodyRepository.Setup(r => r.GetCelestialBodiesOrbitingThisId(cb.Id)).ReturnsAsync(orbiting);

            var result = await _service.GetCelestialBodiesForStarSystemById(1);

            Assert.Single(result);
            Assert.Equal(orbiting[0], Assert.Single(result));
        }

        [Fact]
        public async Task GetCelestialBodiesForStarSystemById_NonExisting_Throws()
        {
            _mockStarSystemRepository.Setup(r => r.GetById(1)).ReturnsAsync((StarSystems)null);

            await Assert.ThrowsAsync<StarSystemDoesNotExist>(() => _service.GetCelestialBodiesForStarSystemById(1));
        }

       /* [Fact]
        public async Task CreateStarSystem_ValidRequest_CreatesSystem()
        {
            var userId = "user1";
            var request = new CreateStarSystemRequest { Name = "Alpha", CenterCbId = 2 };
            var cb = new CelestialBodies { Id = 2, BodyName = "Earth", BodyType = 1 };
            var system = new StarSystems { Id = 1, Name = "Alpha", CenterCb = cb };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockCelestialBodyRepository.Setup(r => r.GetById(2)).ReturnsAsync(cb);
            _mockStarSystemRepository.Setup(r => r.Create(It.IsAny<StarSystems>())).ReturnsAsync(system);

            var result = await _service.CreateStarSystem(request, userId);

            Assert.Equal(system, result);
        }*/

        [Fact]
        public async Task CreateStarSystem_InvalidAccess_Throws()
        {
            var userId = "user1";
            var request = new CreateStarSystemRequest { Name = "Alpha", CenterCbId = 2 };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(false);

            await Assert.ThrowsAsync<UserDoesNotHaveAccess>(() => _service.CreateStarSystem(request, userId));
        }

        [Fact]
        public async Task CreateStarSystem_NonExistingCenterCb_Throws()
        {
            var userId = "user1";
            var request = new CreateStarSystemRequest { Name = "Alpha", CenterCbId = 2 };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockCelestialBodyRepository.Setup(r => r.GetById(2)).ReturnsAsync((CelestialBodies)null);

            await Assert.ThrowsAsync<CelestialBodyDoesNotExist>(() => _service.CreateStarSystem(request, userId));
        }

        [Fact]
        public async Task UpdateStarSystem_ValidRequest_UpdatesSystem()
        {
            var userId = "user1";
            var cb = new CelestialBodies { Id = 2, BodyName = "Earth", BodyType = 1 };
            var system = new StarSystems { Id = 1, Name = "Alpha", CenterCb = cb };
            var request = new UpdateStarSystemRequest { Name = "Beta", CenterCbId = 2 };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockStarSystemRepository.Setup(r => r.GetById(1)).ReturnsAsync(system);
            _mockCelestialBodyRepository.Setup(r => r.GetById(2)).ReturnsAsync(cb);
            _mockStarSystemRepository.Setup(r => r.Update(It.IsAny<StarSystems>())).ReturnsAsync(system);

            var result = await _service.UpdateStarSystem(1, request, userId);

            Assert.Equal(system, result);
        }

        [Fact]
        public async Task UpdateStarSystem_NonExisting_Throws()
        {
            var userId = "user1";
            var request = new UpdateStarSystemRequest { Name = "Beta", CenterCbId = 2 };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockStarSystemRepository.Setup(r => r.GetById(1)).ReturnsAsync((StarSystems)null);

            await Assert.ThrowsAsync<StarSystemDoesNotExist>(() => _service.UpdateStarSystem(1, request, userId));
        }

        [Fact]
        public async Task DeleteStarSystem_ValidRequest_DeletesSystem()
        {
            var userId = "user1";
            var system = new StarSystems { Id = 1, Name = "Alpha", CenterCb = new CelestialBodies { Id = 2, BodyName = "Earth", BodyType = 1 } };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockStarSystemRepository.Setup(r => r.GetById(1)).ReturnsAsync(system);
            _mockStarSystemRepository.Setup(r => r.Delete(system)).Returns(Task.CompletedTask);

            await _service.DeleteStarSystem(1, userId);
            _mockStarSystemRepository.Verify(r => r.Delete(system), Times.Once);
        }

        [Fact]
        public async Task DeleteStarSystem_NonExisting_Throws()
        {
            var userId = "user1";
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockStarSystemRepository.Setup(r => r.GetById(1)).ReturnsAsync((StarSystems)null);

            await Assert.ThrowsAsync<StarSystemDoesNotExist>(() => _service.DeleteStarSystem(1, userId));
        }
    }
} 