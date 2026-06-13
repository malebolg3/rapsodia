using System;

namespace Rapsodia.Blue.Domain.Entities;

public class AssetVuln
{
    public int AssetId { get; init; }
    public Asset Asset { get; protected set; } = null!;
    public int VulnId { get; init; }
    public Vuln Vuln { get; protected set; } = null!;
    public DateTime DiscoveredAt { get; protected set; } = DateTime.UtcNow;
    public string Status { get; protected set; } = "Open";
    public string Notes { get; protected set; } = string.Empty;
    public DateTime? RemediatedAt { get; protected set; }

    public AssetVuln() { }

    public AssetVuln(Asset asset, Vuln vuln, string status = "Open", string notes = "")
    {
        Asset = asset;
        Vuln = vuln;
        Status = status;
        Notes = notes;
    }

    public void Update(string? status = null, string? notes = null)
    {
        if (status != null) Status = status;
        if (notes != null) Notes = notes;
    }
}