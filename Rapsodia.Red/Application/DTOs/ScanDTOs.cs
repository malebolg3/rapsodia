namespace Rapsodia.Red.Application.DTOs;

public class ScanRequest
{
    public string Target { get; set; } = string.Empty;
    public string ScanType { get; set; } = "full";
    public List<int>? Ports { get; set; }
    public Dictionary<string, string>? Options { get; set; }
}

public class ScanResultDTO
{
    public Guid ScanId { get; set; }
    public string Target { get; set; } = string.Empty;
    public string ScanType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<ScanFindingDTO> Findings { get; set; } = new();
}

public class ScanFindingDTO
{
    public int Port { get; set; }
    public string Service { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public List<string>? Vulnerabilities { get; set; }
}

public class ScanFilterDTO
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
    public string? ScanType { get; set; }
}