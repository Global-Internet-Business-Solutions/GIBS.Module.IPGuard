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

        public async Task<List<Models.IPGuard>> GetIPGuardsAsync(int ModuleId)
        {
            List<Models.IPGuard> IPGuards = await GetJsonAsync<List<Models.IPGuard>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), Enumerable.Empty<Models.IPGuard>().ToList());
            return IPGuards.OrderBy(item => item.Name).ToList();
        }

        public async Task<Models.IPGuard> GetIPGuardAsync(int IPGuardId, int ModuleId)
        {
            return await GetJsonAsync<Models.IPGuard>(CreateAuthorizationPolicyUrl($"{Apiurl}/{IPGuardId}/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task<Models.IPGuard> AddIPGuardAsync(Models.IPGuard IPGuard)
        {
            return await PostJsonAsync<Models.IPGuard>(CreateAuthorizationPolicyUrl($"{Apiurl}", EntityNames.Module, IPGuard.ModuleId), IPGuard);
        }

        public async Task<Models.IPGuard> UpdateIPGuardAsync(Models.IPGuard IPGuard)
        {
            return await PutJsonAsync<Models.IPGuard>(CreateAuthorizationPolicyUrl($"{Apiurl}/{IPGuard.IPGuardId}", EntityNames.Module, IPGuard.ModuleId), IPGuard);
        }

        public async Task DeleteIPGuardAsync(int IPGuardId, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{IPGuardId}/{ModuleId}", EntityNames.Module, ModuleId));
        }
    }
}
