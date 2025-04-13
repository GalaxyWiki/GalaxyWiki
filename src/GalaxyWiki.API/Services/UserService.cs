using Google.Apis.Auth;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;

namespace GalaxyWiki.API.Services
{
    public class UserService
    {
        private readonly NHibernate.ISession _session;

        public UserService(NHibernate.ISession session)
        {
            _session = session;
        }

        public async Task<Users> getUserById(string googleSub)
        {
            return await _session.GetAsync<Users>(googleSub);
        }

        public async Task<Users> createUser(string googleSub, string email, string name, UserRole userRole)
        {
            using var transaction = _session.BeginTransaction();
            try
            {   
                var role = await _session.GetAsync<Roles>((int)userRole);

                var newUser = new Users
                {
                    Id = googleSub,
                    Email = email,
                    DisplayName = name,
                    Role = role
                };

                await _session.SaveAsync(newUser);

                transaction.Commit();

                return newUser;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}