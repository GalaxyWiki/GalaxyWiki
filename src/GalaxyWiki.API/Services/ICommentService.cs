using System;
using System.Collections.Generic;
using GalaxyWiki.Api.DTOs;

namespace GalaxyWiki.Api.Services
{
    public interface ICommentService
    {
        IEnumerable<CommentDto> GetAll();
        CommentDto? GetById(Guid id);
        CommentDto Create(CreateCommentDto comment);
        IEnumerable<CommentDto> GetByCelestialBody(Guid celestialBodyId);
        IEnumerable<CommentDto> GetByUser(Guid userId);
        IEnumerable<CommentDto> GetByDateRange(DateTime startDate, DateTime endDate, Guid? celestialBodyId = null);
        CommentDto? Update(Guid id, UpdateCommentDto updateDto);
        bool Delete(Guid id);
    }
} 