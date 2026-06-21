// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Blue.Application.DTOs;

public class CreateIntegrationRequest
{
    public string Name { get; set; } = string.Empty;
    public string IntegrationType { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public Dictionary<string, string>? Credentials { get; set; }
    public Dictionary<string, string>? Configuration { get; set; }
}

public class EditIntegrationRequest
{
    public string? Name { get; set; }
    public string? Endpoint { get; set; }
    public Dictionary<string, string>? Credentials { get; set; }
    public Dictionary<string, string>? Configuration { get; set; }
}

public class IntegrationResultDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IntegrationType { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTestedAt { get; set; }
}

public class IntegrationLogDTO
{
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class LogFilterDTO
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Level { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}