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

        public async Task<IEnumerable<UserDto>> GetAllGroupedByRoleAsync()
        {
            // Data access logic: Efficient SQL query with ordering at database level
            var users = await _dbSet
                .Include(u => u.Role) // Include role information
                .ToListAsync();

            // Business logic: Custom ordering by role priority
            var orderedUsers = users
                .OrderBy(u => GetRolePriority(u.Role?.RoleType))
                .ThenBy(u => u.FirstName)
                .ThenBy(u => u.LastName);

            return orderedUsers.Select(MapToModel);
        }

        private int GetRolePriority(SecuritySystemsManager.Shared.Enums.RoleType? roleType)
        {
            return roleType switch
            {
                SecuritySystemsManager.Shared.Enums.RoleType.Admin => 1,
                SecuritySystemsManager.Shared.Enums.RoleType.Manager => 2,
                SecuritySystemsManager.Shared.Enums.RoleType.Technician => 3,
                SecuritySystemsManager.Shared.Enums.RoleType.Client => 4,
                _ => 999 // Unknown roles go last
            };
        }
    }
} 