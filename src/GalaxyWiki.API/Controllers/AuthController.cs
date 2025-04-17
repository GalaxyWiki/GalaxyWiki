using Microsoft.AspNetCore.Mvc;
using GalaxyWiki.API.Services;
using GalaxyWiki.API.DTO;

namespace GalaxyWiki.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AuthCode))
                return BadRequest("Auth code is required.");

            var result = await _authService.Login(request.AuthCode);

            return Ok(new
            {
                message = "Login successful",
                idToken = result[0],
                name = result[1]
            });
        }
    }
}