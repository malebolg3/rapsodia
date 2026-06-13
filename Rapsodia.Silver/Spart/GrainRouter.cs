using Orleans;
using Rapsodia.Silver.Spart.Interfaces;

namespace Rapsodia.Silver.Spart;

public class GrainRouter
{
    private readonly IGrainFactory _grains;

    public GrainRouter(IGrainFactory grains)
    {
        _grains = grains;
    }

    public ISilverAgent GetSilverAgent() => _grains.GetGrain<ISilverAgent>(0);
    public IBlueAgent GetBlueAgent() => _grains.GetGrain<IBlueAgent>(0);
    public IRedAgent GetRedAgent() => _grains.GetGrain<IRedAgent>(0);
    public IVioletAgent GetVioletAgent() => _grains.GetGrain<IVioletAgent>(0);
}