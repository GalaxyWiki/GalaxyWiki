using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Services
{
    public interface IUserService
    {
        Task<Users> GetUserById(string googleSub);
        Task<Users> CreateUser(string googleSub, string email, string name, UserRole userRole);
    }
}