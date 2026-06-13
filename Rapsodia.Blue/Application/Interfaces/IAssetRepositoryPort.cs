using System.Collections.Generic;
using System.Threading.Tasks;
using Rapsodia.Blue.Domain.Entities;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IAssetRepositoryPort
{
    Task<IEnumerable<Asset>> ListAllActiveAsync();
    Task<IEnumerable<Asset>> ListAllAsync();
    Task<Asset?> GetByIdAsync(int id);
    Task<Asset?> GetByIdWithVulnsAsync(int id);
    Task<bool> ExistsByIdAsync(int id);
    Task SaveAsync(Asset asset);
    Task UpdateAsync(Asset asset);
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> ExistsByNameExceptIdAsync(string name, int excludeId);
}