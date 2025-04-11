using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using GalaxyWiki.Core.Entities;

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

            // var celestialBodies = await _session.Query<CelestialBody>()
            //     .Select(cb => new
            //     {
            //         cb.Id,
            //         cb.Name,
            //         cb.Orbits,
            //         cb.BodyType
            //     })
            //     .ToListAsync();

            return Ok(celestialBodies);
        }

        // GET: api/celestial-body/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var celestialBody = await _session.Query<CelestialBodies>()
                .Where(cb => cb.CelestialBodyId == id)
                .Select(cb => new
                {
                    cb.CelestialBodyId,
                    cb.BodyName,
                    OrbitId = cb.Orbits,
                    BodyType = cb.BodyType,
                    // Children = cb.Comments.Count,
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
                .Where(cb => cb.CelestialBodyId == id)
                .Select(cb => new { OrbitId = cb.Orbits != null ? cb.Orbits : (int?)null })
                .FirstOrDefaultAsync();

            if (celestialBody == null)
                return NotFound(new { error = "Celestial body not found." });

            return Ok(celestialBody);
        }

        // POST: api/celestial-body
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CelestialBodyCreateRequest request)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                var bodyType = await _session.Query<BodyTypes>()
                    .FirstOrDefaultAsync(bt => bt.Id == request.Id);

                if (bodyType == null)
                    return BadRequest(new { error = "Invalid body type ID." });

                var celestialBody = new CelestialBodies
                {
                    BodyName = request.Name,
                    Orbits = request.OrbitsId,
                    BodyType = bodyType.Id
                };

                await _session.SaveAsync(celestialBody);
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = celestialBody.CelestialBodyId }, celestialBody);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PUT: api/celestial-body/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CelestialBodyUpdateRequest request)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                var celestialBody = await _session.GetAsync<CelestialBodies>(id);
                if (celestialBody == null)
                    return NotFound(new { error = "Celestial body not found." });

                var bodyType = await _session.Query<BodyTypes>()
                    .FirstOrDefaultAsync(bt => bt.Id == request.Id);

                if (bodyType == null)
                    return BadRequest(new { error = "Invalid body type ID." });

                celestialBody.BodyName = request.Name;
                celestialBody.Orbits = request.OrbitsId;
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
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                // Get the celestial body and its children
                var celestialBody = await _session.Query<CelestialBodies>()
                    .Where(cb => cb.CelestialBodyId == id)
                    .FirstOrDefaultAsync();

                if (celestialBody == null)
                    return NotFound(new { error = "Celestial body not found." });

                // Get all celestial bodies that orbit this one
                var children = await _session.Query<CelestialBodies>()
                    .Where(cb => cb.Orbits != null && cb.Orbits == id)
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

    public class CelestialBodyCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public int? OrbitsId { get; set; }
    }

    public class CelestialBodyUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Id { get; set; }
        public int? OrbitsId { get; set; }
    }
} 