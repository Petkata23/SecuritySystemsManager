using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Dtos;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;

namespace SecuritySystemsManager.Services
{
    [AutoBind]
    public class SecuritySystemOrderService : BaseCrudService<SecuritySystemOrderDto, ISecuritySystemOrderRepository>, ISecuritySystemOrderService
    {
        public SecuritySystemOrderService(ISecuritySystemOrderRepository repository) : base(repository)
        {
        }
    }
} 