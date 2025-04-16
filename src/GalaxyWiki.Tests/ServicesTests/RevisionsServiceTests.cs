using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using GalaxyWiki.API.DTOs;
using GalaxyWiki.API.Services;
using GalaxyWiki.Api.Repositories;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;

namespace GalaxyWiki.Tests.Services
{
    public class RevisionsServiceTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IContentRevisionRepository> _mockContentRevisionRepository;
        private readonly Mock<ICelestialBodyRepository> _mockCelestialBodyRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly ContentRevisionService _service;

        public RevisionsServiceTests()
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
        public async Task CreateRevision_Throws_WhenUserLacksAccess()
        {
            var request = new CreateRevisionRequest
            {
                CelestialBodyPath = "milkyway/earth",
                Content = "Unauthorized Content"
            };

            _mockAuthService
                .Setup(auth => auth.CheckUserHasAccessRight(It.IsAny<UserRole[]>(), It.IsAny<string>()))
                .ReturnsAsync(false); 

            await Assert.ThrowsAsync<UserDoesNotHaveAccess>(() =>
                _service.CreateRevision(request, "unauthorized-user"));
        }

        [Fact]
        public async Task CreateRevision_Throws_WhenUserNotFound()
        {
            var request = new CreateRevisionRequest
            {
                CelestialBodyPath = "milkyway/earth",
                Content = "Some content"
            };

            _mockAuthService
                .Setup(auth => auth.CheckUserHasAccessRight(It.IsAny<UserRole[]>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            _mockUserRepository
                .Setup(repo => repo.GetById(It.IsAny<string>()))
                .ReturnsAsync((Users)null!);

            await Assert.ThrowsAsync<UserDoesNotExist>(() =>
                _service.CreateRevision(request, "nonexistent-user"));
        }

       [Fact]
        public async Task CreateRevision_Throws_WhenCelestialBodyDoesNotExist()
        {
            var request = new CreateRevisionRequest
            {
                CelestialBodyPath = "earth",
                Content = "some content"
            };

            var authorId = "test-author-id";

            _mockAuthService.Setup(x => x.CheckUserHasAccessRight(It.IsAny<UserRole[]>(), It.IsAny<string>()))
                            .ReturnsAsync(true);

            _mockUserRepository.Setup(x => x.GetById(It.IsAny<string>()))
                            .ReturnsAsync(new Users { Id = "test-author-id", Email = "test@domain.com" }); 

            _mockCelestialBodyRepository.Setup(x => x.GetByName(It.IsAny<string>()))
                                        .ReturnsAsync((CelestialBodies)null);

            var exception = await Assert.ThrowsAsync<CelestialBodyDoesNotExist>(() => _service.CreateRevision(request, authorId));

            Assert.Equal("Celestial body not found.", exception.Message);
        }

        [Fact]
        public async Task CreateRevision_SuccessfullyCreatesRevision()
        {
            var request = new CreateRevisionRequest
            {
                CelestialBodyPath = "earth",
                Content = "some content"
            };

            var authorId = "test-author-id";

            _mockAuthService.Setup(x => x.CheckUserHasAccessRight(It.IsAny<UserRole[]>(), It.IsAny<string>()))
                            .ReturnsAsync(true); 

            _mockUserRepository.Setup(x => x.GetById(It.IsAny<string>()))
                            .ReturnsAsync(new Users { Id = "test-author-id", Email = "test@domain.com" }); 

            _mockCelestialBodyRepository.Setup(x => x.GetByName(It.IsAny<string>()))
                                        .ReturnsAsync(new CelestialBodies { Id = 1, BodyName = "earth", BodyType = 1 }); 

            _mockContentRevisionRepository.Setup(x => x.Create(It.IsAny<ContentRevisions>()))
                                        .ReturnsAsync((ContentRevisions revision) => revision);

            var result = await _service.CreateRevision(request, authorId);

            Assert.NotNull(result); 
            Assert.Equal("earth", result.CelestialBody.BodyName); 
            Assert.Equal("some content", result.Content); 
            Assert.Equal(authorId, result.Author.Id); 
            Assert.NotEqual(default, result.CreatedAt); 
        }




    }

}
