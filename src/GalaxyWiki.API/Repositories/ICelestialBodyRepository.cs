using GalaxyWiki.Core.Entities;
using GalaxyWiki.API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Repositories
{
    public interface ICelestialBodyRepository
    {
        Task<IEnumerable<CelestialBodies>> GetAll();
        Task<(IEnumerable<CelestialBodies> Items, int TotalCount)> GetAllPaginated(PaginationParameters parameters);
        Task<CelestialBodies?> GetById(int id);
        Task<CelestialBodies?> GetOrbitsById(int id);
        Task<IEnumerable<CelestialBodies>> GetCelestialBodiesOrbitingThisId(int id);
        Task<(IEnumerable<CelestialBodies> Items, int TotalCount)> GetCelestialBodiesOrbitingThisIdPaginated(int id, PaginationParameters parameters);
        Task<CelestialBodies?> GetByName(string celestialBodyPath);
        Task<CelestialBodies> Create(CelestialBodies celestialBody);
        Task<CelestialBodies> Update(CelestialBodies celestialBody);
        Task Delete(CelestialBodies celestialBody);
    }
}