using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GalaxyWiki.API.Services;
using GalaxyWiki.API.DTOs;
using System.Security.Claims;

namespace GalaxyWiki.API.Controllers
{
    [Route("api/star-system")]
    [ApiController]
    public class StarSystemController : ControllerBase
    {
        private readonly StarSystemService _starSystemService;

        public StarSystemController(StarSystemService starSystemService)
        {
            _starSystemService = starSystemService;
        }

        // GET: api/star-system
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var starSystems = await _starSystemService.GetAll();

            return Ok(starSystems.Select(ss => new
            {
                ss.Id,
                ss.Name,
                CenterCelestialBody = new
                {
                    ss.CenterCb.Id,
                    ss.CenterCb.BodyName,
                    ss.CenterCb.BodyType
                }
            }));
        }

        // GET: api/star-system/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var starSystem = await _starSystemService.GetStarSystemById(id);

            if (starSystem == null)
                return NotFound(new { error = "Star system not found." });

            return Ok(new
            {
                starSystem.Id,
                starSystem.Name,
                CenterCelestialBody = new
                {
                    starSystem.CenterCb.Id,
                    starSystem.CenterCb.BodyName,
                    starSystem.CenterCb.BodyType
                }
            });
        }

        // GET: api/star-system/{id}/celestial-bodies
        [HttpGet("{id}/celestial-bodies")]
        public async Task<IActionResult> GetCelestialBodies(int id)
        {
            var celestialBodies = await _starSystemService.GetCelestialBodiesForStarSystemById(id);
            
            return Ok(celestialBodies.Select(cb => new
            {
                cb.Id,
                cb.BodyName,
                cb.BodyType,
                Orbits = new
                {
                    cb.Orbits?.Id,
                    cb.Orbits?.BodyName
                }
            }));
        }

        // POST: api/star-system
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateStarSystemRequest request)
        {
            var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var starSystem = await _starSystemService.CreateStarSystem(request, authorId);

            return CreatedAtAction(nameof(GetById), new { id = starSystem.Id }, new
            {
                starSystem.Id,
                starSystem.Name,
                CenterCelestialBody = new
                {
                    starSystem.CenterCb.Id,
                    starSystem.CenterCb.BodyName,
                    starSystem.CenterCb.BodyType
                }
            });

        }

        // PUT: api/star-system/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStarSystemRequest request)
        {
            var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var starSystem = await _starSystemService.UpdateStarSystem(id, request, authorId);

            return Ok(new
            {
                starSystem.Id,
                starSystem.Name,
                CenterCelestialBody = new
                {
                    starSystem.CenterCb.Id,
                    starSystem.CenterCb.BodyName,
                    starSystem.CenterCb.BodyType
                }
            });
        }

        // DELETE: api/star-system/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var authorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _starSystemService.DeleteStarSystem(id, authorId);

            return NoContent();
        }
    }
} 