// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Blue.Application.DTOs.Auth;
using System.Text;

namespace Rapsodia.Blue.Application.Services;

public class TenantService
{
    private readonly List<TenantConfig> _tenants = new();

    public TenantConfig CreateTenant(CreateTenantRequest req)
    {
        var limits = req.Plan.ToLower() switch
        {
            "trial" => Plans.Trial,
            "basic" => Plans.SOC_Basic,
            "pro" => Plans.SOC_Pro,
            "pentest" => Plans.Pentest,
            "full" => Plans.Full,
            _ => Plans.Trial
        };

        var tenant = new TenantConfig
        {
            ClientName = req.ClientName,
            ClientEmail = req.ClientEmail,
            Plan = req.Plan,
            Limits = limits,
            ExpiresAt = DateTime.UtcNow.AddDays(req.ValidityDays)
        };

        _tenants.Add(tenant);
        return tenant;
    }

    public string BuildEnvConfig(TenantConfig tenant)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Rapsodia - {tenant.ClientName}");
        sb.AppendLine($"ENV=Production");
        sb.AppendLine($"AUTH_MODE=jwt");
        sb.AppendLine($"TENANT_ID={tenant.Id}");
        sb.AppendLine($"BLUE_URL=https://api.rapsodia.com");
        
        var limits = tenant.Limits;
        sb.AppendLine($"RED_MAX_SCANS={limits.MaxScansPerMonth}");
        sb.AppendLine($"RED_CONCURRENT={limits.MaxConcurrentScans}");
        sb.AppendLine($"RED_EXPLOIT={limits.AllowExploit.ToString().ToLower()}");
        sb.AppendLine($"RED_POST_EXPLOIT={limits.AllowPostExploit.ToString().ToLower()}");
        sb.AppendLine($"VLT_MAX_LABS={limits.MaxLabs}");
        sb.AppendLine($"VLT_MULTI={limits.AllowMultiInstance.ToString().ToLower()}");
        sb.AppendLine($"SLV_MAX_AGENTS={limits.MaxAgents}");
        sb.AppendLine($"SLV_ORCH={limits.AllowOrchestration.ToString().ToLower()}");
        sb.AppendLine($"SLV_OBSIDIAN={limits.AllowObsidian.ToString().ToLower()}");
        sb.AppendLine($"SLV_MULTIPLIER={limits.AllowAgentMultiplier.ToString().ToLower()}");
        
        return sb.ToString();
    }

    public List<TenantConfig> GetActiveTenants() => _tenants.Where(t => t.IsActive).ToList();

    public TenantConfig? GetTenant(string tenantId) => _tenants.FirstOrDefault(t => t.Id == tenantId);

    public bool RevokeTenant(string tenantId)
    {
        var tenant = _tenants.FirstOrDefault(t => t.Id == tenantId);
        if (tenant is null) return false;
        tenant.IsActive = false;
        return true;
    }
}