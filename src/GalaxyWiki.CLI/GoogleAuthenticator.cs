using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

public class GoogleAuthenticator
{
    private static readonly string[] Scopes = { "openid", "email", "profile" };

    public async Task<string> GetIdTokenAsync()
    {
        var clientSecrets = new ClientSecrets
        {
            ClientId = Environment.GetEnvironmentVariable("CLIENT_ID"),
            ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET")
        };

        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            clientSecrets,
            Scopes,
            "user",
            CancellationToken.None,
            new FileDataStore("token.json", true)
        );

        return credential.Token.IdToken;
    }
}
