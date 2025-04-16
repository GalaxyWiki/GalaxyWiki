using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GalaxyWiki.Cli
{
    public class ClaudeRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "claude-3-7-sonnet-20250219";

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 1024;

        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; } = new();

        [JsonPropertyName("system")]
        public string? System { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        public Message(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }

    public class ClaudeResponse
    {
        [JsonPropertyName("content")]
        public List<Content> Content { get; set; } = new();

        [JsonPropertyName("error")]
        public ErrorResponse? Error { get; set; }
    }

    public class ErrorResponse
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class Content
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public static class ClaudeClient
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string apiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY") ?? 
            throw new InvalidOperationException("CLAUDE_API_KEY environment variable is not set");

        static ClaudeClient()
        {
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Remove("Authorization");  // Remove any existing auth header
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        }

        public static async Task<string> GetResponse(string userMessage, string? context = null)
        {
            try
            {
                string systemPrompt = "You are a helpful assistant with expertise in astronomy and space science.";
                if (!string.IsNullOrEmpty(context))
                {
                    // Append the context to the base system prompt
                    systemPrompt += $" The user is currently asking questions about {context}."; 
                }

                var request = new ClaudeRequest
                {
                    Messages = new List<Message>
                    {
                        new Message("user", userMessage)
                    },
                    System = systemPrompt // Use the potentially enhanced system prompt
                };

                var content = JsonContent.Create(request);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var response = await client.PostAsync(
                    "https://api.anthropic.com/v1/messages",
                    content
                );

                var responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return $"Error from Claude API: {response.StatusCode} - {responseContent}";
                }
                
                var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseContent);
                if (claudeResponse?.Error != null)
                {
                    return $"Claude API Error: {claudeResponse.Error.Type} - {claudeResponse.Error.Message}";
                }
                
                if (claudeResponse?.Content == null || !claudeResponse.Content.Any())
                {
                    return "No response content from Claude";
                }
                
                return claudeResponse.Content.First().Text;
            }
            catch (HttpRequestException ex)
            {
                return $"HTTP Request Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error communicating with Claude: {ex.Message}";
            }
        }
    }
} 