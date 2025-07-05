using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Security;

namespace SecuritySystemsManager.Data.Repos
{
    [AutoBind]
    public class UserRepository : BaseRepository<User, UserDto>, IUserRepository
    {
        public UserRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) { }

        public async Task<bool> CanUserLoginAsync(string username, string password)
        {
            var userEntity = await _dbSet.FirstOrDefaultAsync(u => u.UserName == username);

            if (userEntity == null)
            {
                return false;
            }

            return PasswordHasher.VerifyPassword(password, userEntity.PasswordHash);
        }

        public async Task<UserDto> GetByUsernameAsync(string username)
        {
            return MapToModel(await _dbSet.FirstOrDefaultAsync(u => u.UserName == username));
        }

    }
} 