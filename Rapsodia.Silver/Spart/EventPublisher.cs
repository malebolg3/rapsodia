// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Orleans;
using Orleans.Streams;

namespace Rapsodia.Silver.Spart;

public class EventPublisher
{
    private readonly IClusterClient _client;

    public EventPublisher(IClusterClient client)
    {
        _client = client;
    }

    public async Task PublishAsync<T>(string streamProvider, string streamNamespace, Guid streamId, T message)
    {
        var stream = _client.GetStreamProvider(streamProvider)
            .GetStream<T>(StreamId.Create(streamNamespace, streamId));
        await stream.OnNextAsync(message);
    }
}