using Microsoft.AspNetCore.Mvc;
using GalaxyWiki.API.DTOs;
using GalaxyWiki.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace GalaxyWiki.API.Controllers
{
    [Route("api/revision")]
    [ApiController]
    public class RevisionsController(ContentRevisionService revisionService, NHibernate.ISession session) : ControllerBase
    {
        private readonly ContentRevisionService _revisionService = revisionService;
        private readonly NHibernate.ISession _session = session;

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var revision = await _revisionService.GetRevisionByIdAsync(id);
            if (revision == null)
                return NotFound(new { error = "Revision not found." });

            return Ok(new
            {
                revision.Content,
                revision.CreatedAt,
                CelestialBodyName = revision.CelestialBody.BodyName,
                AuthorDisplayName = revision.Author.DisplayName
            });
        }

        [HttpGet("by-name/{celestialBodyPath}")]
        public async Task<IActionResult> GetByCelestialBody(string celestialBodyPath)
        {
            var revisions = await _revisionService.GetRevisionsByCelestialBodyAsync(celestialBodyPath);

            if (revisions == null || !revisions.Any())
                return NotFound(new { error = "No revisions found for the specified celestial body." });

            return Ok(revisions.Select(r => new
            {
                r.Id,
                r.Content,
                r.CreatedAt,
                AuthorDisplayName = r.Author.DisplayName
            }));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateRevisionRequest request)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                var authorId = User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(authorId))
                {
                    return Unauthorized(new { error = "Invalid token. Author ID missing." });
                }

                var revision = await _revisionService.CreateRevisionAsync(request, authorId);

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = revision.Id }, new
                {
                    revision.Id,
                    revision.CreatedAt,
                    revision.Content
                });
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { error = e.Message });
            }
        }
    }
}
