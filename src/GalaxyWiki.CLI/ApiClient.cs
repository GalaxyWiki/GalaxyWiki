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
            Console.WriteLine("API_URL environment variable is not set.");
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
            Console.WriteLine($"Error retrieving revision: {ex.Message}");
            return null;
        }
    }
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