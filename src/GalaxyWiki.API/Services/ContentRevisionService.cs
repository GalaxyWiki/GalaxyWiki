using GalaxyWiki.API.DTOs;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;
using GalaxyWiki.Api.Repositories;

namespace GalaxyWiki.API.Services
{
    public class ContentRevisionService
    {
        private AuthService _authService;
        private readonly ContentRevisionRepository _contentRevisionRepository;
        private readonly CelestialBodyRepository _celestialBodyRepository;
        private readonly UserRepository _userRepository;

        public ContentRevisionService(AuthService authService, ContentRevisionRepository contentRevisionRepository, CelestialBodyRepository celestialBodyRepository, UserRepository userRepository)
        {
            _authService = authService;
            _contentRevisionRepository = contentRevisionRepository;
            _celestialBodyRepository = celestialBodyRepository;
            _userRepository = userRepository;
        }
        public async Task<ContentRevisions?> GetRevisionByIdAsync(int id)
        {
            return await _contentRevisionRepository.GetById(id);
        }

        public async Task<IEnumerable<ContentRevisions>> GetRevisionsByCelestialBodyAsync(string celestialBodyPath)
        {
            var celestialBody = await _celestialBodyRepository.GetByName(celestialBodyPath);

            if (celestialBody == null)
                throw new CelestialBodyDoesNotExist("Celestial body not found.");

            return await _contentRevisionRepository.GetByCelestialBodyId(celestialBody.Id);
        }

        public async Task<ContentRevisions> CreateRevision(CreateRevisionRequest request, string authorId)
        {
            if (await _authService.CheckUserHasAccessRight([UserRole.Admin], authorId) == false)
            {
                throw new UserDoesNotHaveAccess("You do not have access to perform this action.");
            }
            
            var author = await _userRepository.GetById(authorId);

            if (author == null)
                throw new UserDoesNotExist("User does not exist.");

            var celestialBody = await _celestialBodyRepository.GetByName(request.CelestialBodyPath);

            if (celestialBody == null)
                throw new CelestialBodyDoesNotExist("Celestial body not found.");

            var revision = new ContentRevisions
            {
                Content = request.Content,
                CelestialBody = celestialBody,
                Author = author,
                CreatedAt = DateTime.UtcNow
            };

            await _contentRevisionRepository.Create(revision);

            return revision;
        }
    }
}
