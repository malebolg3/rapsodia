using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Rapsodia.Blue.Application.Interfaces.Olimpo;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile file, string path, CancellationToken ct);
    Task DeleteAsync(string path, CancellationToken ct);
}