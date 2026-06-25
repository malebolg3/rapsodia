// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.EntityFrameworkCore;

namespace Rapsodia.Blue.Infrastructure.Data.Providers;

public class SqliteProvider : IDatabaseProvider
{
    public void Configure(DbContextOptionsBuilder options, string connectionString)
    {
        var conn = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = OFF;";
        cmd.ExecuteNonQuery();
        conn.Close();
        
        options.UseSqlite(connectionString + ";Foreign Keys=False", sqliteOptions =>
        {
            sqliteOptions.CommandTimeout(30);
        });
    }
}