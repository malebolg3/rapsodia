using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Services;

public class UserService : IUserService
{
    private readonly List<UserResultDTO> _users = new();
    private readonly IConfiguration _cfg;
    private readonly bool _mock;
    private int _nextId = 1;

    public UserService(IConfiguration cfg)
    {
        _cfg = cfg;
        _mock = cfg["AUTH_MOCK"] == "true" || string.IsNullOrEmpty(cfg["DB_HOST"]);
        
        if (_mock)
        {
            _users.Add(new UserResultDTO
            {
                Id = _nextId++,
                Username = "admin",
                Email = "admin@rapsodia.local",
                FullName = "Admin User",
                IsActive = true,
                Roles = new List<string> { "Admin", "Analyst" },
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            });
        }
    }

    public async Task<Result<UserResultDTO>> CreateAsync(CreateUserRequest req, CancellationToken ct)
    {
        if (_mock)
        {
            var user = new UserResultDTO
            {
                Id = _nextId++,
                Username = req.Username,
                Email = req.Email,
                FullName = req.FullName,
                IsActive = true,
                Roles = req.RoleIds?.Select(id => $"Role_{id}").ToList() ?? new List<string> { "User" },
                CreatedAt = DateTime.UtcNow
            };
            _users.Add(user);
            return Result<UserResultDTO>.Ok(user);
        }

        await Task.Delay(10, ct);
        return Result<UserResultDTO>.Fail("Database not configured");
    }

    public async Task<Result<UserResultDTO>> GetByIdAsync(int id, CancellationToken ct)
    {
        if (_mock)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return user is null ? Result<UserResultDTO>.Fail("User not found") : Result<UserResultDTO>.Ok(user);
        }

        await Task.Delay(10, ct);
        return Result<UserResultDTO>.Fail("Database not configured");
    }

    public async Task<Result<PagedResult<UserResultDTO>>> ListAsync(UserFilterDTO filter, CancellationToken ct)
    {
        if (_mock)
        {
            var items = _users
                .Where(u => string.IsNullOrEmpty(filter.Search) || u.Username.Contains(filter.Search) || u.Email.Contains(filter.Search))
                .Where(u => !filter.IsActive.HasValue || u.IsActive == filter.IsActive.Value)
                .ToList();

            return Result<PagedResult<UserResultDTO>>.Ok(new PagedResult<UserResultDTO>
            {
                Items = items.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList(),
                TotalCount = items.Count,
                Page = filter.Page,
                PageSize = filter.PageSize
            });
        }

        await Task.Delay(10, ct);
        return Result<PagedResult<UserResultDTO>>.Fail("Database not configured");
    }

    public async Task<Result<UserResultDTO>> UpdateAsync(int id, EditUserRequest req, CancellationToken ct)
    {
        if (_mock)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user is null) return Result<UserResultDTO>.Fail("User not found");

            if (req.Email != null) user.Email = req.Email;
            if (req.FullName != null) user.FullName = req.FullName;
            if (req.IsActive.HasValue) user.IsActive = req.IsActive.Value;

            return Result<UserResultDTO>.Ok(user);
        }

        await Task.Delay(10, ct);
        return Result<UserResultDTO>.Fail("Database not configured");
    }

    public Task<Result<bool>> DisableAsync(int id, CancellationToken ct)
    {
        if (_mock)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user is null) return Task.FromResult(Result<bool>.Fail("User not found"));
            user.IsActive = false;
            return Task.FromResult(Result<bool>.Ok(true));
        }

        return Task.FromResult(Result<bool>.Ok(true));
    }

    public Task<Result<bool>> EnableAsync(int id, CancellationToken ct)
    {
        if (_mock)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user is null) return Task.FromResult(Result<bool>.Fail("User not found"));
            user.IsActive = true;
            return Task.FromResult(Result<bool>.Ok(true));
        }

        return Task.FromResult(Result<bool>.Ok(true));
    }

    public Task<Result<UserResultDTO>> AddRoleAsync(int id, AddRoleRequest req, CancellationToken ct)
    {
        if (_mock)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user is null) return Task.FromResult(Result<UserResultDTO>.Fail("User not found"));

            var roleName = $"Role_{req.RoleId}";
            if (!user.Roles.Contains(roleName))
                user.Roles.Add(roleName);

            return Task.FromResult(Result<UserResultDTO>.Ok(user));
        }

        return Task.FromResult(Result<UserResultDTO>.Fail("Database not configured"));
    }

    public Task<Result<bool>> RemoveRoleAsync(int id, int roleId, CancellationToken ct)
    {
        if (_mock)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user is null) return Task.FromResult(Result<bool>.Fail("User not found"));
            user.Roles.RemoveAll(r => r == $"Role_{roleId}");
            return Task.FromResult(Result<bool>.Ok(true));
        }

        return Task.FromResult(Result<bool>.Ok(true));
    }
}