using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;
using GIBS.Module.IPGuard.Repository;

namespace GIBS.Module.IPGuard.Services
{
    public class ServerIPGuardService : IIPGuardService
    {
        private readonly IIPGuardRepository _IPGuardRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerIPGuardService(IIPGuardRepository IPGuardRepository, IUserPermissions userPermissions, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor)
        {
            _IPGuardRepository = IPGuardRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<Models.IPGuard>> GetIPGuardsAsync(int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_IPGuardRepository.GetIPGuards(ModuleId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized IPGuard Get Attempt {ModuleId}", ModuleId);
                return null;
            }
        }

        public Task<Models.IPGuard> GetIPGuardAsync(int IPGuardId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_IPGuardRepository.GetIPGuard(IPGuardId));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized IPGuard Get Attempt {IPGuardId} {ModuleId}", IPGuardId, ModuleId);
                return null;
            }
        }

        public Task<Models.IPGuard> AddIPGuardAsync(Models.IPGuard IPGuard)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, IPGuard.ModuleId, PermissionNames.Edit))
            {
                IPGuard = _IPGuardRepository.AddIPGuard(IPGuard);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "IPGuard Added {IPGuard}", IPGuard);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized IPGuard Add Attempt {IPGuard}", IPGuard);
                IPGuard = null;
            }
            return Task.FromResult(IPGuard);
        }

        public Task<Models.IPGuard> UpdateIPGuardAsync(Models.IPGuard IPGuard)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, IPGuard.ModuleId, PermissionNames.Edit))
            {
                IPGuard = _IPGuardRepository.UpdateIPGuard(IPGuard);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "IPGuard Updated {IPGuard}", IPGuard);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized IPGuard Update Attempt {IPGuard}", IPGuard);
                IPGuard = null;
            }
            return Task.FromResult(IPGuard);
        }

        public Task DeleteIPGuardAsync(int IPGuardId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                _IPGuardRepository.DeleteIPGuard(IPGuardId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "IPGuard Deleted {IPGuardId}", IPGuardId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized IPGuard Delete Attempt {IPGuardId} {ModuleId}", IPGuardId, ModuleId);
            }
            return Task.CompletedTask;
        }
    }
}
