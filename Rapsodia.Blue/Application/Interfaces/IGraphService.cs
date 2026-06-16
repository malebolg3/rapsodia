using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IGraphService
{
    Task<Result<GraphTopologyDTO>> GetTopologyAsync(CancellationToken ct);
    Task<Result<GraphPathDTO>> FindPathAsync(int sourceId, int targetId, CancellationToken ct);
    Task<Result<GraphRelationDTO>> AddRelationAsync(CreateGraphRelationRequest request, CancellationToken ct);
    Task<Result<GraphRelationDTO>> UpdateRelationAsync(int id, EditGraphRelationRequest request, CancellationToken ct);
    Task<Result<bool>> RemoveRelationAsync(int id, CancellationToken ct);
    Task<Result<bool>> RestoreRelationAsync(int id, CancellationToken ct);
    Task<Result<List<GraphRelationDTO>>> GetNodeRelationsAsync(int nodeId, CancellationToken ct);
}