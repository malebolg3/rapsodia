using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rapsodia.Blue.Application.DTOs.Auth;
using Rapsodia.Blue.Application.Services;

namespace Rapsodia.Blue.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly TenantService _tenantService;

    public AdminController(TenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpPost("tenants")]
    public IActionResult CreateTenant([FromBody] CreateTenantRequest req)
    {
        var tenant = _tenantService.CreateTenant(req);
        var envConfig = _tenantService.BuildEnvConfig(tenant);
        
        return Ok(new
        {
            success = true,
            data = new
            {
                tenant.Id,
                tenant.ClientName,
                tenant.Plan,
                tenant.ExpiresAt,
                envConfig
            }
        });
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
        if (tenant is null)
            return NotFound(new { success = false, message = "Tenant not found" });
        
        return Ok(new { success = true, data = tenant });
    }

    [HttpDelete("tenants/{tenantId}")]
    public IActionResult RevokeTenant(string tenantId)
    {
        var result = _tenantService.RevokeTenant(tenantId);
        return result 
            ? Ok(new { success = true, message = "Tenant revoked" })
            : NotFound(new { success = false, message = "Tenant not found" });
    }

    [HttpPost("build-package")]
    public IActionResult BuildPackage([FromBody] PackageBuilderRequest req)
    {
        var tenant = _tenantService.CreateTenant(new CreateTenantRequest
        {
            ClientName = req.ClientName,
            ClientEmail = req.ClientEmail,
            Plan = "custom",
            ValidityDays = req.ValidityDays
        });

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

        return Ok(new
        {
            success = true,
            data = new
            {
                tenant.Id,
                tenant.ClientName,
                envConfig,
                startCommands
            }
        });
    }

    [HttpPost("provision")]
    public async Task<IActionResult> ProvisionEnvironment([FromBody] ProvisionRequest req)
    {
        var tenant = _tenantService.CreateTenant(new CreateTenantRequest
        {
            ClientName = req.ClientName,
            ClientEmail = req.ClientEmail,
            Plan = req.Plan,
            ValidityDays = req.ValidityDays
        });

        try
        {
            var http = new HttpClient();
            var dockerConfig = new
            {
                labs = req.Labs.Select(l => new { name = l.Name, image = l.Image, network = l.Network }),
                adminUser = req.AdminUser,
                adminPassword = req.AdminPassword
            };

            var response = await http.PostAsJsonAsync("http://localhost:5075/api/Lab/provision", dockerConfig);
            
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new { success = false, message = "Violet provisioning failed" });
            }

            var result = await response.Content.ReadFromJsonAsync<ProvisionResult>();

            return Ok(new
            {
                success = true,
                data = new
                {
                    tenant.Id,
                    dashboardUrl = $"https://{tenant.Id}.rapsodia.com",
                    adminUser = req.AdminUser,
                    adminPassword = req.AdminPassword,
                    containers = result?.Containers ?? new List<ProvisionContainerInfo>(),
                    expiresAt = tenant.ExpiresAt
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = $"Provisioning error: {ex.Message}" });
        }
    }

    private string BuildStartCommands(PackageBuilderRequest req)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# Comandos para iniciar os servicos:\n");
        
        sb.AppendLine("docker run -d --name blue --env-file .env -p 5073:5073 rapsodia/blue:latest");
        
        if (req.IncludeRed)
            sb.AppendLine("docker run -d --name red --env-file .env -p 5074:5074 rapsodia/red:latest");
        
        if (req.IncludeViolet)
        {
            if (req.VioletMultiInstance)
                sb.AppendLine($"docker run -d --name violet --env-file .env -p 5075:5075 rapsodia/violet:latest --scale {req.VioletMaxInstances}");
            else
                sb.AppendLine("docker run -d --name violet --env-file .env -p 5075:5075 rapsodia/violet:latest");
        }
        
        if (req.IncludeSilver)
            sb.AppendLine("docker run -d --name silver --env-file .env -p 5076:5076 rapsodia/silver:latest");
        
        return sb.ToString();
    }
}