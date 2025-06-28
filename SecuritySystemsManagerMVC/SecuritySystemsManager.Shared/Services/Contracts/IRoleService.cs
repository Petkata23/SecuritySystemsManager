using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Shared.Services.Contracts
{
    public interface IRoleService : IBaseCrudService<RoleDto, IRoleRepository>
    {
        Task<RoleDto> GetByNameIfExistsAsync(string name);
    }
} 