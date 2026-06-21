// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Silver.Application.DTOs;

public record HealthDto(string Status, DateTime Timestamp, bool IsDatabaseHealthy);