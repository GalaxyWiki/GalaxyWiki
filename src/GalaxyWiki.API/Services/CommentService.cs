using System;
using System.Collections.Generic;
using System.Linq;
using GalaxyWiki.Api.DTOs;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Api.Repositories;

namespace GalaxyWiki.Api.Services
{
    public class CommentService : ICommentService
    {
       private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        private CommentDto MapToDto(Comments comment)
        {
            return new CommentDto
            {
                CommentId = comment.CommentId,
                CommentText = comment.CommentText,
                CreatedDate = comment.CreatedAt.ToString("yyyy-MM-dd"),
                UserId = comment.UserId,
                CelestialBodyId = comment.CelestialBodyId
            };
        }

        public IEnumerable<CommentDto> GetAll()
        {
            var comments = _commentRepository.GetAll();
            return comments.Select(MapToDto);
        }

        public CommentDto? GetById(int id)
        {
            var comment = _commentRepository.GetById(id);
            return comment != null ? MapToDto(comment) : null;
        }

        public CommentDto Create(CreateCommentDto commentDto)
        {
            var comment = new Comments
            {
                CommentText = commentDto.CommentText,
                UserId = commentDto.UserId,
                CelestialBodyId = commentDto.CelestialBodyId,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = _commentRepository.Create(comment);
            return MapToDto(createdComment);
        }

        public IEnumerable<CommentDto> GetByCelestialBody(int celestialBodyId)
        {
            var comments = _commentRepository.GetByCelestialBody(celestialBodyId);
            return comments.Select(MapToDto);
        }

        public IEnumerable<CommentDto> GetByUser(string userId)
        {
            var comments = _commentRepository.GetByUser(userId);
            return comments.Select(MapToDto);
        }

        public IEnumerable<CommentDto> GetByDateRange(DateTime startDate, DateTime endDate, int? celestialBodyId = null)
        {
            var comments = _commentRepository.GetByDateRange(startDate, endDate, celestialBodyId);
            return comments.Select(MapToDto);
        }

        public CommentDto? Update(int id, UpdateCommentDto updateDto)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null) return null;

            // Check if the comment is not older than one month
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            if (comment.CreatedAt < oneMonthAgo)
            {
                throw new InvalidOperationException("Cannot update comments older than one month");
            }

            comment.CommentText = updateDto.CommentText;
            var updatedComment = _commentRepository.Update(comment);
            return MapToDto(updatedComment);
        }

        public bool Delete(int id)
        {
            var comment = _commentRepository.GetById(id);
            if (comment == null) return false;

            _commentRepository.Delete(id);
            return true;
        }
    }
} 