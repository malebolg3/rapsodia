using Rapsodia.Silver.Application.DTOs;

namespace Rapsodia.Silver.Domain.Interfaces;

public interface ITelemetryService
{
    Task<ApiResponse<TelemetryResponse>> CreateAsync(TelemetryRequest request, string? apiKey);
    Task<ApiResponse<List<TelemetryResponse>>> GetLatestAsync(int count = 50);
    Task<ApiResponse<TelemetryResponse>> GetByIdAsync(int id);
    Task<ApiResponse<TelemetryStats>> GetStatsAsync();
    Task<ApiResponse<List<TelemetryResponse>>> GetSinceAsync(DateTime? since);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<string> AnalyzeThreatAsync(string logContent, string environment);
}