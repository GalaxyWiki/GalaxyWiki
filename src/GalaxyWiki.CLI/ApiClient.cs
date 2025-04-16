using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GalaxyWiki.Core.Entities;
using Spectre.Console;



namespace GalaxyWiki.CLI
{
    public static class ApiClient
    {
        private static readonly HttpClient httpClient = new();
        public static string JWT { get; set; } = "";
        private static readonly string? clientId;
        private static readonly string? apiUrl;
        private static readonly string? redirectUri;

        static ApiClient()
        {
            clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
            apiUrl = Environment.GetEnvironmentVariable("API_URL");
            redirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI");

            if (string.IsNullOrEmpty(clientId))
            {
                TUI.Err("ENV", "API_URL environment variable is not set.");
                throw new Exception("API_URL environment variable is not set.");
            }
            if (string.IsNullOrEmpty(apiUrl))
            {
                TUI.Err("ENV", "API_URL environment variable is not set.");
                throw new Exception("API_URL environment variable is not set.");
            }
            if (string.IsNullOrEmpty(redirectUri))
            {
                TUI.Err("ENV", "REDIRECT_URL environment variable is not set.");
                throw new Exception("REDIRECT_URL environment variable is not set.");
            }
        }

        //==================== Auth ====================//
        public static async Task LoginAsync()
        {
            var scopes = "openid email profile";

            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                        $"response_type=code&client_id={clientId}&" +
                        $"redirect_uri={Uri.EscapeDataString(redirectUri!)}&" +
                        $"scope={Uri.EscapeDataString(scopes)}&" +
                        $"access_type=offline";

            using var listener = new HttpListener();
            listener.Prefixes.Add(redirectUri!);
            listener.Start();

            AnsiConsole.Write(new Rule("[orange1]Opening browser for google login...[/] "));
            Process.Start(new ProcessStartInfo(authUrl) { UseShellExecute = true });

            var context = await listener.GetContextAsync();
            var authCode = context.Request.QueryString["code"];
            var responseString = """
            <html>
                <head>
                    <title>Login success</title>
                    <script> function closePage() { window.close(); } </script>
                    <style>
                        body            { background: #140d1c; color: #f0ebf5; margin: 0; }
                        #txt            { display: flex; center; width: 100%; height: 100%; justify-content: center; align-items: center; }
                        #msg            { text-align: center; }
                        h1              { font-family: monospace; font-size: 3em; }
                        p               { font-family: sans-serif; }
                        #space, .stars  { overflow: hidden; position: absolute; top: 0; bottom: 0; left: 0; right: 0; }

                        .stars {
                            background-image: 
                                radial-gradient(2px 2px at 20px 30px, #eee, rgba(0,0,0,0)),
                                radial-gradient(2px 2px at 40px 70px, #fff, rgba(0,0,0,0)),
                                radial-gradient(2px 2px at 50px 160px, #ddd, rgba(0,0,0,0)),
                                radial-gradient(2px 2px at 90px 40px, #fff, rgba(0,0,0,0)),
                                radial-gradient(2px 2px at 130px 80px, #fff, rgba(0,0,0,0)),
                                radial-gradient(2px 2px at 160px 120px, #ddd, rgba(0,0,0,0));
                            background-repeat: repeat;
                            background-size: 200px 200px;
                            animation: zoom 2s infinite;
                            opacity: 0;
                        }

                        .stars:nth-child(1) { background-position: 50% 50%; animation-delay: 0s; }
                        .stars:nth-child(2) { background-position: 20% 60%; animation-delay: 500ms; }
                        .stars:nth-child(3) { background-position: -20% -30%; animation-delay: 1s; }
                        .stars:nth-child(4) { background-position: 40% -80%; animation-delay: 1500ms; }
                        .stars:nth-child(5) { background-position: -20% 30%; animation-delay: 2s; }
                        .stars:nth-child(6) { background-position: 40% -80%; animation-delay: 2500ms; }
                        .stars:nth-child(7) { background-position: -20% 30%; animation-delay: 3s; }
                        .stars:nth-child(8) { background-position: -20% 30%; animation-delay: 3500ms; }
                        .stars:nth-child(9) { background-position: 40% -80%; animation-delay: 4s; }

                        @keyframes zoom {
                            0% { opacity: 0; transform: scale(0.5); animation-timing-function: ease-in; } 
                            85% { opacity: 1; transform: scale(2.8); animation-timing-function: linear; }
                            100% { opacity: 0; transform: scale(3.5); }
                        }
                    </style>
                </head>
                <body>
                    <div id="txt">
                        <div id="msg">
                            <h1> Login successful &#128640; </h1>
                            <p>You may now close this page</p>
                            <br /><br /><br />
                        </div>
                    </div>
                    <div id="space">
                        <div class="stars"></div> <div class="stars"></div> <div class="stars"></div>
                        <div class="stars"></div> <div class="stars"></div> <div class="stars"></div>
                        <div class="stars"></div> <div class="stars"></div> <div class="stars"></div>
                    </div>

                </body>
            </html>
            """;
            var buffer = Encoding.UTF8.GetBytes(responseString);
            context.Response.OutputStream.Write(buffer);
            context.Response.OutputStream.Close();
            Thread.Sleep(1000);
            listener.Stop();

            Console.WriteLine($"Received auth code: {authCode}");

            AnsiConsole.Write(new Rule("[cyan]Logging into GalaxyWiki api...[/]"));

            using var http = new HttpClient();
            var res = await http.PostAsync(apiUrl + "/login",
                new StringContent(JsonSerializer.Serialize(new { authCode }), Encoding.UTF8, "application/json"));

            var json = await res.Content.ReadAsStringAsync();
            JWT = JsonDocument.Parse(json).RootElement.GetProperty("idToken").GetString() ?? "";
            if (JWT == "")
            {
                TUI.Err("JWT", "Error getting JWT. Response did not contain a token.");
                return;
            }
            var userName = JsonDocument.Parse(json).RootElement.GetProperty("name").GetString();

            Console.WriteLine($"JWT from API: \n{JWT}");

            AnsiConsole.Write(new Rule("[green]Logged in successfully. Welcome " + userName + ".[/]"));
        }

