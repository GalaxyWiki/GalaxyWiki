using System.Threading.Tasks;
using GalaxyWiki.Core.Entities;
using NHibernate.Linq;
using ISession = NHibernate.ISession;

namespace GalaxyWiki.API.Repositories
{
    public class BodyTypeRepository : IBodyTypeRepository
    {
        private readonly ISession _session;

        public BodyTypeRepository(ISession session)
        {
            _session = session;
        }

        public async Task<BodyTypes?> GetById(int id)
        {
            return await _session.GetAsync<BodyTypes>(id);
        }

    }
} 