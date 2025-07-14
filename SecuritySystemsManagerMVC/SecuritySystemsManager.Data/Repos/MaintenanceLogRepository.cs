using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecuritySystemsManager.Data.Repos
{
    [AutoBind]
    public class MaintenanceLogRepository : BaseRepository<MaintenanceLog, MaintenanceLogDto>, IMaintenanceLogRepository
    {
        private readonly SecuritySystemsManagerDbContext _dbContext;

        public MaintenanceLogRepository(SecuritySystemsManagerDbContext context, IMapper mapper) : base(context, mapper) 
        {
            _dbContext = context;
        }

        public override async Task<MaintenanceLogDto> GetByIdAsync(int id)
        {
            var entity = await _dbContext.MaintenanceLogs
                .Include(ml => ml.SecuritySystemOrder)
                .Include(ml => ml.Technician)
                .Include(ml => ml.MaintenanceDevices)
                    .ThenInclude(md => md.InstalledDevice)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return MapToModel(entity);
        }

        public async Task<IEnumerable<MaintenanceLogDto>> GetLogsByClientIdAsync(int clientId, int pageSize, int pageNumber)
        {
            var query = _dbContext.MaintenanceLogs
                .Include(ml => ml.SecuritySystemOrder)
                .Include(ml => ml.Technician)
                .Include(ml => ml.MaintenanceDevices)
                    .ThenInclude(md => md.InstalledDevice)
                .Where(ml => ml.SecuritySystemOrder.ClientId == clientId);

            var logs = await query
                .OrderByDescending(ml => ml.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MaintenanceLogDto>>(logs);
        }

        public async Task<IEnumerable<MaintenanceLogDto>> GetLogsByTechnicianIdAsync(int technicianId, int pageSize, int pageNumber)
        {
            var query = _dbContext.MaintenanceLogs
                .Include(ml => ml.SecuritySystemOrder)
                .Include(ml => ml.Technician)
                .Include(ml => ml.MaintenanceDevices)
                    .ThenInclude(md => md.InstalledDevice)
                .Where(ml => 
                    ml.TechnicianId == technicianId || 
                    ml.SecuritySystemOrder.Technicians.Any(t => t.Id == technicianId));

            var logs = await query
                .OrderByDescending(ml => ml.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MaintenanceLogDto>>(logs);
        }

        public async Task<int> GetLogsCountByClientIdAsync(int clientId)
        {
            return await _dbContext.MaintenanceLogs
                .Where(ml => ml.SecuritySystemOrder.ClientId == clientId)
                .CountAsync();
        }

        public async Task<int> GetLogsCountByTechnicianIdAsync(int technicianId)
        {
            return await _dbContext.MaintenanceLogs
                .Where(ml => 
                    ml.TechnicianId == technicianId || 
                    ml.SecuritySystemOrder.Technicians.Any(t => t.Id == technicianId))
                .CountAsync();
        }
    }
} 