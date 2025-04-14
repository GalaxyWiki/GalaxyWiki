using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using GalaxyWiki.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using GalaxyWiki.API.DTOs;
using GalaxyWiki.API.Services;
using System.Security.Claims;

namespace GalaxyWiki.Api.Controllers
{
    [Route("api/celestial-body")]
    [ApiController]
    public class CelestialBodyController : ControllerBase
    {
        private readonly CelestialBodyService _celestialBodyService;

        public CelestialBodyController(CelestialBodyService celestialBodyService)
        {
            _celestialBodyService = celestialBodyService;
        }

        // GET: api/celestial-body
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var celestialBodies = await _celestialBodyService.GetAll();
            return Ok(celestialBodies);
        }

        // GET: api/celestial-body/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var celestialBody = await _celestialBodyService.GetById(id);
            if (celestialBody == null) 
                return NotFound(new { error = "Celestial body not found." });

            return Ok(new
            {
                celestialBody.Id,
                celestialBody.BodyName,
                celestialBody.Orbits,
                celestialBody.BodyType
            });
        }

        // GET: api/celestial-body/{id}/orbits
        [HttpGet("{id}/orbits")]
        public async Task<IActionResult> GetOrbits(int id)
        {
            
            var celestialBody = await _celestialBodyService.GetOrbitsById(id);

            if (celestialBody == null)
                return NotFound(new { error = "Celestial body not found." });

            return Ok(celestialBody);
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
    }
} 