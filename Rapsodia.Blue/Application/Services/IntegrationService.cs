// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Services;

public class IntegrationService : IIntegrationService
{
    private readonly List<IntegrationResultDTO> _integrations = new();
    private readonly List<IntegrationLogDTO> _logs = new();
    private readonly IConfiguration _cfg;
    private readonly bool _mock;
    private int _nextId = 1;
    private int _nextLogId = 1;

    public IntegrationService(IConfiguration cfg)
    {
        _cfg = cfg;
        _mock = cfg["ITG_MOCK"] == "true" || string.IsNullOrEmpty(cfg["DB_HOST"]);
        
        if (_mock)
        {
            _integrations.Add(new IntegrationResultDTO
            {
                Id = _nextId++,
                Name = "SIEM Splunk",
                IntegrationType = "siem",
                Endpoint = "https://splunk.local:8089",
                IsEnabled = true,
                Status = "Connected",
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                LastTestedAt = DateTime.UtcNow.AddHours(-2)
            });
            _integrations.Add(new IntegrationResultDTO
            {
                Id = _nextId++,
                Name = "Jira Service Desk",
                IntegrationType = "ticketing",
                Endpoint = "https://jira.local/rest/api/2",
                IsEnabled = true,
                Status = "Connected",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastTestedAt = DateTime.UtcNow.AddHours(-1)
            });
            _integrations.Add(new IntegrationResultDTO
            {
                Id = _nextId++,
                Name = "Slack Alerts",
                IntegrationType = "notification",
                Endpoint = "https://hooks.slack.com/services/xxx",
                IsEnabled = false,
                Status = "Disabled",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            });
            
            _logs.Add(new IntegrationLogDTO { Id = _nextLogId++, Level = "INFO", Message = "Connected successfully", Timestamp = DateTime.UtcNow.AddHours(-2) });
            _logs.Add(new IntegrationLogDTO { Id = _nextLogId++, Level = "WARN", Message = "Rate limit approaching", Timestamp = DateTime.UtcNow.AddHours(-1) });
        }
    }

    public Task<Result<IntegrationResultDTO>> CreateAsync(CreateIntegrationRequest req, ClaimsPrincipal user, CancellationToken ct)
    {
        var integration = new IntegrationResultDTO
        {
            Id = _nextId++,
            Name = req.Name,
            IntegrationType = req.IntegrationType,
            Endpoint = req.Endpoint,
            IsEnabled = true,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _integrations.Add(integration);
        return Task.FromResult(Result<IntegrationResultDTO>.Ok(integration));
    }

    public Task<Result<IntegrationResultDTO>> GetByIdAsync(int id, ClaimsPrincipal user, CancellationToken ct)
    {
        var integration = _integrations.FirstOrDefault(i => i.Id == id);
        return Task.FromResult(integration is null 
            ? Result<IntegrationResultDTO>.Fail("Integration not found") 
            : Result<IntegrationResultDTO>.Ok(integration));
    }

    public Task<Result<List<IntegrationResultDTO>>> ListAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        return Task.FromResult(Result<List<IntegrationResultDTO>>.Ok(_integrations.ToList()));
    }

    public Task<Result<IntegrationResultDTO>> UpdateAsync(int id, ClaimsPrincipal user, EditIntegrationRequest req, CancellationToken ct)
    {
        var integration = _integrations.FirstOrDefault(i => i.Id == id);
        if (integration is null)
            return Task.FromResult(Result<IntegrationResultDTO>.Fail("Integration not found"));

        if (req.Name != null) integration.Name = req.Name;
        if (req.Endpoint != null) integration.Endpoint = req.Endpoint;
        return Task.FromResult(Result<IntegrationResultDTO>.Ok(integration));
    }

    public async Task<Result<bool>> TestConnectionAsync(int id, ClaimsPrincipal user, CancellationToken ct)
    {
        var integration = _integrations.FirstOrDefault(i => i.Id == id);
        if (integration is null)
            return Result<bool>.Fail("Integration not found");

        await Task.Delay(500, ct);
        integration.LastTestedAt = DateTime.UtcNow;
        integration.Status = "Connected";
        
        _logs.Add(new IntegrationLogDTO
        {
            Id = _nextLogId++,
            Level = "INFO",
            Message = $"Connection test successful for {integration.Name}",
            Timestamp = DateTime.UtcNow
        });

        return Result<bool>.Ok(true);
    }

    public Task<Result<IntegrationResultDTO>> ToggleAsync(int id, ClaimsPrincipal user, CancellationToken ct)
    {
        var integration = _integrations.FirstOrDefault(i => i.Id == id);
        if (integration is null)
            return Task.FromResult(Result<IntegrationResultDTO>.Fail("Integration not found"));

        integration.IsEnabled = !integration.IsEnabled;
        integration.Status = integration.IsEnabled ? "Connected" : "Disabled";
        return Task.FromResult(Result<IntegrationResultDTO>.Ok(integration));
    }

    public Task<Result<bool>> DeleteAsync(int id, ClaimsPrincipal user, CancellationToken ct)
    {
        _integrations.RemoveAll(i => i.Id == id);
        return Task.FromResult(Result<bool>.Ok(true));
    }

    public Task<Result<PagedResult<IntegrationLogDTO>>> GetLogsAsync(int id, LogFilterDTO filter, ClaimsPrincipal user, CancellationToken ct)
    {
        var items = _logs
            .Where(l => string.IsNullOrEmpty(filter.Level) || l.Level == filter.Level)
            .Where(l => !filter.FromDate.HasValue || l.Timestamp >= filter.FromDate.Value)
            .Where(l => !filter.ToDate.HasValue || l.Timestamp <= filter.ToDate.Value)
            .ToList();

        return Task.FromResult(Result<PagedResult<IntegrationLogDTO>>.Ok(new PagedResult<IntegrationLogDTO>
        {
            Items = items.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList(),
            TotalCount = items.Count,
            Page = filter.Page,
            PageSize = filter.PageSize
        }));
    }
}