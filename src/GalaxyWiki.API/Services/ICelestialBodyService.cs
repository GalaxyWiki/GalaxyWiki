using GalaxyWiki.API.DTOs;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Services
{
    public interface ICelestialBodyService
    {
        Task<IEnumerable<(CelestialBodies CelestialBody, BodyTypes? BodyType)>> GetAll();
        Task<PagedResult<(CelestialBodies CelestialBody, BodyTypes? BodyType)>> GetAllPaginated(PaginationParameters parameters);
        Task<(CelestialBodies? CelestialBody, BodyTypes? BodyType)> GetById(int id);
        Task<(CelestialBodies? CelestialBody, BodyTypes? BodyType)> GetOrbitsById(int id);
        Task<CelestialBodies> CreateCelestialBody(CreateCelestialBodyRequest request, string authorId);
        Task<CelestialBodies> UpdateCelestialBody(int celestialBodyId, UpdateCelestialBodyRequest request, string authorId);
        Task DeleteCelestialBody(int celestialBodyId, string authorId);
        Task<IEnumerable<(CelestialBodies CelestialBody, BodyTypes? BodyType)>> GetChildrenById(int id);
        Task<PagedResult<(CelestialBodies CelestialBody, BodyTypes? BodyType)>> GetChildrenByIdPaginated(int id, PaginationParameters parameters);
    }
} 