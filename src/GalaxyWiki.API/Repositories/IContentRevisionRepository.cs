using GalaxyWiki.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Repositories
{
    public interface IContentRevisionRepository
    {
        Task<ContentRevisions> GetById(int id);
        Task<IEnumerable<ContentRevisions>> GetByCelestialBodyId(int id);
        Task<ContentRevisions> Create(ContentRevisions contentRevisions);
    }
}