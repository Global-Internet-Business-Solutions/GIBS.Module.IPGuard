using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIBS.Module.IPGuard.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;

namespace GIBS.Module.IPGuard.Services
{
    public class ServerIPGuardService : IIPGuardService
    {
        private static readonly TimeSpan Window = TimeSpan.FromHours(1);

        private readonly IIPGuardRepository _IPGuardRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly ISettingRepository _settingRepository;
        private readonly IpApiGeoLocationService _geoLocationService;
        private readonly Alias _alias;

        public ServerIPGuardService(IIPGuardRepository IPGuardRepository, IUserPermissions userPermissions, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor, ISettingRepository settingRepository, IpApiGeoLocationService geoLocationService)
        {
            _IPGuardRepository = IPGuardRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _settingRepository = settingRepository;
            _geoLocationService = geoLocationService;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<Models.BlockedIp>> GetBlockedIpsAsync(int ModuleId)
        {
            if (IsAuthorized(ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_IPGuardRepository.GetBlockedIps().OrderByDescending(item => item.BlockedDate).ToList());
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized BlockedIp Get Attempt {ModuleId}", ModuleId);
            return null;
        }

        public Task<Models.BlockedIp> GetBlockedIpAsync(int BlockedIpId, int ModuleId)
        {
            if (IsAuthorized(ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_IPGuardRepository.GetBlockedIp(BlockedIpId));
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized BlockedIp Get Attempt {BlockedIpId} {ModuleId}", BlockedIpId, ModuleId);
            return null;
        }

        public Task<Models.BlockedIp> AddBlockedIpAsync(Models.BlockedIp BlockedIp, int ModuleId)
        {
            if (IsAuthorized(ModuleId, PermissionNames.Edit))
            {
                if (BlockedIp.BlockedDate == default)
                {
                    BlockedIp.BlockedDate = DateTime.UtcNow;
                }

                BlockedIp.IpAddress = NormalizeIpAddress(BlockedIp.IpAddress);

                var existingBlockedIp = _IPGuardRepository.GetBlockedIp(BlockedIp.IpAddress);
                if (existingBlockedIp != null)
                {
                    throw new InvalidOperationException($"The IP address '{BlockedIp.IpAddress}' is already blocked.");
                }

                try
                {
                    BlockedIp = _IPGuardRepository.AddBlockedIp(BlockedIp);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "BlockedIp Added {BlockedIp}", BlockedIp);
                }
                catch (DbUpdateException ex)
                {
                    var duplicateBlockedIp = _IPGuardRepository.GetBlockedIp(BlockedIp.IpAddress);
                    if (duplicateBlockedIp != null)
                    {
                        throw new InvalidOperationException($"The IP address '{BlockedIp.IpAddress}' is already blocked.", ex);
                    }

                    throw;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized BlockedIp Add Attempt {BlockedIp}", BlockedIp);
                BlockedIp = null;
            }

            return Task.FromResult(BlockedIp);
        }

        public Task<Models.BlockedIp> UpdateBlockedIpAsync(Models.BlockedIp BlockedIp, int ModuleId)
        {
            if (IsAuthorized(ModuleId, PermissionNames.Edit))
            {
                BlockedIp.IpAddress = NormalizeIpAddress(BlockedIp.IpAddress);
                BlockedIp = _IPGuardRepository.UpdateBlockedIp(BlockedIp);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "BlockedIp Updated {BlockedIp}", BlockedIp);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized BlockedIp Update Attempt {BlockedIp}", BlockedIp);
                BlockedIp = null;
            }

            return Task.FromResult(BlockedIp);
        }

        public Task DeleteBlockedIpAsync(int BlockedIpId, int ModuleId)
        {
            if (IsAuthorized(ModuleId, PermissionNames.Edit))
            {
                var blockedIp = _IPGuardRepository.GetBlockedIp(BlockedIpId);
                if (blockedIp != null)
                {
                    _IPGuardRepository.DeleteErrorAttempts(NormalizeIpAddress(blockedIp.IpAddress));
                    _IPGuardRepository.DeleteBlockedIp(BlockedIpId);
                }
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "BlockedIp Deleted {BlockedIpId}", BlockedIpId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized BlockedIp Delete Attempt {BlockedIpId} {ModuleId}", BlockedIpId, ModuleId);
            }

            return Task.CompletedTask;
        }

        public Task<List<Models.ErrorAttempt>> GetErrorAttemptsAsync(int ModuleId)
        {
            if (IsAuthorized(ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_IPGuardRepository.GetErrorAttempts().Take(100).ToList());
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ErrorAttempt Get Attempt {ModuleId}", ModuleId);
            return null;
        }

        public Task DeleteAllErrorAttemptsAsync(int ModuleId)
        {
            if (IsAuthorized(ModuleId, PermissionNames.Edit))
            {
                _IPGuardRepository.DeleteAllErrorAttempts();
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Deleted all error attempts for module {ModuleId}", ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ErrorAttempt Delete Attempt {ModuleId}", ModuleId);
            }

            return Task.CompletedTask;
        }

        public Task DeleteErrorAttemptsByIpAddressAsync(string ipAddress, int ModuleId)
        {
            if (IsAuthorized(ModuleId, PermissionNames.Edit))
            {
                var normalizedIpAddress = NormalizeIpAddress(ipAddress);
                if (!string.IsNullOrWhiteSpace(normalizedIpAddress))
                {
                    _IPGuardRepository.DeleteErrorAttempts(normalizedIpAddress);
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Deleted error attempts for IP {IpAddress}", normalizedIpAddress);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized ErrorAttempt Delete Attempt {ModuleId}", ModuleId);
            }

            return Task.CompletedTask;
        }

        public Task<bool> IsBlockedAsync(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress) || !IsEnabled())
            {
                return Task.FromResult(false);
            }

            var normalizedIpAddress = NormalizeIpAddress(ipAddress);
            if (IsAllowedIpAddress(normalizedIpAddress))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(_IPGuardRepository.IsBlocked(normalizedIpAddress));
        }

