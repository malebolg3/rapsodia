// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

namespace Rapsodia.Red.Domain.Interfaces;

public interface IGraphPort
{
    Task ConnectAsync(Guid sourceId, string originType, Guid targetId, string targetType, string relationType);
}
