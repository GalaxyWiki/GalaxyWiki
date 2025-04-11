using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using GalaxyWiki.Models;
using NHibernate;
using NHibernate.Linq;

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
            var celestialBodies = await _session.Query<CelestialBody>()
                .Select(cb => new
                {
                    cb.CelestialBodyId,
                    cb.Name,
                    OrbitId = cb.Orbits != null ? cb.Orbits.CelestialBodyId : (int?)null,
                    BodyType = cb.BodyType.Type
                })
                .ToListAsync();

            return Ok(celestialBodies);
        }

        // GET: api/celestial-body/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var celestialBody = await _session.Query<CelestialBody>()
                .Where(cb => cb.CelestialBodyId == id)
                .Select(cb => new
                {
                    cb.CelestialBodyId,
                    cb.Name,
                    OrbitId = cb.Orbits != null ? cb.Orbits.CelestialBodyId : (int?)null,
                    BodyType = cb.BodyType.Type,
                    Children = cb.Comments.Count,
                    LastRevision = cb.ContentRevisions
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => new { r.RevisionId, r.Content, r.CreatedAt })
                        .FirstOrDefault()
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
            var celestialBody = await _session.Query<CelestialBody>()
                .Where(cb => cb.CelestialBodyId == id)
                .Select(cb => new { OrbitId = cb.Orbits != null ? cb.Orbits.CelestialBodyId : (int?)null })
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
                var bodyType = await _session.Query<BodyType>()
                    .FirstOrDefaultAsync(bt => bt.BodyTypeId == request.BodyTypeId);

                if (bodyType == null)
                    return BadRequest(new { error = "Invalid body type ID." });

                CelestialBody? orbits = null;
                if (request.OrbitsId.HasValue)
                {
                    orbits = await _session.GetAsync<CelestialBody>(request.OrbitsId.Value);
                    if (orbits == null)
                        return BadRequest(new { error = "Invalid orbits ID." });
                }

                var celestialBody = new CelestialBody
                {
                    Name = request.Name,
                    Orbits = orbits,
                    BodyType = bodyType
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
                var celestialBody = await _session.GetAsync<CelestialBody>(id);
                if (celestialBody == null)
                    return NotFound(new { error = "Celestial body not found." });

                var bodyType = await _session.Query<BodyType>()
                    .FirstOrDefaultAsync(bt => bt.BodyTypeId == request.BodyTypeId);

                if (bodyType == null)
                    return BadRequest(new { error = "Invalid body type ID." });

                CelestialBody? orbits = null;
                if (request.OrbitsId.HasValue)
                {
                    orbits = await _session.GetAsync<CelestialBody>(request.OrbitsId.Value);
                    if (orbits == null)
                        return BadRequest(new { error = "Invalid orbits ID." });
                }

                celestialBody.Name = request.Name;
                celestialBody.Orbits = orbits;
                celestialBody.BodyType = bodyType;

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
                var celestialBody = await _session.Query<CelestialBody>()
                    .Where(cb => cb.CelestialBodyId == id)
                    .FirstOrDefaultAsync();

                if (celestialBody == null)
                    return NotFound(new { error = "Celestial body not found." });

                // Get all celestial bodies that orbit this one
                var children = await _session.Query<CelestialBody>()
                    .Where(cb => cb.Orbits != null && cb.Orbits.CelestialBodyId == id)
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
        public int BodyTypeId { get; set; }
        public int? OrbitsId { get; set; }
    }

    public class CelestialBodyUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public int BodyTypeId { get; set; }
        public int? OrbitsId { get; set; }
    }
} 