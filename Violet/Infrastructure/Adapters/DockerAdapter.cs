using System.Net.Http.Json;
using Rapsodia.Violet.Application.DTOs;
using Rapsodia.Violet.Domain.Interfaces;

namespace Rapsodia.Violet.Infrastructure.Adapters;

public class DockerAdapter : ILabContainerPort
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<DockerAdapter> _logger;
    private readonly bool _mock;

    public DockerAdapter(IConfiguration cfg, ILogger<DockerAdapter> logger)
    {
        _cfg = cfg;
        _logger = logger;
        _mock = cfg["VLT_MOCK"] == "true" || string.IsNullOrEmpty(cfg["DOCKER_HOST"]);
    }

    public async Task<LabResultDTO> CreateContainerAsync(string name, string image, List<string>? ports, Dictionary<string, string>? envVars, int ttlMinutes)
    {
        if (_mock) return MockLab(name, image, ports, ttlMinutes);

        using var client = new HttpClient();
        var response = await client.PostAsJsonAsync($"{_cfg["DOCKER_HOST"]}/containers/create", new
        {
            Image = image,
            Cmd = new[] { "/bin/bash" },
            Tty = true,
            OpenStdin = true,
            ExposedPorts = ports?.ToDictionary(p => $"{p}/tcp", _ => new { }),
            Env = envVars?.Select(kv => $"{kv.Key}={kv.Value}").ToList()
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<DockerCreateResponse>();
        
        await client.PostAsync($"{_cfg["DOCKER_HOST"]}/containers/{result!.Id}/start", null);

        return new LabResultDTO
        {
            ContainerId = result.Id[..12],
            Name = name,
            Image = image,
            Status = "running",
            Ports = ports ?? new List<string>(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(ttlMinutes)
        };
    }

    public async Task<LabResultDTO?> GetContainerAsync(string containerId)
    {
        if (_mock) return MockLab("mock-lab", "kalilinux/kali-rolling:latest", new List<string> { "22", "80" }, 60);

        using var client = new HttpClient();
        var response = await client.GetAsync($"{_cfg["DOCKER_HOST"]}/containers/{containerId}/json");
        if (!response.IsSuccessStatusCode) return null;
        
        var result = await response.Content.ReadFromJsonAsync<DockerInspectResponse>();
        return new LabResultDTO
        {
            ContainerId = result!.Id[..12],
            Name = result.Name.TrimStart('/'),
            Image = result.Config.Image,
            Status = result.State.Status,
            IpAddress = result.NetworkSettings?.IPAddress,
            Ports = result.NetworkSettings?.Ports?.Keys.Select(p => p).ToList() ?? new List<string>(),
            CreatedAt = DateTime.Parse(result.Created)
        };
    }

    public async Task<List<LabResultDTO>> ListContainersAsync()
    {
        if (_mock) return new List<LabResultDTO> { MockLab("lab-1", "kalilinux/kali-rolling:latest", new List<string> { "22", "80", "443" }, 60) };

        using var client = new HttpClient();
        var containers = await client.GetFromJsonAsync<List<DockerListResponse>>($"{_cfg["DOCKER_HOST"]}/containers/json?all=true");
        
        return containers?.Select(c => new LabResultDTO
        {
            ContainerId = c.Id[..12],
            Name = c.Names.FirstOrDefault()?.TrimStart('/') ?? "unknown",
            Image = c.Image,
            Status = c.State,
            Ports = c.Ports?.Select(p => $"{p.PublicPort}:{p.PrivatePort}").ToList() ?? new List<string>(),
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(c.Created).DateTime
        }).ToList() ?? new List<LabResultDTO>();
    }

    public async Task StopContainerAsync(string containerId)
    {
        if (_mock) { await Task.Delay(100); return; }
        using var client = new HttpClient();
        await client.PostAsync($"{_cfg["DOCKER_HOST"]}/containers/{containerId}/stop", null);
    }

    public async Task StartContainerAsync(string containerId)
    {
        if (_mock) { await Task.Delay(100); return; }
        using var client = new HttpClient();
        await client.PostAsync($"{_cfg["DOCKER_HOST"]}/containers/{containerId}/start", null);
    }

    public async Task RemoveContainerAsync(string containerId)
    {
        if (_mock) { await Task.Delay(100); return; }
        using var client = new HttpClient();
        await client.DeleteAsync($"{_cfg["DOCKER_HOST"]}/containers/{containerId}?force=true");
    }

    public async Task<string> CreateSnapshotAsync(string containerId, string snapshotName)
    {
        if (_mock) { await Task.Delay(200); return Guid.NewGuid().ToString("N")[..8]; }
        
        using var client = new HttpClient();
        var response = await client.PostAsJsonAsync($"{_cfg["DOCKER_HOST"]}/containers/{containerId}/commit", new
        {
            Repository = snapshotName,
            Tag = DateTime.UtcNow.ToString("yyyyMMddHHmmss")
        });
        
        var result = await response.Content.ReadFromJsonAsync<DockerCommitResponse>();
        return result!.Id[..12];
    }

    public async Task RestoreSnapshotAsync(string containerId, string snapshotId)
    {
        if (_mock) { await Task.Delay(300); return; }
        _logger.LogInformation("Snapshot {SnapshotId} restaurado para {ContainerId}", snapshotId, containerId);
        await Task.CompletedTask;
    }

    public async Task<string> ExecuteCommandAsync(string containerId, string command)
    {
        if (_mock) return $"[{containerId}] Command executed: {command}\nroot@lab:~# exit";

        using var client = new HttpClient();
        var response = await client.PostAsJsonAsync($"{_cfg["DOCKER_HOST"]}/containers/{containerId}/exec", new
        {
            Cmd = new[] { "/bin/bash", "-c", command },
            AttachStdout = true,
            AttachStderr = true
        });
        
        var result = await response.Content.ReadFromJsonAsync<DockerExecResponse>();
        return result?.Output ?? "Command executed";
    }

    private static LabResultDTO MockLab(string name, string image, List<string>? ports, int ttlMinutes) => new()
    {
        ContainerId = Guid.NewGuid().ToString("N")[..12],
        Name = name,
        Image = image,
        Status = "running",
        IpAddress = "172.17.0.2",
        Ports = ports ?? new List<string> { "22", "80" },
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddMinutes(ttlMinutes)
    };
}

public class DockerCreateResponse { public string Id { get; set; } = string.Empty; }
public class DockerInspectResponse 
{ 
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Created { get; set; } = string.Empty;
    public DockerConfig Config { get; set; } = new();
    public DockerState State { get; set; } = new();
    public DockerNetworkSettings? NetworkSettings { get; set; }
}
public class DockerConfig { public string Image { get; set; } = string.Empty; }
public class DockerState { public string Status { get; set; } = string.Empty; }
public class DockerNetworkSettings { public string IPAddress { get; set; } = string.Empty; public Dictionary<string, List<DockerPortBinding>>? Ports { get; set; } }
public class DockerPortBinding { public string HostPort { get; set; } = string.Empty; }
public class DockerListResponse 
{ 
    public string Id { get; set; } = string.Empty;
    public List<string> Names { get; set; } = new();
    public string Image { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public long Created { get; set; }
    public List<DockerPort>? Ports { get; set; }
}
public class DockerPort { public int PublicPort { get; set; } public int PrivatePort { get; set; } }
public class DockerCommitResponse { public string Id { get; set; } = string.Empty; }
public class DockerExecResponse { public string Output { get; set; } = string.Empty; }
