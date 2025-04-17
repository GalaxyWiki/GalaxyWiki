using GalaxyWiki.API.DTOs;
using GalaxyWiki.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Services
{
    public interface IStarSystemService
    {
        Task<IEnumerable<StarSystems>> getAll();
        Task<StarSystems> GetStarSystemById(int id);
        Task<IEnumerable<CelestialBodies>> GetCelestialBodiesForStarSystemById(int id);
        Task<StarSystems> CreateStarSystem(CreateStarSystemRequest request, string authorId);
        Task<StarSystems> UpdateStarSystem(int starSystemId, UpdateStarSystemRequest request, string authorId);
        Task DeleteStarSystem(int starSystemId, string authorId);
    }
}