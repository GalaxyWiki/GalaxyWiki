using Microsoft.AspNetCore.Mvc;
using GalaxyWiki.API.DTOs;
using GalaxyWiki.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GalaxyWiki.API.Controllers
{
    [ApiController]
    [Route("api/comment")]
    public class CommentController : ControllerBase
    {
        private readonly CommentService _commentService;

        public CommentController(CommentService commentService)
        {
            _commentService = commentService;
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
                return NotFound(new { error = "Comment not found." });
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
        public async Task<IActionResult> Create([FromBody] CreateCommentRequest newComment)
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
        public async Task<IActionResult> Update(int commentId, [FromBody] UpdateCommentRequest updateDto)
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