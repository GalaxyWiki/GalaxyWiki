using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GalaxyWiki.Application.DTO;
using GalaxyWiki.Application.Services;
using GalaxyWiki.Core.Exceptions;

namespace GalaxyWiki.Api.Controllers
{
    [Route("api/revision")]
    [ApiController]
    public class RevisionsController : ControllerBase
    {
        private readonly ContentRevisionService _revisionService;

        public RevisionsController(ContentRevisionService revisionService)
        {
            _revisionService = revisionService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var revision = await _revisionService.GetRevisionByIdAsync(id);
            if (revision == null)
                return NotFound(new { error = "Revision not found." });

            return Ok(new
            {
                revision.RevisionId,
                revision.Content,
                revision.CreatedAt,
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
                r.RevisionId,
                r.Content,
                r.CreatedAt,
                AuthorDisplayName = r.Author.DisplayName
            }));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRevisionRequest request)
        {
            try
            {
                var authorId = User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(authorId))
                {
                    return Unauthorized(new { error = "Invalid token. Author ID missing." });
                }

                var revision = await _revisionService.CreateRevisionAsync(request, authorId);

                return CreatedAtAction(nameof(GetById), new { id = revision.RevisionId }, new
                {
                    revision.RevisionId,
                    revision.CreatedAt,
                    revision.Content
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}
