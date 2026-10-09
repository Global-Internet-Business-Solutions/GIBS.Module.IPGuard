using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIBS.Module.IPGuard.Services
{
    public interface IIPGuardService 
    {
        Task<List<Models.IPGuard>> GetIPGuardsAsync(int ModuleId);

        Task<Models.IPGuard> GetIPGuardAsync(int IPGuardId, int ModuleId);

        Task<Models.IPGuard> AddIPGuardAsync(Models.IPGuard IPGuard);

        Task<Models.IPGuard> UpdateIPGuardAsync(Models.IPGuard IPGuard);

        Task DeleteIPGuardAsync(int IPGuardId, int ModuleId);
    }
}
