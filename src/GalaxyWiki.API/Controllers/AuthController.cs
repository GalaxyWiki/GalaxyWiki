using Microsoft.AspNetCore.Mvc;
using GalaxyWiki.API.Services;
using GalaxyWiki.API.DTO;

namespace GalaxyWiki.API.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase
  {   
      private readonly AuthService _authService;

      public AuthController(AuthService authService)
      {
          _authService = authService;
      }

      [HttpPost("/login")]
      public async Task<IActionResult> Login([FromBody] LoginRequest request)
      {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            return BadRequest("ID token is required.");

        var userName = await _authService.Login(request.IdToken);
        return Ok(new
        {
            message = "Login successful",
            name = userName
        });
      }
  }
}