// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Orleans;
using Rapsodia.Silver.Application.DTOs;
using Rapsodia.Silver.Application.Interfaces;
using Rapsodia.Silver.Domain.Interfaces;

namespace Rapsodia.Silver.Spart.Grains;

public abstract class HybridAgent : Grain
{
    protected IChatService _chat = null!;
    protected IMemoryService _memory = null!;
    protected IObsidianService _obsidian = null!;
    protected ILogger _logger = null!;
    protected string _context = string.Empty;
    protected string _agentName = string.Empty;
    protected string _agentColor = string.Empty;
    protected bool _isAutonomous;
    protected CancellationTokenSource? _loopCts;

    protected void Initialize(
        IChatService chat,
        IMemoryService memory,
        IObsidianService obsidian,
        ILogger logger,
        string agentName,
        string agentColor,
        string context,
        bool startAutonomous = false)
    {
        _chat = chat;
        _memory = memory;
        _obsidian = obsidian;
        _logger = logger;
        _agentName = agentName;
        _agentColor = agentColor;
        _context = context;
        
        if (startAutonomous)
            StartAutonomousLoop();
    }

    public async Task<string> ExecuteCommand(string input)
    {
        _logger.LogInformation("{Agent}: Comando recebido: {Input}", _agentName, input);

        var result = await _chat.SendMessageAsync(new ChatRequest(
            ConversationId: Guid.NewGuid(),
            Message: input
        ));

        var response = result.Data?.Content ?? "Sem resposta.";
        await _memory.SaveMemory(_agentName, input, response);
        
        return response;
    }

    public void StartAutonomousLoop(TimeSpan? interval = null)
    {
        if (_loopCts != null) return;
        
        _loopCts = new CancellationTokenSource();
        _isAutonomous = true;
        var tick = interval ?? TimeSpan.FromMinutes(5);

        _ = Task.Run(async () =>
        {
            _logger.LogInformation("{Agent}: Loop autônomo iniciado a cada {Interval}min", _agentName, tick.TotalMinutes);
            
            while (!_loopCts.Token.IsCancellationRequested)
            {
                try
                {
                    await AutonomousTick();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Agent}: Erro no loop autônomo", _agentName);
                }
                await Task.Delay(tick, _loopCts.Token);
            }
        }, _loopCts.Token);
    }

    public void StopAutonomousLoop()
    {
        _loopCts?.Cancel();
        _loopCts = null;
        _isAutonomous = false;
        _logger.LogInformation("{Agent}: Loop autônomo parado", _agentName);
    }

    protected abstract Task AutonomousTick();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        StopAutonomousLoop();
        await base.OnDeactivateAsync(reason, cancellationToken);
    }
}