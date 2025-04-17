using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace GalaxyWiki.API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;

        public ChatController(ILogger<ChatController> logger) { _logger = logger; }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            try
            {
                if (request.Messages == null || !request.Messages.Any())
                {
                    return BadRequest(new { message = "No messages provided" });
                }

                // Get the last message from the list, which is the latest user message
                var lastMessage = request.Messages.Last();

                // Format all previous messages to send as context to Claude
                var claudeMessages = request.Messages.Select(m =>
                    new Message(m.Role, m.Content)).ToList();

                // Request
                var claudeRequest = new ClaudeRequest
                {
                    Messages = claudeMessages,
                    System = request.System,
                    MaxTokens = request.MaxTokens
                };
                var responseText = await MakeClaudeApiRequest(claudeRequest);

                return Ok(new ChatResponse { Message = responseText });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        private async Task<string> MakeClaudeApiRequest(ClaudeRequest request)
        {
            try
            {
                // Create HttpClient w/ headers
                using var client = new HttpClient();
                var apiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY") ??
                    throw new InvalidOperationException("CLAUDE_API_KEY environment variable is not set");

                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                // Serialize and send request
                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);

                // Handle response
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
            catch (Exception ex)
            {
                return $"Error communicating with Claude: {ex.Message}";
            }
        }
    }

    public class ChatRequest
    {
        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = new();

        [JsonPropertyName("system")]
        public string? System { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 1024;
    }

    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class ChatResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class ClaudeRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "claude-3-5-haiku-latest";

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
}