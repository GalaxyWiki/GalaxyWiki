using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
            return Ok(new
            {
                message = "Login successful",
                email = payload.Email
            });
        }
        catch (Exception)
        {
            return Unauthorized("Invalid Google ID token.");
        }
    }
}

public class LoginRequest
{
    public string IdToken { get; set; }
}
