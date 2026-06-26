// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Orleans;
using Rapsodia.Silver.Spart.Interfaces;

namespace Rapsodia.Silver.Spart;

public sealed class GrainRouter
{
    private readonly IGrainFactory _grains;

    public GrainRouter(IGrainFactory grains)
    {
        _grains = grains ?? throw new ArgumentNullException(nameof(grains));
    }

    public ISilverAgent GetSilverAgent() => _grains.GetGrain<ISilverAgent>(Guid.Empty);
    public IBlueAgent GetBlueAgent(Guid id) => _grains.GetGrain<IBlueAgent>(id);
    public IRedAgent GetRedAgent(Guid id) => _grains.GetGrain<IRedAgent>(id);
    public IVioletAgent GetVioletAgent(Guid id) => _grains.GetGrain<IVioletAgent>(id);
}