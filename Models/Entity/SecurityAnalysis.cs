namespace Rapsodia.Models.Entity
{
    public class SecurityAnalysis : BaseEntity
    {
        public string LogContent { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string OriginIp { get; set; } = string.Empty;
        public string AnalysisResult { get; set; } = string.Empty;
        public string Severity { get; set; } = "MEDIUM";
        public string AnalysisType { get; set; } = string.Empty;
        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedAt { get; set; }
        public string? Notes { get; set; }
    }
}
