using GalaxyWiki.Core.Entities;
using NHibernate.Linq;
using ISession = NHibernate.ISession;

namespace GalaxyWiki.Api.Repositories
{
    public class ContentRevisionRepository : IContentRevisionRepository
    {
        private readonly ISession _session;

        public ContentRevisionRepository(ISession session)
        {
            _session = session;
        }

        public async Task<ContentRevisions> GetById(int id)
        {
            return await _session.GetAsync<ContentRevisions>(id);
        }

        public async Task<IEnumerable<ContentRevisions>> GetByCelestialBodyId(int id)
        {
            return await _session.Query<ContentRevisions>()
                                 .Where(r => r.CelestialBody.Id == id)
                                 .ToListAsync();
        }

        public async Task<ContentRevisions> Create(ContentRevisions contentRevisions)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                await _session.SaveAsync(contentRevisions);
                transaction.Commit();
                return contentRevisions;
            }
            catch 
            {
                transaction.Rollback();
                throw;
            }
        }
    }
} 