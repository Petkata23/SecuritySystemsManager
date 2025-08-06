using SecuritySystemsManager.Shared.Dtos;

namespace SecuritySystemsManager.Shared.Repos.Contracts
{
    public interface IUserRepository : IBaseRepository<UserDto>
    {
        Task<bool> CanUserLoginAsync(string username, string password);
        Task<UserDto> GetByUsernameAsync(string username);
        
        // Data access logic for user grouping and ordering
        Task<IEnumerable<UserDto>> GetAllGroupedByRoleAsync();
    }
} 