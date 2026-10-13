using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIBS.Module.IPGuard.Services
{
    public interface IIPGuardService
    {
        Task<List<Models.BlockedIp>> GetBlockedIpsAsync(int ModuleId);

        Task<Models.BlockedIp> GetBlockedIpAsync(int BlockedIpId, int ModuleId);

        Task<Models.BlockedIp> AddBlockedIpAsync(Models.BlockedIp BlockedIp, int ModuleId);

        Task<Models.BlockedIp> UpdateBlockedIpAsync(Models.BlockedIp BlockedIp, int ModuleId);

        Task DeleteBlockedIpAsync(int BlockedIpId, int ModuleId);

        Task<List<Models.ErrorAttempt>> GetErrorAttemptsAsync(int ModuleId);

        Task DeleteAllErrorAttemptsAsync(int ModuleId);

        Task DeleteErrorAttemptsByIpAddressAsync(string ipAddress, int ModuleId);

        Task<Models.BlockedIp> AddToBlockedAsync(string ipAddress, int ModuleId);
    }
}
