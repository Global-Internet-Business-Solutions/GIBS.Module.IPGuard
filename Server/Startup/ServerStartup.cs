using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using GIBS.Module.IPGuard.Repository;
using GIBS.Module.IPGuard.Services;

namespace GIBS.Module.IPGuard.Startup
{
    public class ServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // not implemented
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            // not implemented
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IIPGuardService, ServerIPGuardService>();
            services.AddDbContextFactory<IPGuardContext>(opt => { }, ServiceLifetime.Transient);
        }
    }
}
