using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GalaxyWiki.API.DTOs;
using GalaxyWiki.API.Services;
using System.Security.Claims;

namespace GalaxyWiki.API.Controllers
{
    [Route("api/celestial-body")]
    [ApiController]
    public class CelestialBodyController : ControllerBase
    {
        private readonly ICelestialBodyService _celestialBodyService;

        public CelestialBodyController(ICelestialBodyService celestialBodyService)
        {
            _celestialBodyService = celestialBodyService;
        }

        // GET: api/celestial-body
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var results = await _celestialBodyService.GetAll();
            return Ok(results.Select(r => new
            {
                r.CelestialBody.Id,
                r.CelestialBody.BodyName,
                r.CelestialBody.Orbits,
                r.CelestialBody.BodyType,
                BodyTypeName = r.BodyType?.TypeName,
                r.CelestialBody.ActiveRevision
            }));
        }

        // GET: api/celestial-body/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var (CelestialBody, BodyType) = await _celestialBodyService.GetById(id);
            if (CelestialBody == null)
                return NotFound(new { error = "Celestial body not found." });

            return Ok(new
            {
                CelestialBody.Id,
                CelestialBody.BodyName,
                CelestialBody.Orbits,
                CelestialBody.BodyType,
                BodyTypeName = BodyType?.TypeName,
                CelestialBody.ActiveRevision
            });
        }

        // GET: api/celestial-body/{id}/orbits
        [HttpGet("{id}/orbits")]
        public async Task<IActionResult> GetOrbits(int id)
        {

            var (CelestialBody, BodyType) = await _celestialBodyService.GetOrbitsById(id);

            if (CelestialBody == null)
                return NotFound(new { error = "Celestial body not found." });

            return Ok(new
            {
                CelestialBody.Id,
                CelestialBody.BodyName,
                CelestialBody.Orbits,
                CelestialBody.BodyType,
                BodyTypeName = BodyType?.TypeName,
                CelestialBody.ActiveRevision
            });
        }

        // POST: api/celestial-body
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateCelestialBodyRequest request)
        {
            var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var celestialBody = await _celestialBodyService.CreateCelestialBody(request, authorId);

            return CreatedAtAction(nameof(GetById), new { id = celestialBody.Id }, celestialBody);
        }

        // PUT: api/celestial-body/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCelestialBodyRequest request)
        {
            var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var celestialBody = await _celestialBodyService.UpdateCelestialBody(id, request, authorId);

            return Ok(celestialBody);
        }

        // DELETE: api/celestial-body/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _celestialBodyService.DeleteCelestialBody(id, authorId);

            return NoContent();
        }

        // GET: api/celestial-body/{id}/children
        [HttpGet("{id}/children")]
        public async Task<IActionResult> GetChildren(int id, [FromQuery] PaginationParameters? parameters)
        {
            if (parameters is null || (parameters.PageNumber == 0))
            {
                var children = await _celestialBodyService.GetChildrenById(id);

                return Ok(from r in children
                          select new
                          {
                              r.CelestialBody.Id,
                              r.CelestialBody.BodyName,
                              r.CelestialBody.Orbits,
                              r.CelestialBody.BodyType,
                              BodyTypeName = r.BodyType?.TypeName,
                              r.CelestialBody.ActiveRevision
                          });
            }
            else
            {
                var pagedResult = await _celestialBodyService.GetChildrenByIdPaginated(id, parameters);

                var mappedItems = pagedResult.Items.Select(r => new
                {
                    r.CelestialBody.Id,
                    r.CelestialBody.BodyName,
                    r.CelestialBody.Orbits,
                    r.CelestialBody.BodyType,
                    BodyTypeName = r.BodyType?.TypeName,
                    r.CelestialBody.ActiveRevision
                });

                var result = new
                {
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize,
                    TotalCount = pagedResult.TotalCount,
                    TotalPages = pagedResult.TotalPages,
                    Items = mappedItems
                };

                return Ok(result);
            }
        }
    }
}