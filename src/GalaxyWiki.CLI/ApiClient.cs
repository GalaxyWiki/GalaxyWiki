using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using GalaxyWiki.Core.Entities;

public static class ApiClient
{
    private static readonly HttpClient httpClient = new HttpClient();

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

    public static async Task<List<CelestialBodies>> GetCelestialBodiesAsync(string apiUrl)
    {
        // Perform the HTTP GET request.
        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        // Read the JSON response string.
        string jsonString = await response.Content.ReadAsStringAsync();

        // Deserialize the JSON into a List of CelestialBody objects.
        List<CelestialBodies> bodies = JsonSerializer
            .Deserialize<List<CelestialBodies>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new List<CelestialBodies>();

        return bodies;
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