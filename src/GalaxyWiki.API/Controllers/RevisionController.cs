using Microsoft.AspNetCore.Mvc;
using GalaxyWiki.API.DTO;
using GalaxyWiki.API.Services;
using Microsoft.AspNetCore.Authorization;
using GalaxyWiki.Core.Enums;
using System.Security.Claims;

namespace GalaxyWiki.Api.Controllers
{
    [Route("api/revision")]
    [ApiController]
    public class RevisionsController(ContentRevisionService revisionService, AuthService authService, NHibernate.ISession session) : ControllerBase
    {
        private readonly ContentRevisionService _revisionService = revisionService;
        private readonly AuthService _authService = authService;
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
            var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (await _authService.CheckUserHasAccessRight([UserRole.Admin], authorId) == false)
            {
                return StatusCode(403, new { error = "You do not have access to perform this action." });
            }

            var revision = await _revisionService.CreateRevisionAsync(request, authorId);

            return CreatedAtAction(nameof(GetById), new { id = revision.Id }, new
            {
                revision.Id,
                revision.CreatedAt,
                revision.Content
            });
        }
    }
}
