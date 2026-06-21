// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.DTOs.Auth;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Application.Services;

namespace Rapsodia.Blue.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[EnableRateLimiting("Auth")]
public class AdminController : ControllerBase
{
    private readonly TenantService _tenantService;
    private readonly IUserService _userService;

    public AdminController(TenantService tenantService, IUserService userService)
    {
        _tenantService = tenantService;
        _userService = userService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] UserFilterDTO filter, CancellationToken ct)
    {
        var result = await _userService.ListAsync(filter, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req, CancellationToken ct)
    {
        var result = await _userService.CreateAsync(req, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] EditUserRequest req, CancellationToken ct)
    {
        var result = await _userService.UpdateAsync(id, req, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("users/{id}/permissions")]
    public async Task<IActionResult> SetPermissions(int id, [FromBody] SetPermissionsRequest req, CancellationToken ct)
    {
        var result = await _userService.SetPermissionsAsync(id, req.Role, req.AllowedModules, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("users/{id}/disable")]
    public async Task<IActionResult> DisableUser(int id, CancellationToken ct)
    {
        var result = await _userService.DisableAsync(id, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("users/{id}/enable")]
    public async Task<IActionResult> EnableUser(int id, CancellationToken ct)
    {
        var result = await _userService.EnableAsync(id, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("tenants")]
    public IActionResult CreateTenant([FromBody] CreateTenantRequest req)
    {
        var tenant = _tenantService.CreateTenant(req);
        var envConfig = _tenantService.BuildEnvConfig(tenant);
        return Ok(new { success = true, data = new { tenant.Id, tenant.ClientName, tenant.Plan, tenant.ExpiresAt, envConfig } });
    }

    [HttpGet("tenants")]
    public IActionResult GetTenants()
    {
        var tenants = _tenantService.GetActiveTenants();
        return Ok(new { success = true, data = tenants });
    }

    [HttpGet("tenants/{tenantId}")]
    public IActionResult GetTenant(string tenantId)
    {
        var tenant = _tenantService.GetTenant(tenantId);
        if (tenant is null) return NotFound(new { success = false, message = "Tenant not found" });
        return Ok(new { success = true, data = tenant });
    }

    [HttpDelete("tenants/{tenantId}")]
    public IActionResult RevokeTenant(string tenantId)
    {
        var result = _tenantService.RevokeTenant(tenantId);
        return result ? Ok(new { success = true, message = "Tenant revoked" }) : NotFound(new { success = false, message = "Tenant not found" });
    }

    [HttpPost("build-package")]
    public IActionResult BuildPackage([FromBody] PackageBuilderRequest req)
    {
        var tenant = _tenantService.CreateTenant(new CreateTenantRequest { ClientName = req.ClientName, ClientEmail = req.ClientEmail, Plan = "custom", ValidityDays = req.ValidityDays });
        tenant.Limits = new PlanLimits
        {
            MaxScansPerMonth = req.IncludeRed ? (req.RedExploit ? 20 : 5) : 0,
            MaxConcurrentScans = req.IncludeRed ? 3 : 0,
            AllowExploit = req.RedExploit,
            AllowPostExploit = req.RedPostExploit,
            MaxLabs = req.IncludeViolet ? req.VioletMaxInstances : 0,
            AllowMultiInstance = req.VioletMultiInstance,
            MaxAgents = req.IncludeSilver ? (req.SilverAgents ? 5 : 0) : 0,
            AllowOrchestration = req.SilverOrchestration,
            AllowObsidian = req.IncludeSilver,
            AllowAgentMultiplier = req.SilverAgents
        };
        var envConfig = _tenantService.BuildEnvConfig(tenant);
        var startCommands = BuildStartCommands(req);
        return Ok(new { success = true, data = new { tenant.Id, tenant.ClientName, envConfig, startCommands } });
    }

    [HttpPost("provision")]
    public async Task<IActionResult> ProvisionEnvironment([FromBody] ProvisionRequest req)
    {
        var tenant = _tenantService.CreateTenant(new CreateTenantRequest { ClientName = req.ClientName, ClientEmail = req.ClientEmail, Plan = req.Plan, ValidityDays = req.ValidityDays });
        try
        {
            var http = new HttpClient();
            var dockerConfig = new { labs = req.Labs.Select(l => new { name = l.Name, image = l.Image, network = l.Network }), adminUser = req.AdminUser, adminPassword = req.AdminPassword };
            var response = await http.PostAsJsonAsync("http://localhost:5075/api/Lab/provision", dockerConfig);
            if (!response.IsSuccessStatusCode) return BadRequest(new { success = false, message = "Violet provisioning failed" });
            var result = await response.Content.ReadFromJsonAsync<ProvisionResult>();
            return Ok(new { success = true, data = new { tenant.Id, dashboardUrl = $"https://{tenant.Id}.rapsodia.com", adminUser = req.AdminUser, adminPassword = req.AdminPassword, containers = result?.Containers ?? new List<ProvisionContainerInfo>(), expiresAt = tenant.ExpiresAt } });
        }
        catch (Exception ex) { return BadRequest(new { success = false, message = $"Provisioning error: {ex.Message}" }); }
    }

    private string BuildStartCommands(PackageBuilderRequest req)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Comandos para iniciar os servicos:\n");
        sb.AppendLine("docker run -d --name blue --env-file .env -p 5073:5073 rapsodia/blue:latest");
        if (req.IncludeRed) sb.AppendLine("docker run -d --name red --env-file .env -p 5074:5074 rapsodia/red:latest");
        if (req.IncludeViolet) sb.AppendLine($"docker run -d --name violet --env-file .env -p 5075:5075 rapsodia/violet:latest{(req.VioletMultiInstance ? $" --scale {req.VioletMaxInstances}" : "")}");
        if (req.IncludeSilver) sb.AppendLine("docker run -d --name silver --env-file .env -p 5076:5076 rapsodia/silver:latest");
        return sb.ToString();
    }
}

public class SetPermissionsRequest
{
    public string Role { get; set; } = "Analyst";
    public string AllowedModules { get; set; } = "blue";
}