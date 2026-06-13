using System.Security.Claims;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IDashboardService
{
    Task<Result<DashboardDTO>> GetSummaryAsync(ClaimsPrincipal user, CancellationToken ct);
    Task<Result<AssetStatsDTO>> GetAssetStatsAsync(CancellationToken ct);
    Task<Result<VulnTrendDTO>> GetVulnTrendAsync(TrendFilterDTO filter, CancellationToken ct);
    Task<Result<RiskMatrixDTO>> GetRiskMatrixAsync(CancellationToken ct);
    Task<Result<ComplianceStatusDTO>> GetComplianceStatusAsync(CancellationToken ct);
    Task<Result<PagedResult<RecentActivityDTO>>> GetRecentActivityAsync(ActivityFilterDTO filter, CancellationToken ct);
}