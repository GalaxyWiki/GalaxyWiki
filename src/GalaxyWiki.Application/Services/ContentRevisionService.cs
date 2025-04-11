using System.Collections.Generic;
using System.Threading.Tasks;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Exceptions;
using NHibernate;

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
											   .FirstOrDefaultAsync(cb => cb.Path == celestialBodyPath);

			if (celestialBody == null)
				return null;

			return await _session.Query<ContentRevision>()
								 .Where(r => r.CelestialBody == celestialBody)
								 .ToListAsync();
		}

		public async Task<ContentRevision> CreateRevisionAsync(CreateRevisionRequest request, string authorId)
		{
			var celestialBody = await _session.Query<CelestialBody>()
				.FirstOrDefaultAsync(cb => cb.Path == request.CelestialBodyPath);

			if (celestialBody == null)
				throw new NotFoundException("Celestial body not found.");

			var author = await _session.Query<User>()
				.FirstOrDefaultAsync(u => u.GoogleSub == authorId);

			if (author == null)
				throw new NotFoundException("Author not found.");

			var revision = new ContentRevision(
				content: request.Content,
				celestialBody: celestialBody,
				author: author
			);

			await _session.SaveAsync(revision);
			return revision;
		}
	}
}
