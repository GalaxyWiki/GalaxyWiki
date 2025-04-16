using GalaxyWiki.Core.Enums;

namespace GalaxyWiki.API.Services
{
    public interface IAuthService
    {
        Task<bool> CheckUserHasAccessRight(UserRole[] accessLevelRequired, string? authorId);
    }
}
