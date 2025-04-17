using GalaxyWiki.Core.Entities;
using NHibernate.Linq;
using ISession = NHibernate.ISession;

namespace GalaxyWiki.API.Repositories
{
    public class StarSystemRepository : IStarSystemRepository
    {
        private readonly ISession _session;

        public StarSystemRepository(ISession session)
        {
            _session = session;
        }

        public async Task<IEnumerable<StarSystems>> GetAll()
        {
            return await _session.Query<StarSystems>().ToListAsync();
        }

        public async Task<StarSystems> GetById(int id)
        {
            return await _session.GetAsync<StarSystems>(id);
        }

        public async Task<StarSystems> Create(StarSystems starSystem)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                await _session.SaveAsync(starSystem);
                transaction.Commit();
                return starSystem;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<StarSystems> Update(StarSystems starSystem)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                await _session.UpdateAsync(starSystem);
                transaction.Commit();
                return starSystem;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task Delete(StarSystems starSystem)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                await _session.DeleteAsync(starSystem);
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