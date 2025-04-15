using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;
using GalaxyWiki.API.Repositories;

namespace GalaxyWiki.API.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly RoleRepository _roleRepository;

        public UserService(UserRepository userRepository, RoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<Users> GetUserById(string googleSub)
        {
            return await _userRepository.GetById(googleSub);
        }

        public async Task<Users> CreateUser(string googleSub, string email, string name, UserRole userRole)
        {
            var role = await _roleRepository.GetById((int)userRole);
            if (role == null)
            {
                throw new RoleDoesNotExist("The selected role type does not exist.");
            }

            var user = new Users
            {
                Id = googleSub,
                Email = email,
                DisplayName = name,
                Role = role
            };

            await _userRepository.Create(user);

            return user;
        }
    }
}