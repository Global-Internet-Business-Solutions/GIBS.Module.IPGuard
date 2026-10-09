using GIBS.Module.IPGuard.Middleware;
using GIBS.Module.IPGuard.Repository;
using GIBS.Module.IPGuard.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;

namespace GIBS.Module.IPGuard.Startup
{
    public class ServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<IPGuardMiddleware>();
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            // not implemented
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddTransient<IpApiGeoLocationService>();
            services.AddTransient<ServerIPGuardService>();
            services.AddTransient<IIPGuardService, ServerIPGuardService>();
        }
    }
}
