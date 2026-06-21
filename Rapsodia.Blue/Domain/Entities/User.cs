// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Blue.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; protected set; } = string.Empty;
    public string PasswordHash { get; protected set; } = string.Empty;
    public string Role { get; protected set; } = "Analyst";
    public string Email { get; protected set; } = string.Empty;
    public string FullName { get; protected set; } = string.Empty;
    public string AllowedModules { get; protected set; } = "blue,red,violet,silver";

    public User() { }

    public User(string username, string passwordHash, string role)
    {
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        AllowedModules = "blue,red,violet,silver";
    }

    public User(string username, string passwordHash, string role, string allowedModules)
    {
        Username = username;
        PasswordHash = passwordHash;
        Role = role;
        AllowedModules = allowedModules;
    }

    public void SetPasswordHash(string hash) => PasswordHash = hash;
    public void SetRole(string role) => Role = role;
    public void SetEmail(string email) => Email = email;
    public void SetFullName(string fullName) => FullName = fullName;
    public void SetAllowedModules(string modules) => AllowedModules = modules;
    public void Activate() => MarkAsRestored();
    public void Deactivate() => MarkAsDeleted();
}