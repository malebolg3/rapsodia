// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Rapsodia.Blue.Application.DTOs.Olimpo;

public class OlimpoLoginRequest
{
    public string MasterPassword { get; set; } = string.Empty;
}

public class OlimpoLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class OlimpoSecretResponse
{
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OlimpoCreateSecretRequest
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Environment { get; set; } = "production";
    public string Category { get; set; } = "general";
}

public class OlimpoUpdateSecretRequest
{
    public string Value { get; set; } = string.Empty;
    public string Environment { get; set; } = "production";
}

public class OlimpoAddTotpRequest
{
    public string Uri { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
}

public class OlimpoTotpResponse
{
    public string Id { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int RemainingSeconds { get; set; }
}

public class OlimpoDocumentResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class OlimpoCiEnvResponse
{
    public string Environment { get; set; } = string.Empty;
    public Dictionary<string, string> Variables { get; set; } = new();
}

public class OlimpoUploadDocumentRequest
{
    public IFormFile File { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "general";
    public string Tags { get; set; } = string.Empty;
}