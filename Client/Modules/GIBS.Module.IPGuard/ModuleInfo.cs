using Oqtane.Models;
using Oqtane.Modules;

namespace GIBS.Module.IPGuard
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "IPGuard",
            Description = "Automated IP blocking mechanism based on 404 errors",
            Version = "1.0.10",
            ServerManagerType = "GIBS.Module.IPGuard.Manager.IPGuardManager, GIBS.Module.IPGuard.Server.Oqtane",
            ReleaseVersions = "1.0.0,1.0.1,1.0.2,1.0.3,1.0.4,1.0.5,1.0.6,1.0.7,1.0.8,1.0.9,1.0.10",
            Dependencies = "GIBS.Module.IPGuard.Shared.Oqtane",
            PackageName = "GIBS.Module.IPGuard" 
        };
    }
}
