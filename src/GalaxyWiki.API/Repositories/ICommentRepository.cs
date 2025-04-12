using System;
using System.Collections.Generic;
using GalaxyWiki.Core.Entities;

namespace GalaxyWiki.Api.Repositories
{
    public interface ICommentRepository
    {
        IEnumerable<Comments> GetAll();
        Comments GetById(Guid id);
        Comments Create(Comments comment);
        IEnumerable<Comments> GetByCelestialBody(Guid celestialBodyId);
        IEnumerable<Comments> GetByUser(Guid userId);
        IEnumerable<Comments> GetByDateRange(DateTime startDate, DateTime endDate, Guid? celestialBodyId = null);
        Comments Update(Comments comment);
        void Delete(Guid id);
    }
} 