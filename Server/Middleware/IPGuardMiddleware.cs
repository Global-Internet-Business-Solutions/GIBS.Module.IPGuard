using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GIBS.Module.IPGuard.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GIBS.Module.IPGuard.Middleware
{
    public class IPGuardMiddleware
    {
        private readonly RequestDelegate _next;

        public IPGuardMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IServiceScopeFactory serviceScopeFactory)
        {
            var ipAddress = GetClientIpAddress(context);
            if (string.IsNullOrWhiteSpace(ipAddress) || ShouldBypassProtection(ipAddress, context.Request.Path))
            {
                await _next(context);
                return;
            }

            using var scope = serviceScopeFactory.CreateScope();
            var ipGuardService = scope.ServiceProvider.GetRequiredService<ServerIPGuardService>();

            if (!IsLoopbackAddress(ipAddress) && await ipGuardService.IsBlockedAsync(ipAddress))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Access denied.");
                return;
            }

            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status404NotFound)
            {
                var reExecuteFeature = context.Features.Get<IStatusCodeReExecuteFeature>();
                var path = reExecuteFeature?.OriginalPath ?? context.Request.Path.Value ?? string.Empty;
                if (!ShouldTrack404(path))
                {
                    return;
                }

                var query = reExecuteFeature?.OriginalQueryString ?? context.Request.QueryString.Value ?? string.Empty;
                var requestedUrl = string.Concat(
                    context.Request.Scheme,
                    "://",
                    context.Request.Host.Value,
                    context.Request.PathBase.Value,
                    path,
                    query);

                await ipGuardService.Record404Async(ipAddress, requestedUrl);
            }
        }

        private static bool ShouldBypassProtection(string ipAddress, PathString path)
        {
            return IsFrameworkPath(path.Value);
        }

        private static bool ShouldTrack404(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (IsFrameworkPath(path))
            {
                return false;
            }

            return !Path.HasExtension(path);
        }

        private static bool IsFrameworkPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            return path.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/_content", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/_vs", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLoopbackAddress(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out var address) && IPAddress.IsLoopback(address);
        }

        private static string GetClientIpAddress(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var forwardedIp = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
                if (!string.IsNullOrWhiteSpace(forwardedIp))
                {
                    return forwardedIp;
                }
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
