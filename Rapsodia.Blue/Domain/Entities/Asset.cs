using System.Collections.Generic;
using Rapsodia.Blue.Domain.Enums;

namespace Rapsodia.Blue.Domain.Entities;

public class Asset : BaseEntity
{
    public string Name { get; protected set; } = string.Empty;
    public int AssetTypeId { get; protected set; }
    public AssetType AssetType { get; protected set; } = null!;
    public VulnerabilityEnvironment Environment { get; protected set; }
    public bool IsEnabled { get; protected set; }
    public int? ParentAssetId { get; protected set; }
    public Asset? ParentAsset { get; protected set; }
    public ICollection<Asset> ChildAssets { get; protected set; } = new List<Asset>();
    public ICollection<Asset> RelatedAssets { get; protected set; } = new List<Asset>();
    public ICollection<AssetVuln> AssetVulns { get; protected set; } = new List<AssetVuln>();

    public Asset() { }

    public Asset(string name, int assetTypeId, VulnerabilityEnvironment environment, bool isEnabled, int? parentAssetId = null)
    {
        Name = name;
        AssetTypeId = assetTypeId;
        Environment = environment;
        IsEnabled = isEnabled;
        ParentAssetId = parentAssetId;
    }

    public void Update(string? name = null, int? assetTypeId = null, VulnerabilityEnvironment? environment = null, bool? isEnabled = null, int? parentAssetId = null)
    {
        if (name != null) Name = name;
        if (assetTypeId.HasValue) AssetTypeId = assetTypeId.Value;
        if (environment.HasValue) Environment = environment.Value;
        if (isEnabled.HasValue) IsEnabled = isEnabled.Value;
        if (parentAssetId.HasValue) ParentAssetId = parentAssetId.Value;
        MarkAsUpdated();
    }
}