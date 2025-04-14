using Microsoft.AspNetCore.Mvc;
using GalaxyWiki.Api.DTOs;
using GalaxyWiki.Api.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GalaxyWiki.API.Services;

namespace GalaxyWiki.Api.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;
        private readonly AuthService _authService;
        private readonly UserService _userService;

        public CommentsController(CommentService commentService, AuthService authService, UserService userService)
        {
            _commentService = commentService;
            _authService = authService;
            _userService = userService;
        }

        // GET /comments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var comments = await _commentService.GetAll();
            return Ok(comments);
        }

        // GET /comments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _commentService.GetById(id);
            if (comment == null) 
                return NotFound();
            return Ok(comment);
        }

        // GET /comments/celestial_bodies/{celestialBodyId}
        [HttpGet("celestial_bodies/{celestialBodyId}")]
        public async Task<IActionResult> GetByCelestialBody(int celestialBodyId)
        {
            var comments = await _commentService.GetByCelestialBody(celestialBodyId);
            return Ok(comments);
        }

        // GET /comments/users/{userId}
        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
        {
            var comments = await _commentService.GetByUser(userId);
            return Ok(comments);            
        }

        // GET /comments/date-range?startDate={startDate}&endDate={endDate}&celestialBodyId={celestialBodyId}
        // Example: /comments/date-range?startDate=2024-01-01&endDate=2024-12-31&celestialBodyId=123e4567-e89b-12d3-a456-426614174000
        [HttpGet("date-range")]
        public async Task<IActionResult> GetByDateRange([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] int? celestialBodyId)
        {
            if (!DateTime.TryParse(startDate, out DateTime start) || !DateTime.TryParse(endDate, out DateTime end))
            {
                return BadRequest("Invalid date format. Please use YYYY-MM-DD format.");
            }

            start = start.Date;
            end = end.Date.AddDays(1).AddSeconds(-1);

            var comments = await _commentService.GetByDateRange(start, end, celestialBodyId);
            return Ok(comments);
        }

        // POST /comments
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateCommentDto newComment)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = await _commentService.Create(newComment, userId);

            return CreatedAtAction(nameof(GetById), new { id = comment.CommentId }, comment);            
        }

        // PUT /comments/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int commentId, [FromBody] UpdateCommentDto updateDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var updatedComment = await _commentService.Update(commentId, updateDto, userId);

            return Ok(updatedComment);
        }

        // DELETE /comments/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int commentId)
        {   
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _commentService.Delete(commentId, userId);

            return NoContent();
        }
    }
}