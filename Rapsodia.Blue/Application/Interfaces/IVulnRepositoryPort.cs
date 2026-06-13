using System.Collections.Generic;
using System.Threading.Tasks;
using Rapsodia.Blue.Domain.Entities;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IVulnRepositoryPort
{
    Task<IEnumerable<Vuln>> ListAllAsync();
    Task<Vuln?> GetByIdAsync(int id);
    Task<Vuln?> GetByIdWithRelationsAsync(int id);
    Task<IEnumerable<Vuln>> GetByIdsAsync(List<int> ids);
    Task SaveAsync(Vuln vuln);
    Task UpdateAsync(Vuln vuln);
    Task<bool> ExistsByIdAsync(int id);
}