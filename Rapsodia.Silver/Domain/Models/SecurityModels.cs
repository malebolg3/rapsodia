using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rapsodia.Silver.Domain.Models;

public class SecurityAnalysis : BaseEntity
{
    public string LogContent { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string OriginIp { get; set; } = string.Empty;
    public string AnalysisResult { get; set; } = string.Empty;
    public string Severity { get; set; } = "MEDIUM";
    public string AnalysisType { get; set; } = string.Empty;
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? Notes { get; set; }
}