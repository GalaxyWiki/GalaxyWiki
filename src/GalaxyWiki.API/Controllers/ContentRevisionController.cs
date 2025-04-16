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
        private readonly IContentRevisionService _contentRevisionService;
        public RevisionsController(IContentRevisionService contentRevisionService)
        {
            _contentRevisionService = contentRevisionService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var revision = await _contentRevisionService.GetRevisionByIdAsync(id);

            if (revision == null) 
                return NotFound();

            var result = new ContentRevisionDto
            {
                Id = revision.Id,
                Content = revision.Content,
                CelestialBodyName = revision.CelestialBody.BodyName,
                AuthorDisplayName = revision.Author.DisplayName
            };

            return Ok(result);
        }

        [HttpGet("by-name/{celestialBodyPath}")]
        public async Task<IActionResult> GetByCelestialBody(string celestialBodyPath)
        {
            var revisions = await _contentRevisionService.GetRevisionsByCelestialBodyAsync(celestialBodyPath);

            if (revisions == null || !revisions.Any())
            {
                return NotFound(new ContentRevisionDto());
            }

            return Ok(revisions.Select(r => new ContentRevisionDto
            {
                Id = r.Id,
                Content = r.Content,
                CelestialBodyName = r.CelestialBody.BodyName,
                AuthorDisplayName = r.Author.DisplayName,
                CreatedAt = r.CreatedAt
            }));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateRevisionRequest request)
        { 
            var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var revision = await _contentRevisionService.CreateRevision(request, authorId!);

            var dto = new ContentRevisionDto
            {
                Id = revision.Id,
                Content = revision.Content,
                CreatedAt = revision.CreatedAt,
                CelestialBodyName = revision.CelestialBody.BodyName,
                AuthorDisplayName = revision.Author.DisplayName
            };

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
    }
}