        public async Task Record404Async(string ipAddress, string requestedUrl)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return;
            }

            var normalizedIpAddress = NormalizeIpAddress(ipAddress);
            _IPGuardRepository.AddErrorAttempt(new Models.ErrorAttempt
            {
                IpAddress = normalizedIpAddress,
                RequestedUrl = requestedUrl,
                AttemptDate = DateTime.UtcNow
            });

            if (!IsEnabled())
            {
                return;
            }

            var threshold = GetThreshold();
            var recentAttempts = _IPGuardRepository.GetRecentErrorAttemptCount(normalizedIpAddress, DateTime.UtcNow.Subtract(Window));
            var isAllowedIpAddress = IsAllowedIpAddress(normalizedIpAddress);

            if (recentAttempts >= threshold && !isAllowedIpAddress)
            {
                var blockedIp = _IPGuardRepository.GetBlockedIp(normalizedIpAddress);
                if (recentAttempts == threshold)
                {
                    LogDiagnostic(LogLevel.Information, "IPGuard threshold crossed for {IpAddress}: RecentAttempts={RecentAttempts} Threshold={Threshold} ExistingBlockedRecord={HasBlockedRecord} SiteId={SiteId} RequestedUrl={RequestedUrl}", normalizedIpAddress, recentAttempts, threshold, blockedIp != null, _alias?.SiteId ?? -1, requestedUrl);
                }

                if (blockedIp == null)
                {
                    var country = await _geoLocationService.GetCountryAsync(normalizedIpAddress);

                    blockedIp = new Models.BlockedIp
                    {
                        IpAddress = normalizedIpAddress,
                        CountryLookup = country,
                        ErrorCount = recentAttempts,
                        BlockedDate = DateTime.UtcNow,
                        Reason = $"Exceeded {recentAttempts} 404 errors within the last hour."
                    };

                    try
                    {
                        _IPGuardRepository.AddBlockedIp(blockedIp);
                        _logger.Log(LogLevel.Warning, this, LogFunction.Create, "Blocked IP {IpAddress} after {ErrorCount} 404 attempts", normalizedIpAddress, recentAttempts);
                    }
                    catch (DbUpdateException ex)
                    {
                        LogDiagnostic(LogLevel.Error, ex, "IPGuard blocked IP insert failed for {IpAddress}: ErrorCount={ErrorCount} SiteId={SiteId} Message={ErrorMessage}", normalizedIpAddress, recentAttempts, _alias?.SiteId ?? -1, ex.Message);

                        blockedIp = _IPGuardRepository.GetBlockedIp(normalizedIpAddress);
                        if (blockedIp == null)
                        {
                            LogDiagnostic(LogLevel.Error, "IPGuard blocked IP insert failure left no record for {IpAddress}: ErrorCount={ErrorCount} SiteId={SiteId}", normalizedIpAddress, recentAttempts, _alias?.SiteId ?? -1);
                            throw;
                        }

                        if (blockedIp.ErrorCount < recentAttempts)
                        {
                            blockedIp.ErrorCount = recentAttempts;
                            blockedIp.BlockedDate = DateTime.UtcNow;
                            blockedIp.Reason = $"Exceeded {recentAttempts} 404 errors within the last hour.";
                            if (string.IsNullOrWhiteSpace(blockedIp.CountryLookup) || blockedIp.CountryLookup == IpApiGeoLocationService.LookupError)
                            {
                                blockedIp.CountryLookup = country;
                            }
                            _IPGuardRepository.UpdateBlockedIp(blockedIp);
                            _logger.Log(LogLevel.Warning, this, LogFunction.Update, "Updated blocked IP {IpAddress} to {ErrorCount} 404 attempts", normalizedIpAddress, recentAttempts);
                        }
                    }
                }
                else if (blockedIp.ErrorCount < recentAttempts)
                {
                    blockedIp.ErrorCount = recentAttempts;
                    blockedIp.BlockedDate = DateTime.UtcNow;
                    blockedIp.Reason = $"Exceeded {recentAttempts} 404 errors within the last hour.";
                    if (string.IsNullOrWhiteSpace(blockedIp.CountryLookup) || blockedIp.CountryLookup == IpApiGeoLocationService.LookupError)
                    {
                        blockedIp.CountryLookup = await _geoLocationService.GetCountryAsync(normalizedIpAddress);
                    }
                    _IPGuardRepository.UpdateBlockedIp(blockedIp);
                    _logger.Log(LogLevel.Warning, this, LogFunction.Update, "Updated blocked IP {IpAddress} to {ErrorCount} 404 attempts", normalizedIpAddress, recentAttempts);
                }
            }
        }

        private int GetThreshold()
        {
            var threshold = GetSettingValue(IPGuardSettings.ThresholdSettingName, IPGuardSettings.LegacyThresholdSettingName, IPGuardSettings.DefaultThreshold.ToString());
            return int.TryParse(threshold, out var value) && value > 0 ? value : IPGuardSettings.DefaultThreshold;
        }

        private bool IsEnabled()
        {
            var enabled = GetSettingValue(IPGuardSettings.EnabledSettingName, IPGuardSettings.LegacyEnabledSettingName, IPGuardSettings.DefaultEnabled.ToString());
            return bool.TryParse(enabled, out var value) ? value : IPGuardSettings.DefaultEnabled;
        }

        public bool ExcludeReservedRoutesFromTracking()
        {
            var excludeReservedRoutes = GetSettingValue(IPGuardSettings.ExcludeReservedRoutesSettingName, string.Empty, IPGuardSettings.DefaultExcludeReservedRoutes.ToString());
            return bool.TryParse(excludeReservedRoutes, out var value) ? value : IPGuardSettings.DefaultExcludeReservedRoutes;
        }

        private bool IsDiagnosticModeEnabled()
        {
            var diagnosticMode = GetSettingValue(IPGuardSettings.DiagnosticModeSettingName, string.Empty, IPGuardSettings.DefaultDiagnosticMode.ToString());
            return bool.TryParse(diagnosticMode, out var value) ? value : IPGuardSettings.DefaultDiagnosticMode;
        }

        private static string NormalizeIpAddress(string ipAddress)
        {
            return ipAddress?.Trim().ToLowerInvariant();
        }

        private bool IsAllowedIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return false;
            }

            var allowedIpAddresses = GetSettingValue(IPGuardSettings.AllowedIpAddressSettingName, IPGuardSettings.LegacyAllowedIpAddressSettingName, string.Empty);
            if (string.IsNullOrWhiteSpace(allowedIpAddresses))
            {
                return false;
            }

            var normalizedIpAddress = NormalizeIpAddress(ipAddress);

            return allowedIpAddresses
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(allowedIpAddress => string.Equals(NormalizeIpAddress(allowedIpAddress), normalizedIpAddress, StringComparison.OrdinalIgnoreCase));
        }

        private string GetSettingValue(string settingName, string legacySettingName, string defaultValue)
        {
            var hostSetting = _settingRepository.GetSetting(EntityNames.Host, -1, settingName);
            if (hostSetting != null)
            {
                return hostSetting.SettingValue;
            }

            if (!string.IsNullOrWhiteSpace(legacySettingName))
            {
                var legacyHostSetting = _settingRepository.GetSetting(EntityNames.Host, -1, legacySettingName);
                if (legacyHostSetting != null)
                {
                    return legacyHostSetting.SettingValue;
                }
            }

            var siteId = _alias?.SiteId ?? -1;
            if (siteId != -1)
            {
                var siteSetting = _settingRepository.GetSetting(EntityNames.Site, siteId, settingName);
                if (siteSetting != null)
                {
                    return siteSetting.SettingValue;
                }

                if (!string.IsNullOrWhiteSpace(legacySettingName))
                {
                    var legacySiteSetting = _settingRepository.GetSetting(EntityNames.Site, siteId, legacySettingName);
                    if (legacySiteSetting != null)
                    {
                        return legacySiteSetting.SettingValue;
                    }
                }
            }

            return defaultValue;
        }

        private void LogDiagnostic(LogLevel level, string message, params object[] args)
        {
            if (IsDiagnosticModeEnabled())
            {
                _logger.Log(level, this, LogFunction.Other, message, args);
            }
        }

        private void LogDiagnostic(LogLevel level, Exception exception, string message, params object[] args)
        {
            if (IsDiagnosticModeEnabled())
            {
                _logger.Log(level, this, LogFunction.Other, exception, message, args);
            }
        }

        private bool IsAuthorized(int moduleId, string permission)
        {
            return _userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, permission);
        }
    }
}
