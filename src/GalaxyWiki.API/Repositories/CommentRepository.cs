using System.Collections.Generic;
using System.Linq;
using GalaxyWiki.Core.Entities;
using NHibernate;
using NHibernate.Linq;
using ISession = NHibernate.ISession;

namespace GalaxyWiki.Api.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ISession _session;

        public CommentRepository(ISession session)
        {
            _session = session;
        }

        public IEnumerable<Comments> GetAll()
        {
            return _session.Query<Comments>().ToList();
        }

        public Comments GetById(int id)
        {
            return _session.Get<Comments>(id);
        }

        public Comments Create(Comments comment)
        {
            using var transaction = _session.BeginTransaction();
            _session.Save(comment);
            transaction.Commit();
            return comment;
        }

        public IEnumerable<Comments> GetByCelestialBody(int celestialBodyId)
        {
            return _session.Query<Comments>()
                .Where(c => c.CelestialBodyId == celestialBodyId)
                .ToList();
        }

        public IEnumerable<Comments> GetByUser(string userId)
        {
            return _session.Query<Comments>()
                .Where(c => c.UserId == userId)
                .ToList();
        }

        public IEnumerable<Comments> GetByDateRange(DateTime startDate, DateTime endDate, int? celestialBodyId = null)
        {
            var query = _session.Query<Comments>()
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate);

            if (celestialBodyId.HasValue)
            {
                query = query.Where(c => c.CelestialBodyId == celestialBodyId.Value);
            }

            return query.ToList();
        }

        public Comments Update(Comments comment)
        {
            using var transaction = _session.BeginTransaction();
            _session.Update(comment);
            transaction.Commit();
            return comment;
        }

        public void Delete(int id)
        {
            var comment = _session.Get<Comments>(id);
            if (comment != null)
            {
                using var transaction = _session.BeginTransaction();
                _session.Delete(comment);
                transaction.Commit();
            }
        }
    }
} 