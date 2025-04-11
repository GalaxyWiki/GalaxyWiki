using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using GalaxyWiki.Application.DTO;
using GalaxyWiki.Core.Entities;

namespace GalaxyWiki.Application.Services
{
	public class ContentRevisionService
	{
		private readonly ISession _session;

		public ContentRevisionService(ISession session)
		{
			_session = session;
		}

		public async Task<ContentRevision?> GetRevisionByIdAsync(int id)
		{
			return await _session.GetAsync<ContentRevision>(id);
		}

		public async Task<IEnumerable<ContentRevision>> GetRevisionsByCelestialBodyAsync(string celestialBodyPath)
		{
			var celestialBody = await _session.Query<CelestialBody>()
											   .FirstOrDefaultAsync(cb => cb.Name == celestialBodyPath);

			if (celestialBody == null)
				return new List<ContentRevision>();

			return await _session.Query<ContentRevision>()
								 .Where(r => r.CelestialBody == celestialBody)
								 .ToListAsync();
		}

		public async Task<ContentRevision> CreateRevisionAsync(CreateRevisionRequest request, string authorId)
		{
			var celestialBody = await _session.Query<CelestialBody>()
				.FirstOrDefaultAsync(cb => cb.Name == request.CelestialBodyPath);

			if (celestialBody == null)
				throw new Exception("Celestial body not found.");

			var author = await _session.Query<User>()
				.FirstOrDefaultAsync(u => u.Id == authorId);

			if (author == null)
				throw new Exception("Author not found.");

			var revision = new ContentRevision
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
