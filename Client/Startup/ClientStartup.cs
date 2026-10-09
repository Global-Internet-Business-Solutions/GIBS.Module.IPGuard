using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Oqtane.Services;
using GIBS.Module.IPGuard.Services;

namespace GIBS.Module.IPGuard.Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            if (!services.Any(s => s.ServiceType == typeof(IIPGuardService)))
            {
                services.AddScoped<IIPGuardService, ClientIPGuardService>();
            }
        }
    }
}
