// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.ComponentModel.DataAnnotations;

namespace Rapsodia.Silver.Domain.Models;

public class User : BaseEntity
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Analyst";
}