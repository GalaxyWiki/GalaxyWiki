using System.Text.Json;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";

            var statusCode = ex switch
            {
                InvalidGoogleTokenException => StatusCodes.Status401Unauthorized,
                UserDoesNotExist => StatusCodes.Status401Unauthorized,
                CelestialBodyDoesNotExist => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = statusCode;

            var errorResponse = new Dictionary<string, string>
            {
                ["error"] = ex.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}

public class InvalidGoogleTokenException : Exception
{
    public InvalidGoogleTokenException(string message) : base(message) { }
}

public class UserDoesNotExist : Exception
{
    public UserDoesNotExist(string message) : base(message) { }
}

public class CelestialBodyDoesNotExist : Exception
{
    public CelestialBodyDoesNotExist(string message) : base(message) { }
}
