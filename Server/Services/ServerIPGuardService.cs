using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

                BlockedIp.IpAddress = TryNormalizeValidIpAddress(BlockedIp.IpAddress, out var normalizedIpAddress)
                    ? normalizedIpAddress
                    : throw new InvalidOperationException($"The IP address '{BlockedIp.IpAddress}' is not a valid IPv4 or IPv6 address.");

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
                BlockedIp.IpAddress = TryNormalizeValidIpAddress(BlockedIp.IpAddress, out var normalizedIpAddress)
                    ? normalizedIpAddress
                    : throw new InvalidOperationException($"The IP address '{BlockedIp.IpAddress}' is not a valid IPv4 or IPv6 address.");
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
                return Task.FromResult(_IPGuardRepository.GetErrorAttempts().Take(500).ToList());
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
                if (TryNormalizeValidIpAddress(ipAddress, out var normalizedIpAddress))
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

        public async Task<Models.BlockedIp> AddToBlockedAsync(string ipAddress, int ModuleId)
        {
            if (!IsAuthorized(ModuleId, PermissionNames.Edit))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized BlockedIp Add Attempt {IpAddress} {ModuleId}", ipAddress, ModuleId);
                return null;
            }

            if (!TryNormalizeValidIpAddress(ipAddress, out var normalizedIpAddress))
            {
                throw new InvalidOperationException($"The IP address '{ipAddress}' is not a valid IPv4 or IPv6 address.");
            }

            var blockedIp = _IPGuardRepository.GetBlockedIp(normalizedIpAddress);
            var attemptCount = _IPGuardRepository.GetErrorAttemptCount(normalizedIpAddress);

            if (blockedIp == null)
            {
                blockedIp = new Models.BlockedIp
                {
                    IpAddress = normalizedIpAddress,
                    CountryLookup = await _geoLocationService.GetCountryAsync(normalizedIpAddress),
                    ErrorCount = attemptCount,
                    BlockedDate = DateTime.UtcNow,
                    Reason = "Administrator manual block."
                };

                try
                {
                    blockedIp = _IPGuardRepository.AddBlockedIp(blockedIp);
                    _logger.Log(LogLevel.Warning, this, LogFunction.Create, "Blocked IP {IpAddress} manually after {ErrorCount} recorded 404 attempts", normalizedIpAddress, attemptCount);
                }
                catch (DbUpdateException ex)
                {
                    var duplicateBlockedIp = _IPGuardRepository.GetBlockedIp(normalizedIpAddress);
                    if (duplicateBlockedIp == null)
                    {
                        throw;
                    }

                    blockedIp = duplicateBlockedIp;
                    _logger.Log(LogLevel.Information, this, LogFunction.Other, "Concurrent manual block detected for IP {IpAddress}", normalizedIpAddress);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(blockedIp.CountryLookup) || blockedIp.CountryLookup == IpApiGeoLocationService.LookupError)
                {
                    blockedIp.CountryLookup = await _geoLocationService.GetCountryAsync(normalizedIpAddress);
                }

                blockedIp.ErrorCount = attemptCount;
                blockedIp.BlockedDate = DateTime.UtcNow;
                blockedIp.Reason = "Administrator manual block.";
                blockedIp = _IPGuardRepository.UpdateBlockedIp(blockedIp);
                _logger.Log(LogLevel.Warning, this, LogFunction.Update, "Updated blocked IP {IpAddress} manually after {ErrorCount} recorded 404 attempts", normalizedIpAddress, attemptCount);
            }

            _IPGuardRepository.DeleteErrorAttempts(normalizedIpAddress);
            return blockedIp;
        }

        public Task<bool> IsBlockedAsync(string ipAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ipAddress) || !IsEnabled())
                {
                    return Task.FromResult(false);
                }

                if (!TryNormalizeValidIpAddress(ipAddress, out var normalizedIpAddress))
                {
                    return Task.FromResult(false);
                }

                if (IsAllowedIpAddress(normalizedIpAddress))
                {
                    return Task.FromResult(false);
                }

                return Task.FromResult(_IPGuardRepository.IsBlocked(normalizedIpAddress));
            }
            catch (Exception ex) when (ShouldBypassDuringInitialization(ex))
            {
                LogInitializationBypass("blocked IP check", ex);
                return Task.FromResult(false);
            }
        }

        public async Task Record404Async(string ipAddress, string requestedUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ipAddress))
                {
                    return;
                }

                if (!TryNormalizeValidIpAddress(ipAddress, out var normalizedIpAddress))
                {
                    return;
                }

                if (_IPGuardRepository.IsBlocked(normalizedIpAddress))
                {
                    _IPGuardRepository.DeleteErrorAttempts(normalizedIpAddress);
                    return;
                }

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
                var blockWindow = GetBlockWindow();
                var recentAttempts = _IPGuardRepository.GetRecentErrorAttemptCount(normalizedIpAddress, DateTime.UtcNow.Subtract(blockWindow));
                var isAllowedIpAddress = IsAllowedIpAddress(normalizedIpAddress);

                if (recentAttempts >= threshold && !isAllowedIpAddress)
                {
                    var blockedIp = _IPGuardRepository.GetBlockedIp(normalizedIpAddress);

                    if (blockedIp == null)
                    {
                        LogDiagnostic(LogLevel.Information, "IPGuard threshold crossed for {IpAddress}: RecentAttempts={RecentAttempts} Threshold={Threshold} ExistingBlockedRecord={HasBlockedRecord} SiteId={SiteId} RequestedUrl={RequestedUrl}", normalizedIpAddress, recentAttempts, threshold, false, _alias?.SiteId ?? -1, requestedUrl);

                        var country = await _geoLocationService.GetCountryAsync(normalizedIpAddress);

                        blockedIp = new Models.BlockedIp
                        {
                            IpAddress = normalizedIpAddress,
                            CountryLookup = country,
                            ErrorCount = recentAttempts,
                            BlockedDate = DateTime.UtcNow,
                            Reason = $"Exceeded {recentAttempts} 404 errors within the last {FormatWindowDescription(blockWindow)}."
                        };

                        try
                        {
                            _IPGuardRepository.AddBlockedIp(blockedIp);
                            _logger.Log(LogLevel.Warning, this, LogFunction.Create, "Blocked IP {IpAddress} after {ErrorCount} 404 attempts", normalizedIpAddress, recentAttempts);
                        }
                        catch (DbUpdateException ex)
                        {
                            blockedIp = _IPGuardRepository.GetBlockedIp(normalizedIpAddress);
                            if (blockedIp == null)
                            {
                                LogDiagnostic(LogLevel.Error, ex, "IPGuard blocked IP insert failed for {IpAddress}: ErrorCount={ErrorCount} SiteId={SiteId} Message={ErrorMessage}", normalizedIpAddress, recentAttempts, _alias?.SiteId ?? -1, ex.Message);
                                LogDiagnostic(LogLevel.Error, "IPGuard blocked IP insert failure left no record for {IpAddress}: ErrorCount={ErrorCount} SiteId={SiteId}", normalizedIpAddress, recentAttempts, _alias?.SiteId ?? -1);
                                throw;
                            }

                            LogDiagnostic(LogLevel.Information, "IPGuard concurrent blocked IP insert detected for {IpAddress}: ErrorCount={ErrorCount} SiteId={SiteId} ExistingBlockedIpId={BlockedIpId}", normalizedIpAddress, recentAttempts, _alias?.SiteId ?? -1, blockedIp.BlockedIpId);

                            if (blockedIp.ErrorCount < recentAttempts)
                            {
                                blockedIp.ErrorCount = recentAttempts;
                                blockedIp.BlockedDate = DateTime.UtcNow;
                                blockedIp.Reason = $"Exceeded {recentAttempts} 404 errors within the last {FormatWindowDescription(blockWindow)}.";
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
                        blockedIp.Reason = $"Exceeded {recentAttempts} 404 errors within the last {FormatWindowDescription(blockWindow)}.";
                        if (string.IsNullOrWhiteSpace(blockedIp.CountryLookup) || blockedIp.CountryLookup == IpApiGeoLocationService.LookupError)
                        {
                            blockedIp.CountryLookup = await _geoLocationService.GetCountryAsync(normalizedIpAddress);
                        }
                        _IPGuardRepository.UpdateBlockedIp(blockedIp);
                        _logger.Log(LogLevel.Warning, this, LogFunction.Update, "Updated blocked IP {IpAddress} to {ErrorCount} 404 attempts", normalizedIpAddress, recentAttempts);
                    }

                    _IPGuardRepository.DeleteErrorAttempts(normalizedIpAddress);
                }
            }
            catch (Exception ex) when (ShouldBypassDuringInitialization(ex))
            {
                LogInitializationBypass("404 tracking", ex);
            }
        }

        private int GetThreshold()
        {
            var threshold = GetSettingValue(IPGuardSettings.ThresholdSettingName, IPGuardSettings.LegacyThresholdSettingName, IPGuardSettings.DefaultThreshold.ToString());
            return int.TryParse(threshold, out var value) && value > 0 ? value : IPGuardSettings.DefaultThreshold;
        }

        private TimeSpan GetBlockWindow()
        {
            var blockWindowHours = GetSettingValue(IPGuardSettings.BlockWindowHoursSettingName, string.Empty, IPGuardSettings.DefaultBlockWindowHours.ToString());
            return int.TryParse(blockWindowHours, out var value) && value > 0
                ? TimeSpan.FromHours(value)
                : TimeSpan.FromHours(IPGuardSettings.DefaultBlockWindowHours);
        }

        private static string FormatWindowDescription(TimeSpan blockWindow)
        {
            var totalHours = Math.Max(1, (int)Math.Round(blockWindow.TotalHours));
            return totalHours == 1 ? "hour" : $"{totalHours} hours";
        }

        private bool IsEnabled()
        {
            var enabled = GetSettingValue(IPGuardSettings.EnabledSettingName, IPGuardSettings.LegacyEnabledSettingName, IPGuardSettings.DefaultEnabled.ToString());
            return bool.TryParse(enabled, out var value) ? value : IPGuardSettings.DefaultEnabled;
        }

        public bool ExcludeReservedRoutesFromTracking()
        {
            try
            {
                var excludeReservedRoutes = GetSettingValue(IPGuardSettings.ExcludeReservedRoutesSettingName, string.Empty, IPGuardSettings.DefaultExcludeReservedRoutes.ToString());
                return bool.TryParse(excludeReservedRoutes, out var value) ? value : IPGuardSettings.DefaultExcludeReservedRoutes;
            }
            catch (Exception ex) when (ShouldBypassDuringInitialization(ex))
            {
                LogInitializationBypass("reserved route lookup", ex);
                return IPGuardSettings.DefaultExcludeReservedRoutes;
            }
        }

        private bool IsDiagnosticModeEnabled()
        {
            var diagnosticMode = GetSettingValue(IPGuardSettings.DiagnosticModeSettingName, string.Empty, IPGuardSettings.DefaultDiagnosticMode.ToString());
            return bool.TryParse(diagnosticMode, out var value) ? value : IPGuardSettings.DefaultDiagnosticMode;
        }

        private static string NormalizeIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return null;
            }

            var candidate = ipAddress.Trim();
            return IPAddress.TryParse(candidate, out var parsedIpAddress)
                ? NormalizeParsedIpAddress(parsedIpAddress)
                : candidate.ToLowerInvariant();
        }

        private static bool TryNormalizeValidIpAddress(string ipAddress, out string normalizedIpAddress)
        {
            normalizedIpAddress = null;

            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return false;
            }

            var candidate = ipAddress.Trim();
            if (!IPAddress.TryParse(candidate, out var parsedIpAddress))
            {
                return false;
            }

            normalizedIpAddress = NormalizeParsedIpAddress(parsedIpAddress);
            return true;
        }

        private static string NormalizeParsedIpAddress(IPAddress ipAddress)
        {
            var normalizedAddress = ipAddress.IsIPv4MappedToIPv6 ? ipAddress.MapToIPv4() : ipAddress;
            return normalizedAddress.ToString().ToLowerInvariant();
        }

        private bool IsAllowedIpAddress(string ipAddress)
        {
            if (!TryNormalizeValidIpAddress(ipAddress, out var normalizedIpAddress))
            {
                return false;
            }

            var allowedIpAddresses = GetSettingValue(IPGuardSettings.AllowedIpAddressSettingName, IPGuardSettings.LegacyAllowedIpAddressSettingName, string.Empty);
            if (string.IsNullOrWhiteSpace(allowedIpAddresses))
            {
                return false;
            }

            return allowedIpAddresses
                .Split(new[] { ',', ';', '\r', '\n', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(allowedIpAddress => TryNormalizeValidIpAddress(allowedIpAddress, out var normalizedAllowedIpAddress)
                    && string.Equals(normalizedAllowedIpAddress, normalizedIpAddress, StringComparison.OrdinalIgnoreCase));
        }

        private string GetSettingValue(string settingName, string legacySettingName, string defaultValue)
        {
            var settingValue = GetScopedSettingValue(EntityNames.Host, -1, settingName, legacySettingName);
            if (settingValue != null)
            {
                return settingValue;
            }

            var siteId = _alias?.SiteId ?? -1;
            if (siteId != -1)
            {
                settingValue = GetScopedSettingValue(EntityNames.Site, siteId, settingName, legacySettingName);
                if (settingValue != null)
                {
                    return settingValue;
                }
            }

            foreach (var configuredSiteId in _settingRepository.GetEntityIds(EntityNames.Site).OrderBy(id => id))
            {
                if (configuredSiteId == siteId)
                {
                    continue;
                }

                settingValue = GetScopedSettingValue(EntityNames.Site, configuredSiteId, settingName, legacySettingName);
                if (settingValue != null)
                {
                    return settingValue;
                }
            }

            return defaultValue;
        }

        private string GetScopedSettingValue(string entityName, int entityId, string settingName, string legacySettingName)
        {
            var setting = _settingRepository.GetSetting(entityName, entityId, settingName);
            if (setting != null)
            {
                return setting.SettingValue;
            }

            if (!string.IsNullOrWhiteSpace(legacySettingName))
            {
                var legacySetting = _settingRepository.GetSetting(entityName, entityId, legacySettingName);
                if (legacySetting != null)
                {
                    return legacySetting.SettingValue;
                }
            }

            return null;
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

        private bool ShouldBypassDuringInitialization(Exception exception)
        {
            while (exception != null)
            {
                if (exception is InvalidOperationException invalidOperationException
                    && invalidOperationException.Message.Contains("No database provider has been configured for this DbContext", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                exception = exception.InnerException;
            }

            return false;
        }

        private void LogInitializationBypass(string operation, Exception exception)
        {
            _logger.Log(LogLevel.Warning, this, LogFunction.Other, "IPGuard skipped {Operation} because tenant persistence is not ready. SiteId={SiteId} Message={ErrorMessage}", operation, _alias?.SiteId ?? -1, exception.Message);
        }

        private bool IsAuthorized(int moduleId, string permission)
        {
            return _userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, permission);
        }
    }
}
