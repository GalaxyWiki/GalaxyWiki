using GalaxyWiki.API.DTOs;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;
using GalaxyWiki.Api.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Services
{
    public class CelestialBodyService(CelestialBodyRepository celestialBodyRepository, AuthService authService, BodyTypeRepository bodyTypesRepository)
    {
        private readonly CelestialBodyRepository _celestialBodyRepository = celestialBodyRepository;
        private readonly AuthService _authService = authService;
        private readonly BodyTypeRepository _bodyTypeRepository = bodyTypesRepository;

        public async Task<IEnumerable<(CelestialBodies CelestialBody, BodyTypes? BodyType)>> GetAll()
        {
            var celestialBodies = await _celestialBodyRepository.GetAll();
            var result = new List<(CelestialBodies CelestialBody, BodyTypes? BodyType)>();
            
            foreach (var celestialBody in celestialBodies)
            {
                var bodyType = await _bodyTypeRepository.GetById(celestialBody.BodyType);
                result.Add((celestialBody, bodyType));
            }
            
            return result;
        }

        public async Task<PagedResult<(CelestialBodies CelestialBody, BodyTypes? BodyType)>> GetAllPaginated(PaginationParameters parameters)
        {
            var (celestialBodies, totalCount) = await _celestialBodyRepository.GetAllPaginated(parameters);
            var result = new List<(CelestialBodies CelestialBody, BodyTypes? BodyType)>();
            
            foreach (var celestialBody in celestialBodies)
            {
                var bodyType = await _bodyTypeRepository.GetById(celestialBody.BodyType);
                result.Add((celestialBody, bodyType));
            }
            
            return new PagedResult<(CelestialBodies CelestialBody, BodyTypes? BodyType)>
            {
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount,
                Items = result
            };
        }

        public async Task<(CelestialBodies? CelestialBody, BodyTypes? BodyType)> GetById(int id)
        {
            var celestialBody = await _celestialBodyRepository.GetById(id);
            if (celestialBody == null)
                return (null, null);
                
            var bodyType = await _bodyTypeRepository.GetById(celestialBody.BodyType);
            return (celestialBody, bodyType);
        }

        public async Task<(CelestialBodies? CelestialBody, BodyTypes? BodyType)> GetOrbitsById(int id)
        {
            var celestialBody = await _celestialBodyRepository.GetOrbitsById(id);
            if (celestialBody == null)
                return (null, null);
            var bodyType = await _bodyTypeRepository.GetById(celestialBody.BodyType);
            return (celestialBody, bodyType);
        }

        public async Task<CelestialBodies> CreateCelestialBody(CreateCelestialBodyRequest request, string authorId)
        {
            if (await _authService.CheckUserHasAccessRight([UserRole.Admin], authorId) == false)
            {
                throw new UserDoesNotHaveAccess("You do not have access to perform this action.");
            }
            
            if (!request.BodyTypeId.HasValue)
                throw new RequestBodyIsInvalid("Body type ID is required.");

            var bodyType = await _bodyTypeRepository.GetById(request.BodyTypeId.Value);

            if (bodyType == null)
                throw new BodyTypeDoesNotExist("Invalid body type ID." );

            CelestialBodies? orbits = null;
            
            if (request.OrbitsId.HasValue)
            {
                orbits = await _celestialBodyRepository.GetById(request.OrbitsId.Value);
                if (orbits == null)
                    throw new CelestialBodyDoesNotExist("Invalid orbits ID.");
            }

            var celestialBody = new CelestialBodies
            {
                BodyName = request.BodyName,
                Orbits = orbits,
                BodyType = bodyType.Id
            };

            await _celestialBodyRepository.Create(celestialBody);

            return celestialBody;
        }

        public async Task<CelestialBodies> UpdateCelestialBody(int celestialBodyId, UpdateCelestialBodyRequest request, string authorId)
        {
            if (await _authService.CheckUserHasAccessRight([UserRole.Admin], authorId) == false)
            {
                throw new UserDoesNotHaveAccess("You do not have access to perform this action.");
            }

            var celestialBody = await _celestialBodyRepository.GetById(celestialBodyId);

            if (celestialBody == null)
                throw new CelestialBodyDoesNotExist("Celestial body does not exist.");

            if (!request.BodyTypeId.HasValue)
                throw new RequestBodyIsInvalid("Body type ID is required.");

            var bodyType = await _bodyTypeRepository.GetById(request.BodyTypeId.Value);

            if (bodyType == null)
                throw new BodyTypeDoesNotExist("Invalid body type ID." );


            CelestialBodies? orbits = null;
            
            if (request.OrbitsId.HasValue)
            {
                orbits = await _celestialBodyRepository.GetById(request.OrbitsId.Value);
                if (orbits == null)
                    throw new CelestialBodyDoesNotExist("Invalid orbits ID.");
            }

            celestialBody.BodyName = request.BodyName;
            celestialBody.Orbits = orbits;
            celestialBody.BodyType = bodyType.Id;

            await _celestialBodyRepository.Update(celestialBody);

            return celestialBody;
        }

        public async Task DeleteCelestialBody(int celestialBodyId, string authorId)
        {
            if (await _authService.CheckUserHasAccessRight([UserRole.Admin], authorId) == false)
            {
                throw new UserDoesNotHaveAccess("You do not have access to perform this action.");
            }

            // Get the celestial body and its children
            var celestialBody = await _celestialBodyRepository.GetById(celestialBodyId);

            if (celestialBody == null)
                throw new CelestialBodyDoesNotExist("Celestial body does not exist.");

            await _celestialBodyRepository.Delete(celestialBody);
        }

        public async Task<IEnumerable<(CelestialBodies CelestialBody, BodyTypes? BodyType)>> GetChildrenById(int id)
        {
            var celestialBody = await _celestialBodyRepository.GetById(id);
            if (celestialBody == null)
                throw new CelestialBodyDoesNotExist("Celestial body does not exist.");
                
            var children = await _celestialBodyRepository.GetCelestialBodiesOrbitingThisId(id);
            var result = new List<(CelestialBodies CelestialBody, BodyTypes? BodyType)>();
            
            foreach (var child in children)
            {
                var bodyType = await _bodyTypeRepository.GetById(child.BodyType);
                result.Add((child, bodyType));
            }
            
            return result;
        }

        public async Task<PagedResult<(CelestialBodies CelestialBody, BodyTypes? BodyType)>> GetChildrenByIdPaginated(int id, PaginationParameters parameters)
        {
            var celestialBody = await _celestialBodyRepository.GetById(id);
            if (celestialBody == null)
                throw new CelestialBodyDoesNotExist("Celestial body does not exist.");
                
            var (children, totalCount) = await _celestialBodyRepository.GetCelestialBodiesOrbitingThisIdPaginated(id, parameters);
            var result = new List<(CelestialBodies CelestialBody, BodyTypes? BodyType)>();
            
            foreach (var child in children)
            {
                var bodyType = await _bodyTypeRepository.GetById(child.BodyType);
                result.Add((child, bodyType));
            }
            
            return new PagedResult<(CelestialBodies CelestialBody, BodyTypes? BodyType)>
            {
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount,
                Items = result
            };
        }
    }
}
