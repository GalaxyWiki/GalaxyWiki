using GalaxyWiki.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Repositories
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comments>> GetAll();
        Task<Comments> GetById(int id);
        Task<IEnumerable<Comments>> GetByCelestialBody(int celestialBodyId);
        Task<IEnumerable<Comments>> GetByUser(string userId);
        Task<IEnumerable<Comments>> GetByDateRange(DateTime startDate, DateTime endDate, int? celestialBodyId = null);
        Task<Comments> Create(Comments comment);
        Task<Comments> Update(Comments comment);
        Task Delete(Comments comment);
    }
} 