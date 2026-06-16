using System;
using System.Collections.Generic;
using Rapsodia.Blue.Domain.Enums;

namespace Rapsodia.Blue.Application.DTOs;

public record CreateVulnRequest(
    string Code,
    string Title,
    string Description,
    VulnerabilityLevel Level,
    VulnerabilityEnvironment Environment,
    int? ParentVulnId,
    List<int>? RelatedVulnIds,
    List<int>? AssetIds
);

public record EditVulnRequest(
    string? Code,
    string? Title,
    string? Description,
    VulnerabilityLevel? Level,
    VulnerabilityEnvironment? Environment,
    int? ParentVulnId,
    List<int>? RelatedVulnIds,
    List<int>? AssetIds
);

public record AddVulnToAssetRequest(
    int VulnId,
    string Notes
);

public record UpdateAssetVulnRequest(
    string Status,
    string Notes
);

public record VulnResponse(
    int Id,
    string Code,
    string Title,
    string Description,
    VulnerabilityLevel Level,
    VulnerabilityEnvironment Environment,
    int? ParentVulnId,
    string ParentVulnTitle,
    List<VulnChildResponse> ChildVulns,
    List<VulnRelatedResponse> RelatedVulns,
    List<VulnAssetResponse> Assets,
    DateTime CreatedAt
);

public record VulnChildResponse(
    int Id,
    string Code,
    string Title,
    VulnerabilityLevel Level,
    VulnerabilityEnvironment Environment
);

public record VulnRelatedResponse(
    int Id,
    string Code,
    string Title,
    VulnerabilityLevel Level,
    VulnerabilityEnvironment Environment,
    string RelationType
);

public record VulnAssetResponse(
    int AssetId,
    string AssetName,
    string AssetType,
    string Status,
    DateTime DiscoveredAt
);