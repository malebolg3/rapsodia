// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;

namespace Rapsodia.Silver.Domain.Interfaces;

public interface IDatabaseProvider
{
    void Configure(DbContextOptionsBuilder options, string connectionString);
}