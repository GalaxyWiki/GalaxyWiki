using GalaxyWiki.API.DTOs;
using GalaxyWiki.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Services
{
    public interface IContentRevisionService
    {
        Task<ContentRevisions?> GetRevisionByIdAsync(int id);
        Task<IEnumerable<ContentRevisions>> GetRevisionsByCelestialBodyAsync(string celestialBodyPath);
        Task<ContentRevisions> CreateRevision(CreateRevisionRequest request, string authorId);
    }
}