using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NHibernate.Linq;
using GalaxyWiki.Core.Entities;

namespace GalaxyWiki.Api.Controllers
{
    [Route("api/star-system")]
    [ApiController]
    public class StarSystemController : ControllerBase
    {
        private readonly NHibernate.ISession _session;

        public StarSystemController(NHibernate.ISession session)
        {
            _session = session;
        }

        // GET: api/star-system
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var starSystems = await _session.Query<StarSystems>()
                .Select(ss => new
                {
                    ss.Id,
                    ss.Name,
                    CenterCelestialBody = new
                    {
                        ss.CenterCb.Id,
                        ss.CenterCb.BodyName,
                        ss.CenterCb.BodyType
                    }
                })
                .ToListAsync();
            
            return Ok(starSystems);
        }

        // GET: api/star-system/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var starSystem = await _session.Query<StarSystems>()
                .Where(ss => ss.Id == id)
                .Select(ss => new
                {
                    ss.Id,
                    ss.Name,
                    CenterCelestialBody = new
                    {
                        ss.CenterCb.Id,
                        ss.CenterCb.BodyName,
                        ss.CenterCb.BodyType
                    }
                })
                .FirstOrDefaultAsync();

            if (starSystem == null)
                return NotFound(new { error = "Star system not found." });

            return Ok(starSystem);
        }

        // GET: api/star-system/{id}/celestial-bodies
        [HttpGet("{id}/celestial-bodies")]
        public async Task<IActionResult> GetCelestialBodies(int id)
        {
            var starSystem = await _session.GetAsync<StarSystems>(id);
            if (starSystem == null)
                return NotFound(new { error = "Star system not found." });

            // Get all celestial bodies that orbit the center celestial body
            var celestialBodies = await _session.Query<CelestialBodies>()
                .Where(cb => cb.Orbits != null && cb.Orbits.Id == starSystem.CenterCb.Id)
                .Select(cb => new
                {
                    cb.Id,
                    cb.BodyName,
                    cb.BodyType,
                    Orbits = new
                    {
                        cb.Orbits.Id,
                        cb.Orbits.BodyName
                    }
                })
                .ToListAsync();

            return Ok(celestialBodies);
        }

        // POST: api/star-system
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] StarSystemCreateRequest request)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                // Check if the center celestial body exists
                var centerCb = await _session.GetAsync<CelestialBodies>(request.CenterCbId);
                if (centerCb == null)
                    return BadRequest(new { error = "Center celestial body not found." });

                var starSystem = new StarSystems
                {
                    Name = request.Name,
                    CenterCb = centerCb
                };

                await _session.SaveAsync(starSystem);
                await transaction.CommitAsync();

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
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PUT: api/star-system/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] StarSystemUpdateRequest request)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                var starSystem = await _session.GetAsync<StarSystems>(id);
                if (starSystem == null)
                    return NotFound(new { error = "Star system not found." });

                // Check if the new center celestial body exists
                if (request.CenterCbId.HasValue)
                {
                    var centerCb = await _session.GetAsync<CelestialBodies>(request.CenterCbId.Value);
                    if (centerCb == null)
                        return BadRequest(new { error = "Center celestial body not found." });
                    
                    starSystem.CenterCb = centerCb;
                }

                starSystem.Name = request.Name;

                await _session.UpdateAsync(starSystem);
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // DELETE: api/star-system/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                var starSystem = await _session.GetAsync<StarSystems>(id);
                if (starSystem == null)
                    return NotFound(new { error = "Star system not found." });

                await _session.DeleteAsync(starSystem);
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

    public class StarSystemCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public int CenterCbId { get; set; }
    }

    public class StarSystemUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public int? CenterCbId { get; set; }
    }
} 