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
    public class CelestialBodyServiceTests
    {
        private readonly Mock<ICelestialBodyRepository> _mockCelestialBodyRepository;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IBodyTypeRepository> _mockBodyTypeRepository;
        private readonly CelestialBodyService _service;

        public CelestialBodyServiceTests()
        {
            _mockCelestialBodyRepository = new Mock<ICelestialBodyRepository>();
            _mockAuthService = new Mock<IAuthService>();
            _mockBodyTypeRepository = new Mock<IBodyTypeRepository>();
            _service = new CelestialBodyService(
                _mockCelestialBodyRepository.Object,
                _mockAuthService.Object,
                _mockBodyTypeRepository.Object
            );
        }

       /* [Fact]
        public async Task GetAll_ReturnsCelestialBodiesWithBodyTypes()
        {
            var cb = new CelestialBodies { Id = 1, BodyName = "Earth", BodyType = 2 };
            var bt = new BodyTypes { Id = 2, TypeName = "Planet" };
            _mockCelestialBodyRepository.Setup(r => r.GetAll()).ReturnsAsync(new List<CelestialBodies> { cb });
            _mockBodyTypeRepository.Setup(r => r.GetById(2)).ReturnsAsync(bt);

            var result = await _service.GetAll();

            Assert.Single(result);
            Assert.Equal(cb, result.GetEnumerator().Current.CelestialBody);
        }*/

        [Fact]
        public async Task GetById_Existing_ReturnsCelestialBodyWithBodyType()
        {
            var cb = new CelestialBodies { Id = 1, BodyName = "Earth", BodyType = 2 };
            var bt = new BodyTypes { Id = 2, TypeName = "Planet" };
            _mockCelestialBodyRepository.Setup(r => r.GetById(1)).ReturnsAsync(cb);
            _mockBodyTypeRepository.Setup(r => r.GetById(2)).ReturnsAsync(bt);

            var (celestialBody, bodyType) = await _service.GetById(1);

            Assert.Equal(cb, celestialBody);
            Assert.Equal(bt, bodyType);
        }

        [Fact]
        public async Task GetById_NonExisting_ReturnsNulls()
        {
            _mockCelestialBodyRepository.Setup(r => r.GetById(1)).ReturnsAsync((CelestialBodies)null);

            var (celestialBody, bodyType) = await _service.GetById(1);

            Assert.Null(celestialBody);
            Assert.Null(bodyType);
        }

       /* [Fact]
        public async Task CreateCelestialBody_ValidRequest_CreatesBody()
        {
            var userId = "user1";
            var request = new CreateCelestialBodyRequest { BodyName = "Earth", BodyTypeId = 2 };
            var bt = new BodyTypes { Id = 2, TypeName = "Planet" };
            var cb = new CelestialBodies { Id = 0, BodyName = "Earth", BodyType = 2 };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockBodyTypeRepository.Setup(r => r.GetById(2)).ReturnsAsync(bt);
            _mockCelestialBodyRepository.Setup(r => r.Create(It.IsAny<CelestialBodies>())).ReturnsAsync(cb);

            var result = await _service.CreateCelestialBody(request, userId);

            Assert.Equal(cb, result);
        }*/

        [Fact]
        public async Task CreateCelestialBody_InvalidAccess_Throws()
        {
            var userId = "user1";
            var request = new CreateCelestialBodyRequest { BodyName = "Earth", BodyTypeId = 2 };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(false);

            await Assert.ThrowsAsync<UserDoesNotHaveAccess>(() => _service.CreateCelestialBody(request, userId));
        }

        [Fact]
        public async Task CreateCelestialBody_InvalidBodyType_Throws()
        {
            var userId = "user1";
            var request = new CreateCelestialBodyRequest { BodyName = "Earth", BodyTypeId = 2 };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockBodyTypeRepository.Setup(r => r.GetById(2)).ReturnsAsync((BodyTypes)null);

            await Assert.ThrowsAsync<BodyTypeDoesNotExist>(() => _service.CreateCelestialBody(request, userId));
        }

        [Fact]
        public async Task UpdateCelestialBody_ValidRequest_UpdatesBody()
        {
            var userId = "user1";
            var cb = new CelestialBodies { Id = 1, BodyName = "Earth", BodyType = 2 };
            var bt = new BodyTypes { Id = 2, TypeName = "Planet" };
            var request = new UpdateCelestialBodyRequest { BodyName = "Earth", BodyTypeId = 2 };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockCelestialBodyRepository.Setup(r => r.GetById(1)).ReturnsAsync(cb);
            _mockBodyTypeRepository.Setup(r => r.GetById(2)).ReturnsAsync(bt);
            _mockCelestialBodyRepository.Setup(r => r.Update(It.IsAny<CelestialBodies>())).ReturnsAsync(cb);

            var result = await _service.UpdateCelestialBody(1, request, userId);

            Assert.Equal(cb, result);
        }

        [Fact]
        public async Task UpdateCelestialBody_NonExisting_Throws()
        {
            var userId = "user1";
            var request = new UpdateCelestialBodyRequest { BodyName = "Earth", BodyTypeId = 2 };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockCelestialBodyRepository.Setup(r => r.GetById(1)).ReturnsAsync((CelestialBodies)null);

            await Assert.ThrowsAsync<CelestialBodyDoesNotExist>(() => _service.UpdateCelestialBody(1, request, userId));
        }

        [Fact]
        public async Task DeleteCelestialBody_ValidRequest_DeletesBody()
        {
            var userId = "user1";
            var cb = new CelestialBodies { Id = 1, BodyName = "Earth", BodyType = 2 };
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockCelestialBodyRepository.Setup(r => r.GetById(1)).ReturnsAsync(cb);
            _mockCelestialBodyRepository.Setup(r => r.Delete(cb)).Returns(Task.CompletedTask);

            await _service.DeleteCelestialBody(1, userId);
            _mockCelestialBodyRepository.Verify(r => r.Delete(cb), Times.Once);
        }

        [Fact]
        public async Task DeleteCelestialBody_NonExisting_Throws()
        {
            var userId = "user1";
            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin }, userId)).ReturnsAsync(true);
            _mockCelestialBodyRepository.Setup(r => r.GetById(1)).ReturnsAsync((CelestialBodies)null);

            await Assert.ThrowsAsync<CelestialBodyDoesNotExist>(() => _service.DeleteCelestialBody(1, userId));
        }
    }
} 