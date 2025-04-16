using GalaxyWiki.Core.Entities;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Repositories
{
    public interface IBodyTypeRepository
    {
        Task<BodyTypes?> GetById(int id);
    }
} 