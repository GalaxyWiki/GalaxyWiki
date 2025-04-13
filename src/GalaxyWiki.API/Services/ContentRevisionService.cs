using NHibernate.Linq;
using GalaxyWiki.API.DTOs;
using GalaxyWiki.Core.Entities;

namespace GalaxyWiki.API.Services
{
    public class ContentRevisionService(NHibernate.ISession session)
    {
        private readonly NHibernate.ISession _session = session;

        public async Task<ContentRevisions?> GetRevisionByIdAsync(int id)
        {
            return await _session.GetAsync<ContentRevisions>(id);
        }

        public async Task<IEnumerable<ContentRevisions>> GetRevisionsByCelestialBodyAsync(string celestialBodyPath)
        {
            var celestialBody = await _session.Query<CelestialBodies>()
                                               .FirstOrDefaultAsync(cb => cb.BodyName == celestialBodyPath);

            if (celestialBody == null)
                return new List<ContentRevisions>();

            return await _session.Query<ContentRevisions>()
                                 .Where(r => r.CelestialBody == celestialBody)
                                 .ToListAsync();
        }

        public async Task<ContentRevisions> CreateRevisionAsync(CreateRevisionRequest request, string authorId)
        {
            var celestialBody = await _session.Query<CelestialBodies>()
                .FirstOrDefaultAsync(cb => cb.BodyName == request.CelestialBodyPath);

            if (celestialBody == null)
                throw new Exception("Celestial body not found.");

            var author = await _session.Query<Users>()
                .FirstOrDefaultAsync(u => u.Id == authorId);

            if (author == null)
                throw new Exception("Author not found.");

            var revision = new ContentRevisions
            {
                Content = request.Content,
                CelestialBody = celestialBody,
                Author = author,
                CreatedAt = DateTime.UtcNow
            };

            await _session.SaveAsync(revision);
            return revision;
        }
    }
}
