using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;

namespace GIBS.Module.IPGuard.Services
{
    public class ClientIPGuardService : ServiceBase, IIPGuardService
    {
        public ClientIPGuardService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("IPGuard");

        public async Task<List<Models.BlockedIp>> GetBlockedIpsAsync(int ModuleId)
        {
            var blockedIps = await GetJsonAsync<List<Models.BlockedIp>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), Enumerable.Empty<Models.BlockedIp>().ToList());
            return blockedIps.OrderByDescending(item => item.BlockedDate).ToList();
        }

        public async Task<Models.BlockedIp> GetBlockedIpAsync(int BlockedIpId, int ModuleId)
        {
            return await GetJsonAsync<Models.BlockedIp>(CreateAuthorizationPolicyUrl($"{Apiurl}/{BlockedIpId}/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task<Models.BlockedIp> AddBlockedIpAsync(Models.BlockedIp BlockedIp, int ModuleId)
        {
            return await PostJsonAsync<Models.BlockedIp>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), BlockedIp);
        }

        public async Task<Models.BlockedIp> UpdateBlockedIpAsync(Models.BlockedIp BlockedIp, int ModuleId)
        {
            return await PutJsonAsync<Models.BlockedIp>(CreateAuthorizationPolicyUrl($"{Apiurl}/{BlockedIp.BlockedIpId}/{ModuleId}", EntityNames.Module, ModuleId), BlockedIp);
        }

        public async Task DeleteBlockedIpAsync(int BlockedIpId, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{BlockedIpId}/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task<List<Models.ErrorAttempt>> GetErrorAttemptsAsync(int ModuleId)
        {
            var errorAttempts = await GetJsonAsync<List<Models.ErrorAttempt>>(CreateAuthorizationPolicyUrl($"{Apiurl}/attempts?moduleid={ModuleId}", EntityNames.Module, ModuleId), Enumerable.Empty<Models.ErrorAttempt>().ToList());
            return errorAttempts.OrderByDescending(item => item.AttemptDate).ToList();
        }

        public async Task DeleteAllErrorAttemptsAsync(int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/attempts/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task DeleteErrorAttemptsByIpAddressAsync(string ipAddress, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/attempts/byip?moduleid={ModuleId}&ipAddress={Uri.EscapeDataString(ipAddress ?? string.Empty)}", EntityNames.Module, ModuleId));
        }
    }
}
