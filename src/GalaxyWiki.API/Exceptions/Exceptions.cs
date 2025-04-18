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
                RequestBodyIsInvalid => StatusCodes.Status400BadRequest,
                InvalidGoogleTokenException => StatusCodes.Status401Unauthorized,
                UserDoesNotHaveAccess => StatusCodes.Status403Forbidden,
                UserDoesNotExist => StatusCodes.Status401Unauthorized,
                CelestialBodyDoesNotExist => StatusCodes.Status404NotFound,
                StarSystemDoesNotExist => StatusCodes.Status404NotFound,
                BodyTypeDoesNotExist => StatusCodes.Status404NotFound,
                RoleDoesNotExist => StatusCodes.Status400BadRequest,
                CommentDoesNotExist => StatusCodes.Status400BadRequest,
                CommentTooOldToUpdate => StatusCodes.Status403Forbidden,
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

public class RequestBodyIsInvalid : Exception
{
    public RequestBodyIsInvalid(string message) : base(message) { }
}
public class InvalidGoogleTokenException : Exception
{
    public InvalidGoogleTokenException(string message) : base(message) { }
}
public class UserDoesNotHaveAccess : Exception
{
    public UserDoesNotHaveAccess(string message) : base(message) { }
}

public class UserDoesNotExist : Exception
{
    public UserDoesNotExist(string message) : base(message) { }
}

public class CelestialBodyDoesNotExist : Exception
{
    public CelestialBodyDoesNotExist(string message) : base(message) { }
}
public class BodyTypeDoesNotExist : Exception
{
    public BodyTypeDoesNotExist(string message) : base(message) { }
}
public class StarSystemDoesNotExist : Exception
{
    public StarSystemDoesNotExist(string message) : base(message) { }
}
public class RoleDoesNotExist : Exception
{
    public RoleDoesNotExist(string message) : base(message) { }
}

public class CommentDoesNotExist : Exception
{
    public CommentDoesNotExist(string message) : base(message) { }
}

public class CommentTooOldToUpdate : Exception
{
    public CommentTooOldToUpdate(string message) : base(message) { }
}