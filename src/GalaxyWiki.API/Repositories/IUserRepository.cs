using GalaxyWiki.Core.Entities;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Repositories
{
    public interface IUserRepository
    {
        Task<Users> GetById(string id);
        Task<Users> Create(Users user);
    }
}