// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Red.Domain.Entities;

public class ScanTarget
{
    public Guid Id { get; private set; }
    public string Host { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Pending";

    public static ScanTarget Create(string host)
    {
        return new ScanTarget
        {
            Id = Guid.NewGuid(),
            Host = host,
            Status = "Queued"
        };
    }

    public void UpdateStatus(string newStatus) => Status = newStatus;
}