using GalaxyWiki.API.DTOs;
using GalaxyWiki.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<Comments>> GetAll();
        Task<CommentRequest?> GetById(int id);
        Task<IEnumerable<CommentRequest>> GetByCelestialBody(int celestialBodyId);
        Task<IEnumerable<CommentRequest>> GetByUser(string userId);
        Task<IEnumerable<CommentRequest>> GetByDateRange(DateTime startDate, DateTime endDate, int? celestialBodyId = null);
        Task<CommentRequest> Create(CreateCommentRequest commentDto, string userId);
        Task<CommentRequest> Update(int id, UpdateCommentRequest updateDto, string userId);
        Task Delete(int id, string userId);
    }
} 