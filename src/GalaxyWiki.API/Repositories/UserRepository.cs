using GalaxyWiki.Core.Entities;
using ISession = NHibernate.ISession;

namespace GalaxyWiki.API.Repositories
{
    public class UserRepository
    {
        private readonly ISession _session;

        public UserRepository(ISession session)
        {
            _session = session;
        }

        public async Task<Users> GetById(string id)
        {
            return await _session.GetAsync<Users>(id);
        }

        public async Task<Users> Create(Users user)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                await _session.SaveAsync(user);
                transaction.Commit();
                return user;
            }
            catch 
            {
                transaction.Rollback();
                throw;
            }
        }
    }
} 