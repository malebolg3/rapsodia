using System;
using System.Collections.Generic;
using Rapsodia.Blue.Domain.Enums;

namespace Rapsodia.Blue.Application.DTOs;

public record CreateAssetRequest(
    string Name,
    int TypeId,
    VulnerabilityEnvironment Environment,
    bool Enabled,
    int? ParentAssetId,
    List<int>? RelatedAssetIds
);

public record EditAssetRequest(
    string? Name,
    int? TypeId,
    VulnerabilityEnvironment? Environment,
    bool? Enabled,
    int? ParentAssetId,
    List<int>? RelatedAssetIds
);

public record AddAssetRelationRequest(
    int RelatedAssetId
);

public record AssetResponse(
    int Id,
    string Name,
    string TypeName,
    VulnerabilityEnvironment Environment,
    bool Enabled,
    DateTime CreatedAt,
    int? ParentAssetId,
    string ParentAssetName,
    List<AssetChildResponse> ChildAssets,
    List<AssetRelatedResponse> RelatedAssets,
    List<VulnResponse> Vulns
);

public record AssetChildResponse(
    int Id,
    string Name,
    string TypeName,
    VulnerabilityEnvironment Environment,
    bool Enabled
);

public record AssetRelatedResponse(
    int Id,
    string Name,
    string TypeName,
    VulnerabilityEnvironment Environment,
    bool Enabled
);