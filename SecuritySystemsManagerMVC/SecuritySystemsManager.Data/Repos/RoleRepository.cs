using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;

namespace SecuritySystemsManager.Data.Repos
{
    [AutoBind]
    public class RoleRepository : BaseRepository<Role, RoleDto>, IRoleRepository
    {
        public RoleRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) { }

        public async Task<RoleDto> GetByNameIfExistsAsync(string name)
        {
            return MapToModel(await _dbSet.FirstOrDefaultAsync(u => u.Name == name));
        }

    }
} 