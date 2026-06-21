// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Blue.Application.Services;

namespace Rapsodia.Blue.Application.Middleware;

public class TenantLimitMiddleware
{
    private readonly RequestDelegate _next;

    public TenantLimitMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantService tenantService)
    {
        var tenantId = context.User?.FindFirst("tenant_id")?.Value;
        
        if (string.IsNullOrEmpty(tenantId))
        {
            await _next(context);
            return;
        }

        var tenant = tenantService.GetTenant(tenantId);
        
        if (tenant is null || !tenant.IsActive)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Tenant not found or inactive",
                message = "Contact support to renew your plan."
            });
            return;
        }

        if (tenant.ExpiresAt < DateTime.UtcNow)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Plan expired",
                expiredAt = tenant.ExpiresAt,
                message = "Contact support to renew."
            });
            return;
        }

        var limits = tenant.Limits;
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method;

        if (path.Contains("/api/exploit") && !limits.AllowExploit)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Exploit not allowed in your plan",
                requiredPlan = "Pro or above"
            });
            return;
        }

        if (path.Contains("/api/lab/create") && limits.MaxLabs == 0)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Labs not included in your plan"
            });
            return;
        }

        if (path.Contains("/api/agent/create") && limits.MaxAgents == 0)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Agents not included in your plan"
            });
            return;
        }

        context.Items["TenantLimits"] = limits;
        await _next(context);
    }
}

public static class TenantLimitMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantLimits(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantLimitMiddleware>();
    }
}