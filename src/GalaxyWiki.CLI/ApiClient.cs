using System.Net.Http.Json;
using System.Text.Json;
using GalaxyWiki.Core.Entities;

public static class ApiClient
{
    private static readonly HttpClient httpClient = new HttpClient();

    //==================== Auth ====================//
    public static async Task LoginAsync(string jwt)
    {
        var baseUrl = Environment.GetEnvironmentVariable("API_URL");

        if (string.IsNullOrEmpty(baseUrl))
        {
            TUI.Err("ENV", "API_URL environment variable is not set.");
            return;
        }

        var loginResponse = await httpClient.PostAsJsonAsync($"{baseUrl}/login", new { idToken = jwt });
        var loginResult = await loginResponse.Content.ReadAsStringAsync();

        Console.WriteLine($"Login response: {loginResult}");
    }

    //==================== Getters ====================//
    // Get a JSON string from the given API endpoint
    public static async Task<string> GetJson(string endpoint) {
        // Fetch
        HttpResponseMessage resp = await httpClient.GetAsync($"http://localhost:5216/api{endpoint}");
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    // Get data from an endpoint and auto-deserialize to the given type
    // If deserialization is not possible, a new blank/default instance of the given type is returned
    public static async Task<T> GetDeserialized<T>(string endpoint) where T : new() {
        string json = await GetJson(endpoint);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new T();
    }

    public static async Task<List<CelestialBodies>> GetCelestialBodies() {
        return await GetDeserialized<List<CelestialBodies>>("/celestial-body");
    }

    public static async Task<IdMap<CelestialBodies>> GetCelestialBodiesMap() {
        var bodies = await GetCelestialBodies();
        var res = new IdMap<CelestialBodies>();
        foreach(var body in bodies) { res.Add(body.Id, body); }

        return res;
    }
    
    public static async Task<Revision?> GetRevisionAsync(string apiUrl)
    {
        try
        {
            // Perform the HTTP GET request
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
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

    // Get comments for a celestial body
    public static async Task<List<Comment>> GetCommentsByCelestialBodyAsync(int celestialBodyId)
    {
        try
        {
            string endpoint = $"/comment/celestial_bodies/{celestialBodyId}";
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
            var commentRequest = new CreateCommentRequest
            {
                CommentText = commentText,
                CelestialBodyId = celestialBodyId
            };
            
            HttpResponseMessage response = await httpClient.PostAsJsonAsync(
                "http://localhost:5216/api/comment", 
                commentRequest
            );
            
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
}

// Simple DTO for comment data
public class Comment
{
    public int CommentId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
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