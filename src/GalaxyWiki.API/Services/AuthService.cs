using Google.Apis.Auth;
using GalaxyWiki.Core.Enums;
using System.Text.Json;

namespace GalaxyWiki.API.Services
{
    public class AuthService
    {
        private readonly UserService _userService;

        public AuthService(NHibernate.ISession session, UserService userService)
        {
            _userService = userService;
        }

        public async Task<string[]> Login(string authCode)
        {
            var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
            var redirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI");

            using var http = new HttpClient();
            var resp = await http.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "code", authCode },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" }
                }));

            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                throw new InvalidGoogleTokenException("Google token exchange failed: " + err);
            }

            var json = await resp.Content.ReadAsStringAsync();
            var token = JsonDocument.Parse(json).RootElement;

            var idToken = token.GetProperty("id_token").GetString();
            
            GoogleJsonWebSignature.Payload payload;
            try 
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            }
            catch (InvalidJwtException ex)
            {
                throw new InvalidGoogleTokenException("Invalid Google ID token: " + ex.Message);
            }

            var user = await _userService.GetUserById(payload.Subject);

            if (user == null)
            {
                user = await _userService.CreateUser(payload.Subject, payload.Email, payload.Name, UserRole.Viewer);
            }

            return [idToken, user.DisplayName];     
        }

        public async Task<bool> CheckUserHasAccessRight(UserRole[] accessLevelRequired, string? authorId)
        {
            if (string.IsNullOrEmpty(authorId))
            {
                throw new InvalidGoogleTokenException("Author Id missing.");
            }

            var user = await _userService.GetUserById(authorId);

            if (user == null)
            {
                throw new UserDoesNotExist("User does not exist.");
            }

            return Array.Exists(accessLevelRequired, r => (int)r == user.Role.Id);
        }
    }
}
