using Microsoft.AspNetCore.Mvc;
using GalaxyWiki.API.DTOs;
using GalaxyWiki.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GalaxyWiki.API.Controllers
{
    [Route("api/revision")]
    [ApiController]
    public class RevisionsController : ControllerBase
    {
        private readonly ContentRevisionService _contentRevisionService;
        public RevisionsController(ContentRevisionService contentRevisionService)
        {
            _contentRevisionService = contentRevisionService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var revision = await _contentRevisionService.GetRevisionByIdAsync(id);
            if (revision == null)
                return NotFound(new { error = "Revision not found." });

            return Ok(new
            {   
                revision.Id,
                revision.Content,
                revision.CreatedAt,
                CelestialBodyName = revision.CelestialBody.BodyName,
                AuthorDisplayName = revision.Author.DisplayName
            });
        }

        [HttpGet("by-name/{celestialBodyPath}")]
        public async Task<IActionResult> GetByCelestialBody(string celestialBodyPath)
        {
            var revisions = await _contentRevisionService.GetRevisionsByCelestialBodyAsync(celestialBodyPath);

            if (revisions == null || !revisions.Any())
                return NotFound(new { error = "No revisions found for the specified celestial body." });

            return Ok(revisions.Select(r => new
            {
                r.Id,
                r.Content,
                r.CreatedAt,
                CelestialBodyName = r.CelestialBody.BodyName,
                AuthorDisplayName = r.Author.DisplayName
            }));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateRevisionRequest request)
        { 
            var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var revision = await _contentRevisionService.CreateRevision(request, authorId);

            return CreatedAtAction(nameof(GetById), new { id = revision.Id }, new
            {
                revision.Id,
                revision.CreatedAt,
                revision.Content
            });
        }
    }
}
