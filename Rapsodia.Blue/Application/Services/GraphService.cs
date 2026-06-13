using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Services;

public class GraphService : IGraphService
{
    private readonly List<GraphNodeDTO> _nodes = new();
    private readonly List<GraphRelationDTO> _relations = new();
    private int _nextId = 1;

    public Task<Result<GraphTopologyDTO>> GetTopologyAsync(CancellationToken ct)
    {
        return Task.FromResult(Result<GraphTopologyDTO>.Ok(new GraphTopologyDTO
        {
            Nodes = _nodes.Where(n => n.Id > 0).ToList(),
            Relations = _relations.ToList()
        }));
    }

    public Task<Result<GraphPathDTO>> FindPathAsync(int sourceId, int targetId, CancellationToken ct)
    {
        var path = new GraphPathDTO
        {
            Path = _nodes.Take(3).ToList(),
            TotalHops = 2,
            TotalWeight = 1.5
        };
        return Task.FromResult(Result<GraphPathDTO>.Ok(path));
    }

    public Task<Result<GraphRelationDTO>> AddRelationAsync(CreateGraphRelationRequest req, CancellationToken ct)
    {
        var rel = new GraphRelationDTO
        {
            Id = _nextId++,
            SourceNodeId = req.SourceNodeId,
            TargetNodeId = req.TargetNodeId,
            RelationType = req.RelationType,
            Metadata = req.Metadata,
            CreatedAt = DateTime.UtcNow
        };
        _relations.Add(rel);
        return Task.FromResult(Result<GraphRelationDTO>.Ok(rel));
    }

    public Task<Result<GraphRelationDTO>> UpdateRelationAsync(int id, EditGraphRelationRequest req, CancellationToken ct)
    {
        var rel = _relations.FirstOrDefault(r => r.Id == id);
        if (rel is null)
            return Task.FromResult(Result<GraphRelationDTO>.Fail("Relation not found"));

        rel.RelationType = req.RelationType;
        rel.Metadata = req.Metadata;
        rel.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(Result<GraphRelationDTO>.Ok(rel));
    }

    public Task<Result<bool>> RemoveRelationAsync(int id, CancellationToken ct)
    {
        _relations.RemoveAll(r => r.Id == id);
        return Task.FromResult(Result<bool>.Ok(true));
    }

    public Task<Result<bool>> RestoreRelationAsync(int id, CancellationToken ct)
        => Task.FromResult(Result<bool>.Ok(true));

    public Task<Result<List<GraphRelationDTO>>> GetNodeRelationsAsync(int nodeId, CancellationToken ct)
    {
        var rels = _relations.Where(r => r.SourceNodeId == nodeId || r.TargetNodeId == nodeId).ToList();
        return Task.FromResult(Result<List<GraphRelationDTO>>.Ok(rels));
    }

    public GraphCorrelation CorrelateAsync(string assetId, string vulnId, CancellationToken ct = default)
    {
        return new GraphCorrelation(assetId, vulnId, 1);
    }
}