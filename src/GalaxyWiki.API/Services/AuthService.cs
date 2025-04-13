using Google.Apis.Auth;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;

namespace GalaxyWiki.API.Services
{
    public class AuthService
    {
        private readonly NHibernate.ISession _session;

        public AuthService(NHibernate.ISession session)
        {
            _session = session;
        }

        public async Task<string> Login(string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            
            var user = await _session.GetAsync<Users>(payload.Subject);

            if (user == null)
            {
                using var transaction = _session.BeginTransaction();
                try
                {   
                    var viewerRole = await _session.GetAsync<Roles>((int)UserRole.Viewer);

                    var newUser = new Users
                    {
                        Id = payload.Subject,
                        Email = payload.Email,
                        DisplayName = payload.Name,
                        Role = viewerRole
                    };

                    await _session.SaveAsync(newUser);

                    transaction.Commit();
                
                    return newUser.DisplayName;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            else
            {
                return user.DisplayName;
            }
        }
    }
}
