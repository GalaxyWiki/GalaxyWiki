using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;

namespace GalaxyWiki.API.Controllers
{
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
                    name = payload.Name
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
        public required string IdToken { get; set; }
    }
    
}

