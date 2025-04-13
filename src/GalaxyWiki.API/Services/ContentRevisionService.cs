using NHibernate.Linq;
using GalaxyWiki.API.DTOs;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;

namespace GalaxyWiki.API.Services
{
    public class ContentRevisionService(NHibernate.ISession _session)
    {
        // private readonly NHibernate.ISession _session;

        // private readonly AuthService _authService;


        public async Task<ContentRevisions?> GetRevisionByIdAsync(int id)
        {
            return await _session.GetAsync<ContentRevisions>(id);
        }

        public async Task<IEnumerable<ContentRevisions>> GetRevisionsByCelestialBodyAsync(string celestialBodyPath)
        {
            var celestialBody = await _session.Query<CelestialBodies>()
                                               .FirstOrDefaultAsync(cb => cb.BodyName == celestialBodyPath);

            if (celestialBody == null)
                return [];

            return await _session.Query<ContentRevisions>()
                                 .Where(r => r.CelestialBody == celestialBody)
                                 .ToListAsync();
        }

        public async Task<ContentRevisions> CreateRevisionAsync(CreateRevisionRequest request, string? authorId = null)
        {
            using var transaction = _session.BeginTransaction();
            try 
            {
                var celestialBody = await _session.Query<CelestialBodies>()
                    .FirstOrDefaultAsync(cb => cb.BodyName == request.CelestialBodyPath);

                if (celestialBody == null)
                    throw new CelestialBodyDoesNotExist("Celestial body not found.");

                var author = await _session.Query<Users>()
                    .FirstOrDefaultAsync(u => u.Id == authorId);

                if (author == null)
                    throw new UserDoesNotExist("User does not exist.");

                var revision = new ContentRevisions
                {
                    Content = request.Content,
                    CelestialBody = celestialBody,
                    Author = author,
                    CreatedAt = DateTime.UtcNow
                };

                await _session.SaveAsync(revision);

                await transaction.CommitAsync();

                return revision;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            
        }
    }
}