        //==================== Getters ====================//
        // Get a JSON string from the given API endpoint
        public static async Task<string> GetJson(string endpoint)
        {
            // Fetch
            HttpResponseMessage resp = await httpClient.GetAsync($"{apiUrl}/api{endpoint}");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync();
        }

        // Get data from an endpoint and auto-deserialize to the given type
        // If deserialization is not possible, a new blank/default instance of the given type is returned
        public static async Task<T> GetDeserialized<T>(string endpoint) where T : new()
        {
            string json = await GetJson(endpoint);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new T();
        }

        public static async Task<List<CelestialBodies>> GetCelestialBodies()
        {
            return await GetDeserialized<List<CelestialBodies>>("/celestial-body");
        }

        public static async Task<IdMap<CelestialBodies>> GetCelestialBodiesMap()
        {
            var bodies = await GetCelestialBodies();
            var res = new IdMap<CelestialBodies>();
            foreach (var body in bodies) { res.Add(body.Id, body); }

            return res;
        }

        public static async Task<Revision?> GetRevisionAsync(string endpoint)
        {
            try
            {
                // Perform the HTTP GET request
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl + endpoint);
                response.EnsureSuccessStatusCode();

                // Read the JSON response string
                string jsonString = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into a Revision object
                Revision? revision = JsonSerializer
                    .Deserialize<Revision>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return revision;
            }
            catch (Exception ex)
            {
                TUI.Err("GET", "Couldn't retrieve revision", ex.Message);
                return null;
            }
        }

