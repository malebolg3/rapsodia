namespace rapsodia.Services.Telemetry;

public class ObsidianService
{
    private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), "Knowledge", "Attractor");

    public async Task SaveNote(string content, string topic)
    {
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);

        var fileName = $"{DateTime.Now:yyyyMMdd_HHmm}_{topic}.md";
        await File.WriteAllTextAsync(Path.Combine(_path, fileName), content);
    }
}