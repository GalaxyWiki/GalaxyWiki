using GalaxyWiki.Core.Enums;
using System.Threading.Tasks;

namespace GalaxyWiki.API.Services
{
    public interface IAuthService
    {
        Task<string[]> Login(string authCode);
        Task<bool> CheckUserHasAccessRight(UserRole[] accessLevelRequired, string? authorId);
    }
}