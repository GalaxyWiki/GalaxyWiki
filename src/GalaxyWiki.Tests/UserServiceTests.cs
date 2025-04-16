using Xunit;
using Moq;
using GalaxyWiki.API.Services;
using GalaxyWiki.API.Repositories;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;
using System.Threading.Tasks;

namespace GalaxyWiki.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();
            _service = new UserService(
                _mockUserRepository.Object,
                _mockRoleRepository.Object
            );
        }

        [Fact]
        public async Task GetUserById_ExistingUser_ReturnsUser()
        {
            var user = new Users { Id = "user1", Email = "test@example.com" };
            _mockUserRepository.Setup(r => r.GetById("user1")).ReturnsAsync(user);

            var result = await _service.GetUserById("user1");

            Assert.Equal(user, result);
        }

       /* [Fact]
        public async Task CreateUser_ValidRole_CreatesUser()
        {
            var role = new Roles { Id = (int)UserRole.Viewer, RoleName = "Viewer" };
            var user = new Users { Id = "user1", Email = "test@example.com", DisplayName = "Test", Role = role };
            _mockRoleRepository.Setup(r => r.GetById((int)UserRole.Viewer)).ReturnsAsync(role);
            _mockUserRepository.Setup(r => r.Create(It.IsAny<Users>())).ReturnsAsync(user);

            var result = await _service.CreateUser("user1", "test@example.com", "Test", UserRole.Viewer);

            Assert.Equal(user, result);
        }*/

        [Fact]
        public async Task CreateUser_InvalidRole_Throws()
        {
            _mockRoleRepository.Setup(r => r.GetById((int)UserRole.Viewer)).ReturnsAsync((Roles)null);

            await Assert.ThrowsAsync<RoleDoesNotExist>(() => _service.CreateUser("user1", "test@example.com", "Test", UserRole.Viewer));
        }
    }
} 