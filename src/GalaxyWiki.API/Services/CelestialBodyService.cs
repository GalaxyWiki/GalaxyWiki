using GalaxyWiki.API.DTOs;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;
using GalaxyWiki.Api.Repositories;

namespace GalaxyWiki.API.Services
{
    public class CelestialBodyService
    {
        private readonly CelestialBodyRepository _celestialBodyRepository;
        private readonly AuthService _authService;
        private readonly BodyTypeRepository _bodyTypeRepository;
        public CelestialBodyService(CelestialBodyRepository celestialBodyRepository, AuthService authService, BodyTypeRepository bodyTypesRepository)
        {
            _celestialBodyRepository = celestialBodyRepository;
            _authService = authService;
            _bodyTypeRepository = bodyTypesRepository;
        }

        public async Task<IEnumerable<CelestialBodies>> GetAll()
        {
            return await _celestialBodyRepository.GetAll();
        }

        public async Task<CelestialBodies?> GetById(int id)
        {
            return await _celestialBodyRepository.GetById(id);
        }

        public async Task<CelestialBodies?> GetOrbitsById(int id)
        {
            return await _celestialBodyRepository.GetOrbitsById(id);
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
    }
}
