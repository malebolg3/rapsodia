// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.Extensions.Configuration;
using Rapsodia.Silver.Application.Interfaces;
using StackExchange.Redis;

namespace Rapsodia.Silver.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDatabase? _db;
    private readonly bool _enabled;

    public CacheService(IConfiguration cfg)
    {
        _enabled = cfg["CCH_ENB"] == "true";
        if (_enabled)
        {
            var redis = ConnectionMultiplexer.Connect(cfg["CCH_URL"] ?? "localhost:6379");
            _db = redis.GetDatabase();
        }
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (!_enabled || _db is null) return;
        await _db.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetAsync(string key)
    {
        if (!_enabled || _db is null) return null;
        return await _db.StringGetAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (!_enabled || _db is null) return false;
        return await _db.KeyExistsAsync(key);
    }

    public async Task RemoveAsync(string key)
    {
        if (!_enabled || _db is null) return;
        await _db.KeyDeleteAsync(key);
    }
}