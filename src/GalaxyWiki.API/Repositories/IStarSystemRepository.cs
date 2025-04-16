using GalaxyWiki.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Repositories
{
    public interface IStarSystemRepository
    {
        Task<IEnumerable<StarSystems>> GetAll();
        Task<StarSystems> GetById(int id);
        Task<StarSystems> Create(StarSystems starSystem);
        Task<StarSystems> Update(StarSystems starSystem);
        Task Delete(StarSystems starSystem);
    }
} 