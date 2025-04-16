using GalaxyWiki.Core.Entities;
using System.Threading.Tasks;

namespace GalaxyWiki.Api.Repositories
{
    public interface IUserRepository
    {
        Task<Users> GetById(string id);
        Task<Users> Create(Users user);
    }
}
