using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using GalaxyWiki.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using GalaxyWiki.API.DTOs;

namespace GalaxyWiki.Api.Controllers
{
    [Route("api/celestial-body")]
    [ApiController]
    public class CelestialBodyController(NHibernate.ISession session) : ControllerBase
    {
        private readonly NHibernate.ISession _session = session;

        // GET: api/celestial-body
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var celestialBodies = await _session.Query<CelestialBodies>().ToListAsync();
            return Ok(celestialBodies);
        }

        // GET: api/celestial-body/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var celestialBody = await _session.Query<CelestialBodies>()
                .Where(cb => cb.Id == id)
                .Select(cb => new
                {
                    cb.Id,
                    cb.BodyName,
                    Orbits = cb.Orbits,
                    cb.BodyType,
                })
                .FirstOrDefaultAsync();

            if (celestialBody == null)
                return NotFound(new { error = "Celestial body not found." });

            return Ok(celestialBody);
        }

        // GET: api/celestial-body/{id}/orbits
        [HttpGet("{id}/orbits")]
        public async Task<IActionResult> GetOrbits(int id)
        {
            var celestialBody = await _session.Query<CelestialBodies>()
                .Where(cb => cb.Id == id)
                .Select(cb => new { Orbits = cb.Orbits })
                .FirstOrDefaultAsync();

            if (celestialBody == null)
                return NotFound(new { error = "Celestial body not found." });

            return Ok(celestialBody);
        }

        // POST: api/celestial-body
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CelestialBodyCreateRequest request)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                if (!request.BodyTypeId.HasValue)
                    return BadRequest(new { error = "Body type ID is required." });

                var bodyType = await _session.Query<BodyTypes>()
                    .FirstOrDefaultAsync(bt => bt.Id == request.BodyTypeId.Value);

                if (bodyType == null)
                    return BadRequest(new { error = "Invalid body type ID." });

                CelestialBodies? orbits = null;
                if (request.OrbitsId.HasValue)
                {
                    orbits = await _session.GetAsync<CelestialBodies>(request.OrbitsId.Value);
                    if (orbits == null)
                        return BadRequest(new { error = "Invalid orbits ID." });
                }

                var celestialBody = new CelestialBodies
                {
                    BodyName = request.BodyName,
                    Orbits = orbits,
                    BodyType = bodyType.Id
                };

                await _session.SaveAsync(celestialBody);
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = celestialBody.Id }, celestialBody);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PUT: api/celestial-body/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] CelestialBodyUpdateRequest request)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                var celestialBody = await _session.GetAsync<CelestialBodies>(id);
                if (celestialBody == null)
                    return NotFound(new { error = "Celestial body not found." });

                if (!request.BodyTypeId.HasValue)
                    return BadRequest(new { error = "Body type ID is required." });

                var bodyType = await _session.Query<BodyTypes>()
                    .FirstOrDefaultAsync(bt => bt.Id == request.BodyTypeId.Value);

                if (bodyType == null)
                    return BadRequest(new { error = "Invalid body type ID." });

                CelestialBodies? orbits = null;
                if (request.OrbitsId.HasValue)
                {
                    orbits = await _session.GetAsync<CelestialBodies>(request.OrbitsId.Value);
                    if (orbits == null)
                        return BadRequest(new { error = "Invalid orbits ID." });
                }

                celestialBody.BodyName = request.BodyName;
                celestialBody.Orbits = orbits;
                celestialBody.BodyType = bodyType.Id;

                await _session.UpdateAsync(celestialBody);
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // DELETE: api/celestial-body/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                // Get the celestial body and its children
                var celestialBody = await _session.Query<CelestialBodies>()
                    .Where(cb => cb.Id == id)
                    .FirstOrDefaultAsync();

                if (celestialBody == null)
                    return NotFound(new { error = "Celestial body not found." });

                // Get all celestial bodies that orbit this one
                var children = await _session.Query<CelestialBodies>()
                    .Where(cb => cb.Orbits != null && cb.Orbits.Id == id)
                    .ToListAsync();

                // Delete all children first
                foreach (var child in children)
                {
                    await _session.DeleteAsync(child);
                }

                // Delete the celestial body itself
                await _session.DeleteAsync(celestialBody);
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
} 