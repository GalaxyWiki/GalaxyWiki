using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Enums;
using GalaxyWiki.API.Repositories;

namespace GalaxyWiki.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<Users> GetUserById(string googleSub)
        {
            Console.WriteLine("In the user");
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