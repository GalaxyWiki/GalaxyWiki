using GalaxyWiki.Core.Entities;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Repositories
{
    public interface IRoleRepository
    {
        Task<Roles> GetById(int id);
    }
}