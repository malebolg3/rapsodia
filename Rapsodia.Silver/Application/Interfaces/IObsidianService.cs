namespace Rapsodia.Silver.Application.Interfaces;

public interface IObsidianService
{
    Task<string> AppendNoteAsync(string vault, string content);
    Task<string> AppendNoteWithLinksAsync(string vault, string content, IEnumerable<string>? relatedNoteIds = null, IEnumerable<string>? tags = null);
    Task<string> ReadNoteAsync(string vault, string noteId);
    Task<List<string>> SearchNotesAsync(string vault, string query);
    Task<bool> HealthCheckAsync();
}