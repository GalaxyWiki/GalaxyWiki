using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

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
}