        public static async Task<List<Revision>> GetRevisionsByBodyNameAsync(string bodyName)
        {
            try
            {
                // Performing the HTTP GET request
                string endpoint = $"/api/revision/by-name/{Uri.EscapeDataString(bodyName)}";
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl + endpoint);
                response.EnsureSuccessStatusCode();

                // Reading the JSON response string
                string jsonString = await response.Content.ReadAsStringAsync();

                // Deserialising the JSON into a List<Revision> object
                List<Revision>? revisions = JsonSerializer
                    .Deserialize<List<Revision>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return revisions ?? [];
            }
            catch (Exception ex)
            {
                TUI.Err("GET", "Couldn't retrieve revisions", ex.Message);
                return new List<Revision>();
            }
        }

    // Get comments for a celestial body
    public static async Task<List<Comment>> GetCommentsByCelestialBodyAsync(int celestialBodyId)
    {
        try
        {
            string endpoint = $"/comment/celestial-body/{celestialBodyId}";
            return await GetDeserialized<List<Comment>>(endpoint);
        }
        catch (Exception ex)
        {
            TUI.Err("GET", "Couldn't retrieve comments", ex.Message);
            return new List<Comment>();
        }
    }
    
    // Get comments by date range
    public static async Task<List<Comment>> GetCommentsByDateRangeAsync(DateTime startDate, DateTime endDate, int celestialBodyId)
    {
        try
        {
            string formattedStart = startDate.ToString("yyyy-MM-dd");
            string formattedEnd = endDate.ToString("yyyy-MM-dd");
            string endpoint = $"/comment/date-range?startDate={formattedStart}&endDate={formattedEnd}&celestialBodyId={celestialBodyId}";
            return await GetDeserialized<List<Comment>>(endpoint);
        }
        catch (Exception ex)
        {
            TUI.Err("GET", "Couldn't retrieve comments by date range", ex.Message);
            return new List<Comment>();
        }
    }
    
    // Create a new comment
    public static async Task<Comment?> CreateCommentAsync(string commentText, int celestialBodyId)
    {
        try
        {
            if (JWT == "")
            {
                TUI.Err("AUTH", "Please login to post a comment.");
                return null;
            }
            
            var commentRequest = new CreateCommentRequest
            {
                CommentText = commentText,
                CelestialBodyId = celestialBodyId
            };
            
            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl + "/api/comment")
            {
                Content = JsonContent.Create(commentRequest)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JWT);

                var response = await httpClient.SendAsync(request);

                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Comment>(
                    jsonResponse,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (Exception ex)
            {
                TUI.Err("POST", "Couldn't create comment", ex.Message);
                return null;
            }
        }

        // Delete a comment by ID
        public static async Task<bool> DeleteCommentAsync(int commentId)
        {
            try
            {
                if (JWT == "")
                {
                    TUI.Err("AUTH", "Please login to delete a comment.");
                    return false;
                }
                
                var request = new HttpRequestMessage(HttpMethod.Delete, apiUrl + $"/api/comment/{commentId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JWT);

                var response = await httpClient.SendAsync(request);
                
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                TUI.Err("DELETE", $"Couldn't delete comment with ID {commentId}", ex.Message);
                return false;
            }
        }
    
    // Get user by ID
    public static async Task<Users?> GetUserByIdAsync(string userId)
    {
        try
        {
            string endpoint = $"/user/{userId}";
            return await GetDeserialized<Users>(endpoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving user: {ex.Message}");
            return null;
        }
    }


    public static async Task<Comment?> UpdateCommentAsync(int commentId, string commentText)
    {
        try
        {
            if (JWT == "")
            {
                TUI.Err("AUTH", "Please login to update a comment.");
                return null;
            }

            var updateRequest = new UpdateCommentRequest { CommentText = commentText };
            var request = new HttpRequestMessage(HttpMethod.Put, apiUrl + "/api/comment/" + commentId)
            {
                Content = JsonContent.Create(updateRequest)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JWT);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Comment>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
        catch (Exception ex)
        {
            TUI.Err("PUT", "Couldn't update comment", ex.Message);
            return null;
        }
    }

        // Create a new celestial body
        public static async Task<CelestialBodies?> CreateCelestialBodyAsync(string bodyName, int bodyTypeId, int? orbitsId)
        {
            try
            {
                if (JWT == "")
                {
                    TUI.Err("AUTH", "Please login to create a celestial body.");
                    return null;
                }
                
                var bodyRequest = new
                {
                    BodyName = bodyName,
                    BodyTypeId = bodyTypeId,
                    OrbitsId = orbitsId
                };
                
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl + "/api/celestial-body")
                {
                    Content = JsonContent.Create(bodyRequest)
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JWT);

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<CelestialBodies>(
                    jsonResponse, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (Exception ex)
            {
                TUI.Err("POST", "Couldn't create celestial body", ex.Message);
                return null;
            }
        }
        
        // Update an existing celestial body
        public static async Task<CelestialBodies?> UpdateCelestialBodyAsync(int bodyId, string bodyName, int bodyTypeId, int? orbitsId)
        {
            try
            {
                if (JWT == "")
                {
                    TUI.Err("AUTH", "Please login to update a celestial body.");
                    return null;
                }
                
                var bodyRequest = new
                {
                    BodyName = bodyName,
                    BodyTypeId = bodyTypeId,
                    OrbitsId = orbitsId
                };
                
                var request = new HttpRequestMessage(HttpMethod.Put, apiUrl + $"/api/celestial-body/{bodyId}")
                {
                    Content = JsonContent.Create(bodyRequest)
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JWT);

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<CelestialBodies>(
                    jsonResponse, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (Exception ex)
            {
                TUI.Err("PUT", $"Couldn't update celestial body with ID {bodyId}", ex.Message);
                return null;
            }
        }
        
        // Delete a celestial body by ID
        public static async Task<bool> DeleteCelestialBodyAsync(int bodyId)
        {
            try
            {
                if (JWT == "")
                {
                    TUI.Err("AUTH", "Please login to delete a celestial body.");
                    return false;
                }
                
                var request = new HttpRequestMessage(HttpMethod.Delete, apiUrl + $"/api/celestial-body/{bodyId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JWT);

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                
                return true;
            }
            catch (Exception ex)
            {
                TUI.Err("DELETE", $"Couldn't delete celestial body with ID {bodyId}", ex.Message);
                return false;
            }
        }
}

    // Simple DTO for comment data
    public class Comment
    {
        public int CommentId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int CelestialBodyId { get; set; }
        public string CelestialBodyName { get; set; } = string.Empty;
    }

    // Simple DTO to create a comment
    public class CreateCommentRequest
    {
        public string CommentText { get; set; } = string.Empty;
        public int CelestialBodyId { get; set; }
    }

// Simple DTO to hold revision data
public class Revision
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CelestialBodyName { get; set; }
    public string? AuthorDisplayName { get; set; }
}

// Simple DTO to hold user data
public class Users
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int RoleId { get; set; }
}

public class UpdateCommentRequest
{
    public string CommentText { get; set; }
}
}
