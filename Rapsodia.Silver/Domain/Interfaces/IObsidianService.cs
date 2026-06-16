namespace Rapsodia.Silver.Domain.Interfaces;

public interface IObsidianService
{
    Task<string> QueryAsync(string query);
    Task<bool> SyncDocumentAsync(string path, string content);
    Task<string> GetDocumentAsync(string path);
}