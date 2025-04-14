using System.Threading.Tasks;
using GalaxyWiki.Core.Entities;
using NHibernate.Linq;
using ISession = NHibernate.ISession;

namespace GalaxyWiki.Api.Repositories
{
    public class CelestialBodyRepository
    {
        private readonly ISession _session;

        public CelestialBodyRepository(ISession session)
        {
            _session = session;
        }

        public async Task<CelestialBodies?> GetById(int id)
        {
            return await _session.GetAsync<CelestialBodies>(id);
        }
        public async Task<CelestialBodies?> GetByName(string celestialBodyPath)
        {
            return await _session.Query<CelestialBodies>()
                                .FirstOrDefaultAsync(cb => cb.BodyName == celestialBodyPath);
        }
    }
} 