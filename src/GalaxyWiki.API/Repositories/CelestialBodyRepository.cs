using GalaxyWiki.Core.Entities;
using GalaxyWiki.API.DTOs;
using NHibernate.Linq;
using ISession = NHibernate.ISession;

namespace GalaxyWiki.Api.Repositories
{
    public class CelestialBodyRepository(ISession session)
    {
        private readonly ISession _session = session;

        public async Task<IEnumerable<CelestialBodies>> GetAll()
        {
            return await _session.Query<CelestialBodies>().ToListAsync();
        }

        public async Task<(IEnumerable<CelestialBodies> Items, int TotalCount)> GetAllPaginated(PaginationParameters parameters)
        {
            var query = _session.Query<CelestialBodies>();
            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
            
            return (items, totalCount);
        }

        public async Task<CelestialBodies?> GetById(int id)
        {
            return await _session.GetAsync<CelestialBodies>(id);
        }

        public async Task<CelestialBodies?> GetOrbitsById(int id)
        {
            return await _session.Query<CelestialBodies>()
                .Where(cb => cb.Id == id)
                .Select(cb => cb.Orbits)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CelestialBodies>> GetCelestialBodiesOrbitingThisId(int id)
        {
            return await _session.Query<CelestialBodies>()
                .Where(cb => cb.Orbits != null && cb.Orbits.Id == id)
                .ToListAsync();
        }

        public async Task<(IEnumerable<CelestialBodies> Items, int TotalCount)> GetCelestialBodiesOrbitingThisIdPaginated(int id, PaginationParameters parameters)
        {
            var query = _session.Query<CelestialBodies>()
                .Where(cb => cb.Orbits != null && cb.Orbits.Id == id);
                
            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
            
            return (items, totalCount);
        }

        public async Task<CelestialBodies?> GetByName(string celestialBodyPath)
        {
            return await _session.Query<CelestialBodies>()
                                .FirstOrDefaultAsync(cb => cb.BodyName == celestialBodyPath);
        }

        public async Task<CelestialBodies> Create(CelestialBodies celestialBody)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                await _session.SaveAsync(celestialBody);
                transaction.Commit();
                return celestialBody; 
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<CelestialBodies> Update(CelestialBodies celestialBody)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                await _session.UpdateAsync(celestialBody);
                transaction.Commit();
                return celestialBody; 
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task Delete(CelestialBodies celestialBody)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                // Delete all children first
                var children = await GetCelestialBodiesOrbitingThisId(celestialBody.Id);
                foreach (var child in children)
                {
                    await _session.DeleteAsync(child);
                }

                await _session.DeleteAsync(celestialBody);
                transaction.Commit();
                return;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
} 