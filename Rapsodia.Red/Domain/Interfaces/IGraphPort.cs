namespace Rapsodia.Red.Domain.Interfaces;

public interface IGraphPort
{
    Task ConnectAsync(Guid sourceId, string originType, Guid targetId, string targetType, string relationType);
}
