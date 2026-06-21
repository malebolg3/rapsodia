// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Blue.Domain.Entities.Olimpo;

public sealed class OlimpoDocument
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long Size { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Tags { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private OlimpoDocument() { }

    public static OlimpoDocument Create(Guid userId, string name, string fileName, string contentType, long size, string storagePath, string category, string tags)
    {
        return new OlimpoDocument
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            FileName = fileName,
            ContentType = contentType,
            Size = size,
            StoragePath = storagePath,
            Category = category,
            Tags = tags,
            CreatedAt = DateTime.UtcNow
        };
    }
}