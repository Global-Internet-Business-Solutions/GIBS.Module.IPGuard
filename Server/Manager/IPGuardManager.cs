using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GIBS.Module.IPGuard.Repository;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Models;
using Oqtane.Modules;
using Oqtane.Repository;

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
            var blockedIps = _IPGuardRepository.GetBlockedIps().ToList();
            return blockedIps.Count > 0 ? JsonSerializer.Serialize(blockedIps) : string.Empty;
        }

        public void ImportModule(Oqtane.Models.Module module, string content, string version)
        {
            List<Models.BlockedIp> blockedIps = null;
            if (!string.IsNullOrEmpty(content))
            {
                blockedIps = JsonSerializer.Deserialize<List<Models.BlockedIp>>(content);
            }

            if (blockedIps != null)
            {
                foreach (var blockedIp in blockedIps)
                {
                    _IPGuardRepository.AddBlockedIp(new Models.BlockedIp
                    {
                        IpAddress = blockedIp.IpAddress,
                        CountryLookup = blockedIp.CountryLookup,
                        ErrorCount = blockedIp.ErrorCount,
                        BlockedDate = blockedIp.BlockedDate,
                        Reason = blockedIp.Reason
                    });
                }
            }
        }

        public Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
            var searchContentList = new List<SearchContent>();

            foreach (var blockedIp in _IPGuardRepository.GetBlockedIps())
            {
                if (blockedIp.ModifiedOn >= lastIndexedOn)
                {
                    searchContentList.Add(new SearchContent
                    {
                        EntityName = "GIBSIPGuardBlockedIp",
                        EntityId = blockedIp.BlockedIpId.ToString(),
                        Title = blockedIp.IpAddress,
                        Body = string.IsNullOrWhiteSpace(blockedIp.Reason) ? blockedIp.IpAddress : blockedIp.Reason,
                        ContentModifiedBy = blockedIp.ModifiedBy,
                        ContentModifiedOn = blockedIp.ModifiedOn
                    });
                }
            }

            return Task.FromResult(searchContentList);
        }
    }
}
