using System.Collections.Generic;
using Rapsodia.Blue.Domain.Enums;

namespace Rapsodia.Blue.Domain.Entities;

public class Vuln : BaseEntity
{
    public string Code { get; protected set; } = string.Empty;
    public string Title { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public VulnerabilityLevel Level { get; protected set; }
    public VulnerabilityEnvironment Environment { get; protected set; }
    public int? ParentVulnId { get; protected set; }
    public Vuln? ParentVuln { get; protected set; }
    public ICollection<Vuln> ChildVulns { get; protected set; } = new List<Vuln>();
    public ICollection<Vuln> RelatedVulns { get; protected set; } = new List<Vuln>();
    public ICollection<AssetVuln> AssetVulns { get; protected set; } = new List<AssetVuln>();

    public Vuln() { }

    public Vuln(string code, string title, string description, VulnerabilityLevel level, VulnerabilityEnvironment environment, int? parentVulnId = null)
    {
        Code = code;
        Title = title;
        Description = description;
        Level = level;
        Environment = environment;
        ParentVulnId = parentVulnId;
    }

    public void Update(string? code = null, string? title = null, string? description = null, VulnerabilityLevel? level = null, VulnerabilityEnvironment? environment = null, int? parentVulnId = null)
    {
        if (code != null) Code = code;
        if (title != null) Title = title;
        if (description != null) Description = description;
        if (level.HasValue) Level = level.Value;
        if (environment.HasValue) Environment = environment.Value;
        if (parentVulnId.HasValue) ParentVulnId = parentVulnId.Value;
        MarkAsUpdated();
    }
}