using GalaxyWiki.Api.DTOs;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Api.Repositories;
using GalaxyWiki.API.Services;
using GalaxyWiki.Core.Enums;

namespace GalaxyWiki.Api.Services
{
    public class CommentService : ICommentService
    {
        private IAuthService _authService;
        private IUserService _userService;
        private readonly CommentRepository _commentRepository;
        private readonly ICelestialBodyRepository _celestialBodyRepository;

        public CommentService(IAuthService authService, IUserService userService, CommentRepository commentRepository, ICelestialBodyRepository celestialBodyRepository)
        {
            _authService = authService;
            _userService = userService;
            _commentRepository = commentRepository;
            _celestialBodyRepository = celestialBodyRepository;
        }

        private CommentRequest MapToDto(Comments comment)
        {
            return new CommentRequest
            {
                CommentId = comment.CommentId,
                CommentText = comment.CommentText,
                CreatedDate = comment.CreatedAt.ToString("yyyy-MM-dd"),
                UserId = comment.Author.Id,
                DisplayName = comment.Author.DisplayName,
                CelestialBodyId = comment.CelestialBodyId
            };
        }

        public async Task<IEnumerable<Comments>> GetAll()
        {
            var comments = await _commentRepository.GetAll();
            return comments;
        }

        public async Task<CommentRequest?> GetById(int id)
        {
            var comment = await _commentRepository.GetById(id);
            return comment != null ? MapToDto(comment) : null;
        }

        public async Task<IEnumerable<CommentRequest>> GetByCelestialBody(int celestialBodyId)
        {
            var comments = await _commentRepository.GetByCelestialBody(celestialBodyId);
            return comments.Select(MapToDto);
        }

        public async Task<IEnumerable<CommentRequest>> GetByUser(string userId)
        {
            var comments = await _commentRepository.GetByUser(userId);
            return comments.Select(MapToDto);
        }

        public async Task<IEnumerable<CommentRequest>> GetByDateRange(DateTime startDate, DateTime endDate, int? celestialBodyId = null)
        {
            var comments = await _commentRepository.GetByDateRange(startDate, endDate, celestialBodyId);
            return comments.Select(MapToDto);
        }

        public async Task<CommentRequest> Create(CreateCommentRequest commentDto, string userId)
        {
            if (await _authService.CheckUserHasAccessRight([UserRole.Admin, UserRole.Viewer], userId) == false)
            {
                throw new UserDoesNotHaveAccess("You do not have access to perform this action.");
            }

            var user = await _userService.GetUserById(userId);

            var celestialBody = await _celestialBodyRepository.GetById(commentDto.CelestialBodyId);

            if (celestialBody == null)
                throw new CelestialBodyDoesNotExist("Celestial body not found.");

            var comment = new Comments
            {
                CommentText = commentDto.CommentText,
                Author = user,
                CelestialBodyId = commentDto.CelestialBodyId,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _commentRepository.Create(comment);
            return MapToDto(createdComment);
        }

        public async Task<CommentRequest> Update(int id, UpdateCommentRequest updateDto, string userId)
        {
            if (await _authService.CheckUserHasAccessRight([UserRole.Admin, UserRole.Viewer], userId) == false)
            {
                throw new UserDoesNotHaveAccess("You do not have access to perform this action.");
            }

            var user = await _userService.GetUserById(userId);

            var comment = await _commentRepository.GetById(id);
            if (comment == null) 
                throw new CommentDoesNotExist("The selected comment does not exist");

            if (user.Id != comment.Author.Id)
                throw new UserDoesNotHaveAccess("Cannot update a comment that is not your own."); 

            // Check if the comment is not older than one month
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            if (comment.CreatedAt < oneMonthAgo)
                throw new CommentTooOldToUpdate("Cannot update comments older than one month");

            comment.CommentText = updateDto.CommentText;
            var updatedComment = await _commentRepository.Update(comment);

            return MapToDto(updatedComment);
        }

        public async Task Delete(int id, string userId)
        {
            if (await _authService.CheckUserHasAccessRight([UserRole.Admin, UserRole.Viewer], userId) == false)
            {
                throw new UserDoesNotHaveAccess("You do not have access to perform this action.");
            }

            var user = await _userService.GetUserById(userId);

            var comment = await _commentRepository.GetById(id);
            if (comment == null) 
                throw new CommentDoesNotExist("The selected comment does not exist");

            if (user.Role.Id != (int)UserRole.Admin && user.Id != comment.Author.Id)
                throw new UserDoesNotHaveAccess("Cannot delete a comment that is not your own."); 

            await _commentRepository.Delete(comment);
        }
    }
} 