using SecuritySystemsManager.Shared.Dtos;

namespace SecuritySystemsManager.Shared.Repos.Contracts
{
    public interface IRoleRepository : IBaseRepository<RoleDto>
    {
        Task<RoleDto> GetByNameIfExistsAsync(string name);
    }
} 