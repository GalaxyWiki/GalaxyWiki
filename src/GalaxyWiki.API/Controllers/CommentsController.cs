using Microsoft.AspNetCore.Mvc;
using GalaxyWiki.Api.DTOs;
using GalaxyWiki.Api.Services;

namespace GalaxyWiki.Api.Controllers
{
    [ApiController]
    [Route("comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // GET /comments
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var comments = _commentService.GetAll();
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // GET /comments/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            try
            {
                var comment = _commentService.GetById(id);
                if (comment == null) return NotFound();
                return Ok(comment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // POST /comments
        [HttpPost]
        public IActionResult Create([FromBody] CreateCommentDto newComment)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var comment = _commentService.Create(newComment);
                return CreatedAtAction(nameof(GetById), new { id = comment.CommentId }, comment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // GET /celestial_bodies/{celestialBodyId}
        [HttpGet("celestial_bodies/{celestialBodyId}")]
        public IActionResult GetByCelestialBody(Guid celestialBodyId)
        {
            try
            {
                var comments = _commentService.GetByCelestialBody(celestialBodyId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // GET /comments/users/{userId}
        [HttpGet("users/{userId}")]
        public IActionResult GetByUser(Guid userId)
        {
            try
            {
                var comments = _commentService.GetByUser(userId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // GET /comments/date-range?startDate={startDate}&endDate={endDate}&celestialBodyId={celestialBodyId}
        // Example: /comments/date-range?startDate=2024-01-01&endDate=2024-12-31&celestialBodyId=123e4567-e89b-12d3-a456-426614174000
        [HttpGet("date-range")]
        public IActionResult GetByDateRange([FromQuery] string startDate, [FromQuery] string endDate, [FromQuery] Guid? celestialBodyId)
        {
            try
            {
                if (!DateTime.TryParse(startDate, out DateTime start) || !DateTime.TryParse(endDate, out DateTime end))
                {
                    return BadRequest("Invalid date format. Please use YYYY-MM-DD format.");
                }

                // Set start time to beginning of day and end time to end of day
                start = start.Date;
                end = end.Date.AddDays(1).AddSeconds(-1);

                var comments = _commentService.GetByDateRange(start, end, celestialBodyId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // PUT /comments/{id}
        [HttpPut("{id}")]
        public IActionResult Update(Guid id, [FromBody] UpdateCommentDto updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updatedComment = _commentService.Update(id, updateDto);
                if (updatedComment == null) return NotFound();
                return Ok(updatedComment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // DELETE /comments/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                var deleted = _commentService.Delete(id);
                if (!deleted) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
