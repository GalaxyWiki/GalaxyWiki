using System.Threading.Tasks;
using GalaxyWiki.Core.Entities;
using NHibernate.Linq;
using ISession = NHibernate.ISession;

namespace GalaxyWiki.Api.Repositories
{
    public class BodyTypesRepository
    {
        private readonly ISession _session;

        public BodyTypesRepository(ISession session)
        {
            _session = session;
        }

        public async Task<BodyTypes?> GetById(int id)
        {
            return await _session.GetAsync<BodyTypes>(id);
        }

    }
} 