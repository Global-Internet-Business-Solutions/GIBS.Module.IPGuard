using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Enums;
using Oqtane.Repository;
using GIBS.Module.IPGuard.Repository;
using System.Threading.Tasks;

namespace GIBS.Module.IPGuard.Manager
{
    public class IPGuardManager : MigratableModuleBase, IInstallable, IPortable, ISearchable
    {
        private readonly IIPGuardRepository _IPGuardRepository;
        private readonly IDBContextDependencies _DBContextDependencies;

        public IPGuardManager(IIPGuardRepository IPGuardRepository, IDBContextDependencies DBContextDependencies)
        {
            _IPGuardRepository = IPGuardRepository;
            _DBContextDependencies = DBContextDependencies;
        }

        public bool Install(Tenant tenant, string version)
        {
            return Migrate(new IPGuardContext(_DBContextDependencies), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new IPGuardContext(_DBContextDependencies), tenant, MigrationType.Down);
        }

        public string ExportModule(Oqtane.Models.Module module)
        {
            string content = "";
            List<Models.IPGuard> IPGuards = _IPGuardRepository.GetIPGuards(module.ModuleId).ToList();
            if (IPGuards != null)
            {
                content = JsonSerializer.Serialize(IPGuards);
            }
            return content;
        }

        public void ImportModule(Oqtane.Models.Module module, string content, string version)
        {
            List<Models.IPGuard> IPGuards = null;
            if (!string.IsNullOrEmpty(content))
            {
                IPGuards = JsonSerializer.Deserialize<List<Models.IPGuard>>(content);
            }
            if (IPGuards != null)
            {
                foreach(var IPGuard in IPGuards)
                {
                    _IPGuardRepository.AddIPGuard(new Models.IPGuard { ModuleId = module.ModuleId, Name = IPGuard.Name });
                }
            }
        }

        public Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
           var searchContentList = new List<SearchContent>();

           foreach (var IPGuard in _IPGuardRepository.GetIPGuards(pageModule.ModuleId))
           {
               if (IPGuard.ModifiedOn >= lastIndexedOn)
               {
                   searchContentList.Add(new SearchContent
                   {
                       EntityName = "GIBSIPGuard",
                       EntityId = IPGuard.IPGuardId.ToString(),
                       Title = IPGuard.Name,
                       Body = IPGuard.Name,
                       ContentModifiedBy = IPGuard.ModifiedBy,
                       ContentModifiedOn = IPGuard.ModifiedOn
                   });
               }
           }

           return Task.FromResult(searchContentList);
        }
    }
}
