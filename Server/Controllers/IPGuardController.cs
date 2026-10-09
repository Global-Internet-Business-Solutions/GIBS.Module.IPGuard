using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using GIBS.Module.IPGuard.Services;
using Oqtane.Controllers;
using System.Net;
using System.Threading.Tasks;

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

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Models.IPGuard>> Get(string moduleid)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _IPGuardService.GetIPGuardsAsync(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized IPGuard Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.IPGuard> Get(int id, int moduleid)
        {
            Models.IPGuard IPGuard = await _IPGuardService.GetIPGuardAsync(id, moduleid);
            if (IPGuard != null && IsAuthorizedEntityId(EntityNames.Module, IPGuard.ModuleId))
            {
                return IPGuard;
            }
            else
            { 
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized IPGuard Get Attempt {IPGuardId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.IPGuard> Post([FromBody] Models.IPGuard IPGuard)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, IPGuard.ModuleId))
            {
                IPGuard = await _IPGuardService.AddIPGuardAsync(IPGuard);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized IPGuard Post Attempt {IPGuard}", IPGuard);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                IPGuard = null;
            }
            return IPGuard;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.IPGuard> Put(int id, [FromBody] Models.IPGuard IPGuard)
        {
            if (ModelState.IsValid && IPGuard.IPGuardId == id && IsAuthorizedEntityId(EntityNames.Module, IPGuard.ModuleId))
            {
                IPGuard = await _IPGuardService.UpdateIPGuardAsync(IPGuard);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized IPGuard Put Attempt {IPGuard}", IPGuard);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                IPGuard = null;
            }
            return IPGuard;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleid)
        {
            Models.IPGuard IPGuard = await _IPGuardService.GetIPGuardAsync(id, moduleid);
            if (IPGuard != null && IsAuthorizedEntityId(EntityNames.Module, IPGuard.ModuleId))
            {
                await _IPGuardService.DeleteIPGuardAsync(id, IPGuard.ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized IPGuard Delete Attempt {IPGuardId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
