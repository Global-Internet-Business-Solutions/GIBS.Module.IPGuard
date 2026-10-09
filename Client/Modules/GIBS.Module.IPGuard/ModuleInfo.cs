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
            Version = "1.0.0",
            ServerManagerType = "GIBS.Module.IPGuard.Manager.IPGuardManager, GIBS.Module.IPGuard.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "GIBS.Module.IPGuard.Shared.Oqtane",
            PackageName = "GIBS.Module.IPGuard" 
        };
    }
}
