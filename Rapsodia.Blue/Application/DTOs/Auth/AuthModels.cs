namespace Rapsodia.Blue.Application.DTOs.Auth;

public class AuthorizeRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string ExpiresIn { get; set; } = "8h";
}

public class Verify2FARequest
{
    public string Username { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class AuthorizeResponse
{
    public bool Requires2FA { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

public class SessionInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];
    public string Username { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime AuthorizedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public string AuthorizedBy { get; set; } = "admin";
    public string IP { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class TenantConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..12];
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string Plan { get; set; } = "trial";
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
    public PlanLimits Limits { get; set; } = Plans.Trial;
    public bool IsActive { get; set; } = true;
}

public class PlanLimits
{
    public int MaxScansPerMonth { get; set; }
    public int MaxConcurrentScans { get; set; }
    public bool AllowExploit { get; set; }
    public bool AllowPostExploit { get; set; }
    public int MaxLabs { get; set; }
    public bool AllowMultiInstance { get; set; }
    public int MaxAgents { get; set; }
    public bool AllowOrchestration { get; set; }
    public bool AllowObsidian { get; set; }
    public bool AllowAgentMultiplier { get; set; }
}

public static class Plans
{
    public static PlanLimits Trial => new()
    {
        MaxScansPerMonth = 1, MaxConcurrentScans = 1,
        MaxLabs = 1,
        MaxAgents = 0
    };
    
    public static PlanLimits SOC_Basic => new()
    {
        MaxScansPerMonth = 5, MaxConcurrentScans = 2,
        MaxLabs = 3,
        MaxAgents = 0
    };
    
    public static PlanLimits SOC_Pro => new()
    {
        MaxScansPerMonth = 20, MaxConcurrentScans = 5, AllowExploit = true,
        MaxLabs = 10, AllowMultiInstance = true,
        MaxAgents = 2, AllowOrchestration = true, AllowObsidian = true
    };
    
    public static PlanLimits Pentest => new()
    {
        MaxScansPerMonth = int.MaxValue, MaxConcurrentScans = 5,
        AllowExploit = true, AllowPostExploit = true,
        MaxLabs = 0,
        MaxAgents = 0
    };
    
    public static PlanLimits Full => new()
    {
        MaxScansPerMonth = int.MaxValue, MaxConcurrentScans = 10,
        AllowExploit = true, AllowPostExploit = true,
        MaxLabs = 20, AllowMultiInstance = true,
        MaxAgents = 5, AllowOrchestration = true, AllowObsidian = true,
        AllowAgentMultiplier = true
    };
}

public class CreateTenantRequest
{
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string Plan { get; set; } = "trial";
    public int ValidityDays { get; set; } = 7;
}

public class PackageBuilderRequest
{
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public bool IncludeRed { get; set; }
    public bool RedScanWorker { get; set; }
    public bool RedExploit { get; set; }
    public bool RedPostExploit { get; set; }
    public bool IncludeViolet { get; set; }
    public bool VioletMultiInstance { get; set; }
    public int VioletMaxInstances { get; set; } = 1;
    public bool IncludeSilver { get; set; }
    public bool SilverOrchestration { get; set; }
    public bool SilverAgents { get; set; }
    public bool Enable2FA { get; set; } = true;
    public string TwoFAMethod { get; set; } = "email";
    public string AllowedIPs { get; set; } = string.Empty;
    public int ValidityDays { get; set; } = 30;
}

public class ProvisionRequest
{
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string Plan { get; set; } = "trial";
    public int ValidityDays { get; set; } = 7;
    public string AdminUser { get; set; } = "admin";
    public string AdminPassword { get; set; } = "Admin@123";
    public List<ProvisionLabConfig> Labs { get; set; } = new();
}

public class ProvisionLabConfig
{
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = "kalilinux/kali-rolling:latest";
    public string Network { get; set; } = "isolated";
}

public class ProvisionResult
{
    public List<ProvisionContainerInfo> Containers { get; set; } = new();
}

public class ProvisionContainerInfo
{
    public string Name { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}