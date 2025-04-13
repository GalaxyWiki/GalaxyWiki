using System;
using System.Collections.Generic;
using GalaxyWiki.Core.Entities;

namespace GalaxyWiki.Api.Repositories
{
    public interface ICommentRepository
    {
        IEnumerable<Comments> GetAll();
        Comments GetById(int id);
        Comments Create(Comments comment);
        IEnumerable<Comments> GetByCelestialBody(int celestialBodyId);
        IEnumerable<Comments> GetByUser(string userId);
        IEnumerable<Comments> GetByDateRange(DateTime startDate, DateTime endDate, int? celestialBodyId = null);
        Comments Update(Comments comment);
        void Delete(int id);
    }
} 