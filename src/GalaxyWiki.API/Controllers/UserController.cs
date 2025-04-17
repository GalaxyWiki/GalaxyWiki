using Microsoft.AspNetCore.Mvc;
using GalaxyWiki.API.Services;

namespace GalaxyWiki.API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
                return NotFound(new { error = "User not found." });

            return Ok(new
            {
                user.Id,
                user.Email,
                user.DisplayName,
                RoleId = user.Role.Id
            });
        }
    }
}