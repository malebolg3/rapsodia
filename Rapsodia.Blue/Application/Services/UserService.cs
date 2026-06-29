// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;
using Rapsodia.Blue.Domain.Entities;
using Rapsodia.Blue.Infrastructure.Data;

namespace Rapsodia.Blue.Application.Services;

public class UserService : IUserService
{
    private readonly BlueDbContext _db;

    public UserService(BlueDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<Result<UserResultDTO>> CreateAsync(CreateUserRequest req, CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(u => u.Username == req.Username, ct))
            return Result<UserResultDTO>.Fail("Username already exists");

        var user = new User(req.Username, BCrypt.Net.BCrypt.HashPassword(req.Password), "Analyst", "blue");
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return Result<UserResultDTO>.Ok(Map(user));
    }

    public async Task<Result<UserResultDTO>> GetByIdAsync(int id, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        return user is null ? Result<UserResultDTO>.Fail("User not found") : Result<UserResultDTO>.Ok(Map(user));
    }

    public async Task<Result<PagedResult<UserResultDTO>>> ListAsync(UserFilterDTO filter, CancellationToken ct)
    {
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(u => u.Username.Contains(filter.Search));

        if (filter.IsActive.HasValue)
            query = query.Where(u => u.DeletedAt == null == filter.IsActive.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(u => Map(u))
            .ToListAsync(ct);

        return Result<PagedResult<UserResultDTO>>.Ok(new PagedResult<UserResultDTO>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    public async Task<Result<UserResultDTO>> UpdateAsync(int id, EditUserRequest req, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user is null) return Result<UserResultDTO>.Fail("User not found");

        if (req.Email != null) user.SetEmail(req.Email);
        if (req.FullName != null) user.SetFullName(req.FullName);
        if (req.Role != null && user.Role != "Admin") user.SetRole(req.Role);
        if (req.AllowedModules != null) user.SetAllowedModules(req.AllowedModules);
        if (req.IsActive.HasValue)
        {
            if (req.IsActive.Value) user.Activate();
            else user.Deactivate();
        }

        await _db.SaveChangesAsync(ct);
        return Result<UserResultDTO>.Ok(Map(user));
    }

    public async Task<Result<bool>> DisableAsync(int id, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user is null) return Result<bool>.Fail("User not found");
        if (user.Role == "Admin") return Result<bool>.Fail("Cannot disable the Admin");
        user.Deactivate();
        await _db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> EnableAsync(int id, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user is null) return Result<bool>.Fail("User not found");
        user.Activate();
        await _db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<UserResultDTO>> SetPermissionsAsync(int id, string role, string allowedModules, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user is null) return Result<UserResultDTO>.Fail("User not found");

        if (user.Role == "Admin" && role != "Admin")
            return Result<UserResultDTO>.Fail("The Admin cannot be demoted.");

        if (role == "Admin")
            return Result<UserResultDTO>.Fail("There can only be one Admin.");

        user.SetRole(role);
        user.SetAllowedModules(allowedModules);
        await _db.SaveChangesAsync(ct);

        return Result<UserResultDTO>.Ok(Map(user));
    }
    public async Task<Result<bool>> AddRoleAsync(int id, AddRoleRequest request, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user is null) return Result<bool>.Fail("User not found");
        if (user.Role == "Admin") return Result<bool>.Fail("Admin already has all roles");
        user.SetRole(request.RoleId.ToString());
        await _db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> RemoveRoleAsync(int id, int roleId, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync(new object[] { id }, ct);
        if (user is null) return Result<bool>.Fail("User not found");
        if (user.Role == "Admin") return Result<bool>.Fail("Cannot remove roles from Admin");
        user.SetRole("Analyst");
        await _db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }

    private static UserResultDTO Map(User user) => new UserResultDTO
{
    Id = user.Id,
    Username = user.Username,
    Email = user.Email ?? string.Empty,
    FullName = user.FullName ?? string.Empty,
    IsActive = user.DeletedAt == null,
    Roles = new List<string> { user.Role },
    AllowedModules = user.AllowedModules ?? string.Empty,
    CreatedAt = user.CreatedAt
};
}