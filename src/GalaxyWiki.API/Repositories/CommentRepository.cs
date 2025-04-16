using GalaxyWiki.Core.Entities;
using NHibernate.Linq;
using ISession = NHibernate.ISession;

namespace GalaxyWiki.API.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ISession _session;

        public CommentRepository(ISession session)
        {
            _session = session;
        }

        public async Task<IEnumerable<Comments>> GetAll()
        {
            return await _session.Query<Comments>().ToListAsync();
        }

        public async Task<Comments> GetById(int id)
        {
            return await _session.GetAsync<Comments>(id);
        }

        public async Task<IEnumerable<Comments>> GetByCelestialBody(int celestialBodyId)
        {
            return await _session.Query<Comments>()
                .Where(c => c.CelestialBodyId == celestialBodyId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comments>> GetByUser(string userId)
        {
            return await _session.Query<Comments>()
                .Where(c => c.Author.Id == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comments>> GetByDateRange(DateTime startDate, DateTime endDate, int? celestialBodyId = null)
        {
            var query = _session.Query<Comments>()
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate);

            if (celestialBodyId.HasValue)
            {
                query = query.Where(c => c.CelestialBodyId == celestialBodyId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Comments> Create(Comments comment)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                await _session.SaveAsync(comment);
                transaction.Commit();
                return comment;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Comments> Update(Comments comment)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                await _session.UpdateAsync(comment);
                transaction.Commit();
                return comment;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task Delete(Comments comment)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                await _session.DeleteAsync(comment);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
} 