using GalaxyWiki.Core.Entities;
using ISession = NHibernate.ISession;

namespace GalaxyWiki.Api.Repositories
{
    public class RoleRepository
    {
        private readonly ISession _session;

        public RoleRepository(ISession session)
        {
            _session = session;
        }

        public async Task<Roles> GetById(int id)
        {
            return await _session.GetAsync<Roles>(id);
        }
    }
} 