using Xunit;
using Moq;
using GalaxyWiki.API.Services;
using GalaxyWiki.API.Repositories;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.API.DTOs;
using GalaxyWiki.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalaxyWiki.Tests
{
    public class CommentServiceTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ICommentRepository> _mockCommentRepository;
        private readonly Mock<ICelestialBodyRepository> _mockCelestialBodyRepository;
        private readonly CommentService _commentService;
        private readonly Users _user;

        public CommentServiceTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockUserService = new Mock<IUserService>();
            _mockCommentRepository = new Mock<ICommentRepository>();
            _mockCelestialBodyRepository = new Mock<ICelestialBodyRepository>();

            _commentService = new CommentService(
                _mockAuthService.Object,
                _mockUserService.Object,
                _mockCommentRepository.Object,
                _mockCelestialBodyRepository.Object
            );
            _user = new Users { Id = "user123", Role = new Roles { Id = (int)UserRole.Viewer, RoleName = "User" } };
        }

        [Fact]
        public async Task GetAll_ReturnsAllComments()
        {
            // Arrange
            var comments = new List<Comments>
            {
                new Comments { CommentId = 1, CommentText = "Test 1", Author = _user },
                new Comments { CommentId = 2, CommentText = "Test 2", Author = _user }
            };
            _mockCommentRepository.Setup(r => r.GetAll()).ReturnsAsync(comments);

            // Act
            var result = await _commentService.GetAll();

            // Assert
            Assert.Equal(2, result.Count());
            _mockCommentRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetById_ExistingComment_ReturnsComment()
        {
            // Arrange
            var comment = new Comments { CommentId = 1, CommentText = "Test", Author = _user };
            _mockCommentRepository.Setup(r => r.GetById(1)).ReturnsAsync(comment);

            // Act
            var result = await _commentService.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CommentId);
            Assert.Equal("Test", result.CommentText);
            _mockCommentRepository.Verify(r => r.GetById(1), Times.Once);
        }

        [Fact]
        public async Task GetById_NonExistingComment_ReturnsNull()
        {
            // Arrange
            _mockCommentRepository.Setup(r => r.GetById(1)).ReturnsAsync((Comments)null);

            // Act
            var result = await _commentService.GetById(1);

            // Assert
            Assert.Null(result);
            _mockCommentRepository.Verify(r => r.GetById(1), Times.Once);
        }

        [Fact]
        public async Task GetByCelestialBody_ReturnsCommentsForBody()
        {
            // Arrange
            var comments = new List<Comments>
            {
                new Comments { CommentId = 1, CommentText = "Test 1", CelestialBodyId = 1, Author = _user },
                new Comments { CommentId = 2, CommentText = "Test 2", CelestialBodyId = 1, Author = _user }
            };
            _mockCommentRepository.Setup(r => r.GetByCelestialBody(1)).ReturnsAsync(comments);

            // Act
            var result = await _commentService.GetByCelestialBody(1);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, c => Assert.Equal(1, c.CelestialBodyId));
            _mockCommentRepository.Verify(r => r.GetByCelestialBody(1), Times.Once);
        }

        [Fact]
        public async Task GetByUser_ReturnsCommentsForUser()
        {
            // Arrange
            var userId = "user123";
            var comments = new List<Comments>
            {
                new Comments { CommentId = 1, CommentText = "Test 1",Author = _user },
                new Comments { CommentId = 2, CommentText = "Test 2",Author = _user }
            };
            _mockCommentRepository.Setup(r => r.GetByUser(userId)).ReturnsAsync(comments);

            // Act
            var result = await _commentService.GetByUser(userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, c => Assert.Equal(userId, c.UserId));
            _mockCommentRepository.Verify(r => r.GetByUser(userId), Times.Once);
        }

        [Fact]
        public async Task GetByDateRange_ReturnsCommentsInRange()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var comments = new List<Comments>
            {
                new Comments { CommentId = 1, CommentText = "Test 1", CreatedAt = startDate.AddDays(1), Author = _user },
                new Comments { CommentId = 2, CommentText = "Test 2", CreatedAt = endDate.AddDays(-1), Author = _user }
            };
            _mockCommentRepository.Setup(r => r.GetByDateRange(startDate, endDate, null)).ReturnsAsync(comments);

            // Act
            var result = await _commentService.GetByDateRange(startDate, endDate);

            // Assert
            Assert.Equal(2, result.Count());
            _mockCommentRepository.Verify(r => r.GetByDateRange(startDate, endDate, null), Times.Once);
        }

        [Fact]
        public async Task Create_ValidRequest_CreatesComment()
        {
            // Arrange
            var userId = "user123";
            var request = new CreateCommentRequest { CommentText = "Test", CelestialBodyId = 1 };
            var celestialBody = new CelestialBodies { Id = 1, BodyName="Test", BodyType = 1 };

            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin, UserRole.Viewer }, userId))
                .ReturnsAsync(true);
            _mockUserService.Setup(u => u.GetUserById(userId)).ReturnsAsync(_user);
            _mockCelestialBodyRepository.Setup(r => r.GetById(1)).ReturnsAsync(celestialBody);
            _mockCommentRepository.Setup(r => r.Create(It.IsAny<Comments>()))
                .ReturnsAsync((Comments c) => c);

            // Act
            var result = await _commentService.Create(request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test", result.CommentText);
            Assert.Equal(1, result.CelestialBodyId);
            _mockAuthService.Verify(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin, UserRole.Viewer }, userId), Times.Once);
            _mockUserService.Verify(u => u.GetUserById(userId), Times.Once);
            _mockCelestialBodyRepository.Verify(r => r.GetById(1), Times.Once);
            _mockCommentRepository.Verify(r => r.Create(It.IsAny<Comments>()), Times.Once);
        }

        [Fact]
        public async Task Create_InvalidAccess_ThrowsException()
        {
            // Arrange
            var userId = "user123";
            var request = new CreateCommentRequest { CommentText = "Test", CelestialBodyId = 1 };

            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin, UserRole.Viewer }, userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<UserDoesNotHaveAccess>(() => _commentService.Create(request, userId));
        }

        [Fact]
        public async Task Create_NonExistingCelestialBody_ThrowsException()
        {
            // Arrange
            var userId = "user123";
            var request = new CreateCommentRequest { CommentText = "Test", CelestialBodyId = 1 };

            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin, UserRole.Viewer }, userId))
                .ReturnsAsync(true);
            _mockUserService.Setup(u => u.GetUserById(userId)).ReturnsAsync(_user);
            _mockCelestialBodyRepository.Setup(r => r.GetById(1)).ReturnsAsync((CelestialBodies)null);

            // Act & Assert
            await Assert.ThrowsAsync<CelestialBodyDoesNotExist>(() => _commentService.Create(request, userId));
        }

        [Fact]
        public async Task Update_ValidRequest_UpdatesComment()
        {
            // Arrange
            var userId = "user123";
            var commentId = 1;
            var request = new UpdateCommentRequest { CommentText = "Updated" };
            var existingComment = new Comments 
            { 
                CommentId = commentId, 
                CommentText = "Original", 
               Author = _user,
                CreatedAt = DateTime.UtcNow
            };

            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin, UserRole.Viewer }, userId))
                .ReturnsAsync(true);
            _mockUserService.Setup(u => u.GetUserById(userId)).ReturnsAsync(_user);
            _mockCommentRepository.Setup(r => r.GetById(commentId)).ReturnsAsync(existingComment);
            _mockCommentRepository.Setup(r => r.Update(It.IsAny<Comments>()))
                .ReturnsAsync((Comments c) => c);

            // Act
            var result = await _commentService.Update(commentId, request, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result.CommentText);
            _mockAuthService.Verify(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin, UserRole.Viewer }, userId), Times.Once);
            _mockUserService.Verify(u => u.GetUserById(userId), Times.Once);
            _mockCommentRepository.Verify(r => r.GetById(commentId), Times.Once);
            _mockCommentRepository.Verify(r => r.Update(It.IsAny<Comments>()), Times.Once);
        }

        [Fact]
        public async Task Update_NonExistingComment_ThrowsException()
        {
            // Arrange
            var userId = "user123";
            var commentId = 1;
            var request = new UpdateCommentRequest { CommentText = "Updated" };

            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin, UserRole.Viewer }, userId))
                .ReturnsAsync(true);
            _mockUserService.Setup(u => u.GetUserById(userId)).ReturnsAsync(_user);
            _mockCommentRepository.Setup(r => r.GetById(commentId)).ReturnsAsync((Comments)null);

            // Act & Assert
            await Assert.ThrowsAsync<CommentDoesNotExist>(() => _commentService.Update(commentId, request, userId));
        }

        [Fact]
        public async Task Update_OldComment_ThrowsException()
        {
            // Arrange
            var userId = "user123";
            var commentId = 1;
            var request = new UpdateCommentRequest { CommentText = "Updated" };
            var existingComment = new Comments 
            { 
                CommentId = commentId, 
                CommentText = "Original", 
               Author = _user,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };

            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin, UserRole.Viewer }, userId))
                .ReturnsAsync(true);
            _mockUserService.Setup(u => u.GetUserById(userId)).ReturnsAsync(_user);
            _mockCommentRepository.Setup(r => r.GetById(commentId)).ReturnsAsync(existingComment);

            // Act & Assert
            await Assert.ThrowsAsync<CommentTooOldToUpdate>(() => _commentService.Update(commentId, request, userId));
        }

        [Fact]
        public async Task Delete_ValidRequest_DeletesComment()
        {
            // Arrange
            var userId = "user123";
            var commentId = 1;
            var existingComment = new Comments 
            { 
                CommentId = commentId, 
               Author = _user
            };

            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin, UserRole.Viewer }, userId))
                .ReturnsAsync(true);
            _mockUserService.Setup(u => u.GetUserById(userId)).ReturnsAsync(_user);
            _mockCommentRepository.Setup(r => r.GetById(commentId)).ReturnsAsync(existingComment);
            _mockCommentRepository.Setup(r => r.Delete(It.IsAny<Comments>())).Returns(Task.CompletedTask);

            // Act
            await _commentService.Delete(commentId, userId);

            // Assert
            _mockAuthService.Verify(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin, UserRole.Viewer }, userId), Times.Once);
            _mockUserService.Verify(u => u.GetUserById(userId), Times.Once);
            _mockCommentRepository.Verify(r => r.GetById(commentId), Times.Once);
            _mockCommentRepository.Verify(r => r.Delete(It.IsAny<Comments>()), Times.Once);
        }

        [Fact]
        public async Task Delete_NonExistingComment_ThrowsException()
        {
            // Arrange
            var userId = "user123";
            var commentId = 1;
            

            _mockAuthService.Setup(a => a.CheckUserHasAccessRight(new[] { UserRole.Admin, UserRole.Viewer }, userId))
                .ReturnsAsync(true);
            _mockUserService.Setup(u => u.GetUserById(userId)).ReturnsAsync(_user);
            _mockCommentRepository.Setup(r => r.GetById(commentId)).ReturnsAsync((Comments)null);

            // Act & Assert
            await Assert.ThrowsAsync<CommentDoesNotExist>(() => _commentService.Delete(commentId, userId));
        }
    }
} 