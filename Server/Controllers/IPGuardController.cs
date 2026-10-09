using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GIBS.Module.IPGuard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;

namespace GIBS.Module.IPGuard.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class IPGuardController : ModuleControllerBase
    {
        private readonly IIPGuardService _IPGuardService;

        public IPGuardController(IIPGuardService IPGuardService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _IPGuardService = IPGuardService;
        }

        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Models.BlockedIp>> Get(int moduleid)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                return await _IPGuardService.GetBlockedIpsAsync(moduleid);
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized BlockedIp Get Attempt {ModuleId}", moduleid);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return null;
        }

        [HttpGet("attempts")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Models.ErrorAttempt>> GetAttempts(int moduleid)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                return await _IPGuardService.GetErrorAttemptsAsync(moduleid);
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ErrorAttempt Get Attempt {ModuleId}", moduleid);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return null;
        }

        [HttpGet("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.BlockedIp> Get(int id, int moduleid)
        {
            var blockedIp = await _IPGuardService.GetBlockedIpAsync(id, moduleid);
            if (blockedIp != null && IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                return blockedIp;
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized BlockedIp Get Attempt {BlockedIpId} {ModuleId}", id, moduleid);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return null;
        }

        [HttpDelete("attempts/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task DeleteAttempts(int moduleid)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                await _IPGuardService.DeleteAllErrorAttemptsAsync(moduleid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ErrorAttempt Delete Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        [HttpDelete("attempts/byip")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task DeleteAttemptsByIp(int moduleid, string ipAddress)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                await _IPGuardService.DeleteErrorAttemptsByIpAddressAsync(ipAddress, moduleid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ErrorAttempt Delete Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.BlockedIp> Post(int moduleid, [FromBody] Models.BlockedIp blockedIp)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                blockedIp = await _IPGuardService.AddBlockedIpAsync(blockedIp, moduleid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized BlockedIp Post Attempt {BlockedIp}", blockedIp);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                blockedIp = null;
            }

            return blockedIp;
        }

        [HttpPut("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.BlockedIp> Put(int id, int moduleid, [FromBody] Models.BlockedIp blockedIp)
        {
            if (ModelState.IsValid && blockedIp.BlockedIpId == id && IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                blockedIp = await _IPGuardService.UpdateBlockedIpAsync(blockedIp, moduleid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized BlockedIp Put Attempt {BlockedIp}", blockedIp);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                blockedIp = null;
            }

            return blockedIp;
        }

        [HttpDelete("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleid)
        {
            var blockedIp = await _IPGuardService.GetBlockedIpAsync(id, moduleid);
            if (blockedIp != null && IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                await _IPGuardService.DeleteBlockedIpAsync(id, moduleid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized BlockedIp Delete Attempt {BlockedIpId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
