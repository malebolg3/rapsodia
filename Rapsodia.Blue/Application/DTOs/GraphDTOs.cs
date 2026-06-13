namespace Rapsodia.Blue.Application.DTOs;

public class CreateGraphRelationRequest
{
    public int SourceNodeId { get; set; }
    public int TargetNodeId { get; set; }
    public string RelationType { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}

public class EditGraphRelationRequest
{
    public string RelationType { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}

public class GraphRelationDTO
{
    public int Id { get; set; }
    public int SourceNodeId { get; set; }
    public int TargetNodeId { get; set; }
    public string RelationType { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class GraphTopologyDTO
{
    public List<GraphNodeDTO> Nodes { get; set; } = new();
    public List<GraphRelationDTO> Relations { get; set; } = new();
}

public class GraphNodeDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string>? Properties { get; set; }
}

public class GraphPathDTO
{
    public List<GraphNodeDTO> Path { get; set; } = new();
    public int TotalHops { get; set; }
    public double TotalWeight { get; set; }
}

public class GraphCorrelation
{
    public string AssetId { get; set; }
    public string VulnId { get; set; }
    public int Weight { get; set; }

    public GraphCorrelation(string assetId, string vulnId, int weight)
    {
        AssetId = assetId;
        VulnId = vulnId;
        Weight = weight;
    }
}