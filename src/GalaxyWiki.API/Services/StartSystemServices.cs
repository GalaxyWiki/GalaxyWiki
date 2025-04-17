using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;
using GalaxyWiki.API.Repositories;
using GalaxyWiki.API.DTOs;

namespace GalaxyWiki.API.Services
{
    public class StarSystemService : IStarSystemService
    {
        private readonly IAuthService _authService;
        private readonly IStarSystemRepository _starSystemRepository;
        private readonly ICelestialBodyRepository _celestialBodyRepository;

        public StarSystemService(IAuthService authService, IStarSystemRepository starSystemRepository, ICelestialBodyRepository celestialBodyRepository)
        {
            _authService = authService;
            _starSystemRepository = starSystemRepository;
            _celestialBodyRepository = celestialBodyRepository;
        }

        public async Task<IEnumerable<StarSystems>> getAll()
        {
            return await _starSystemRepository.GetAll();
        }

        public async Task<StarSystems> GetStarSystemById(int id)
        {
            return await _starSystemRepository.GetById(id);
        }

        public async Task<IEnumerable<CelestialBodies>> GetCelestialBodiesForStarSystemById(int id)
        {
            var starSystem = await _starSystemRepository.GetById(id);
            if (starSystem == null)
                throw new StarSystemDoesNotExist("Star system not found.");

            return await _celestialBodyRepository.GetCelestialBodiesOrbitingThisId(starSystem.CenterCb.Id);
        }

        public async Task<StarSystems> CreateStarSystem(CreateStarSystemRequest request, string authorId)
        {
            if (await _authService.CheckUserHasAccessRight([UserRole.Admin], authorId) == false)
            {
                throw new UserDoesNotHaveAccess("You do not have access to perform this action.");
            }

            var centerCb = await _celestialBodyRepository.GetById(request.CenterCbId);

            if (centerCb == null)
                throw new CelestialBodyDoesNotExist("Center celestial body not found.");

            var starSystem = new StarSystems
            {
                Name = request.Name,
                CenterCb = centerCb
            };

            await _starSystemRepository.Create(starSystem);

            return starSystem;
        }

        public async Task<StarSystems> UpdateStarSystem(int starSystemId, UpdateStarSystemRequest request, string authorId)
        {
            if (await _authService.CheckUserHasAccessRight([UserRole.Admin], authorId) == false)
            {
                throw new UserDoesNotHaveAccess("You do not have access to perform this action.");
            }

            var starSystem = await _starSystemRepository.GetById(starSystemId);

            if (starSystem == null)
                throw new StarSystemDoesNotExist("Star system not found.");


            if (request.CenterCbId.HasValue)
            {
                var centerCb = await _celestialBodyRepository.GetById(request.CenterCbId.Value);

                if (centerCb == null)
                    throw new CelestialBodyDoesNotExist("Center celestial body not found.");

                starSystem.CenterCb = centerCb;
            }

            starSystem.Name = request.Name;

            await _starSystemRepository.Update(starSystem);

            return starSystem;
        }

        public async Task DeleteStarSystem(int starSystemId, string authorId)
        {
            if (await _authService.CheckUserHasAccessRight([UserRole.Admin], authorId) == false)
            {
                throw new UserDoesNotHaveAccess("You do not have access to perform this action.");
            }

            var starSystem = await _starSystemRepository.GetById(starSystemId);

            if (starSystem == null)
                throw new StarSystemDoesNotExist("Star system not found.");

            await _starSystemRepository.Delete(starSystem);
        }
    }
}