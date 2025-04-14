using Google.Apis.Auth;
using GalaxyWiki.Core.Enums;

namespace GalaxyWiki.API.Services
{
    public class AuthService
    {
        private readonly UserService _userService;

        public AuthService(NHibernate.ISession session, UserService userService)
        {
            _userService = userService;
        }

        public async Task<string> Login(string idToken)
        {
            GoogleJsonWebSignature.Payload payload;
            try 
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            }
            catch (InvalidJwtException ex)
            {
                throw new InvalidGoogleTokenException("Invalid Google ID token: " + ex.Message);
            }
            
            var user = await _userService.getUserById(payload.Subject);

            if (user == null)
            {
                user = await _userService.createUser(payload.Subject, payload.Email, payload.Name, UserRole.Viewer);
            }

            return user.DisplayName;     
        }

        public async Task<bool> CheckUserHasAccessRight(UserRole[] accessLevelRequired, string? authorId)
        {
            if (string.IsNullOrEmpty(authorId))
            {
                throw new InvalidGoogleTokenException("Author Id missing.");
            }

            var user = await _userService.getUserById(authorId);

            if (user == null)
            {
                throw new UserDoesNotExist("User does not exist.");
            }

            return Array.Exists(accessLevelRequired, r => (int)r == user.Role.Id);
        }
    }
}
