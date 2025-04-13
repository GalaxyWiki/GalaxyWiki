using System;
using System.Collections.Generic;
using GalaxyWiki.Api.DTOs;

namespace GalaxyWiki.Api.Services
{
    public interface ICommentService
    {
        IEnumerable<CommentDto> GetAll();
        CommentDto? GetById(int id);
        CommentDto Create(CreateCommentDto comment);
        IEnumerable<CommentDto> GetByCelestialBody(int celestialBodyId);
        IEnumerable<CommentDto> GetByUser(string userId);
        IEnumerable<CommentDto> GetByDateRange(DateTime startDate, DateTime endDate, int? celestialBodyId = null);
        CommentDto? Update(int id, UpdateCommentDto updateDto);
        bool Delete(int id);
    }
} 