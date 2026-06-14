using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Rapsodia.Blue.Application.Interfaces.Olimpo;

namespace Rapsodia.Blue.Application.Services.Olimpo;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public FileStorageService()
    {
        _basePath = Environment.GetEnvironmentVariable("OLP_PATH") ?? "./storage/olimpo";
    }

    public async Task<string> SaveAsync(IFormFile file, string subPath, CancellationToken ct)
    {
        var path = Path.Combine(_basePath, subPath);
        Directory.CreateDirectory(path);
        var fileName = $"{Path.GetRandomFileName()}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(path, fileName);
        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream, ct);
        return fullPath;
    }

    public Task DeleteAsync(string path, CancellationToken ct)
    {
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }
}