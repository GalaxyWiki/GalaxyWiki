using GalaxyWiki.API.DTOs;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;
using GalaxyWiki.API.Repositories;

namespace GalaxyWiki.API.Services
{
    public class ContentRevisionService : IContentRevisionService
    {
        private IAuthService _authService;
        private readonly IContentRevisionRepository _contentRevisionRepository;
        private readonly ICelestialBodyRepository _celestialBodyRepository;
        private readonly IUserRepository _userRepository;

        public ContentRevisionService(IAuthService authService, IContentRevisionRepository contentRevisionRepository, ICelestialBodyRepository celestialBodyRepository, IUserRepository userRepository)
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

            if (author == null) throw new UserDoesNotExist("User does not exist.");

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

            celestialBody.ActiveRevision = revision.Id;
            await _celestialBodyRepository.Update(celestialBody);

            return revision;
        }
    }
}
