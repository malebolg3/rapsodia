using Rapsodia.Red.Application.DTOs;
using Rapsodia.Red.Application.Interfaces;
using Rapsodia.Red.Domain.Entities;
using Rapsodia.Red.Domain.Common;
using Rapsodia.Red.Domain.Interfaces;

namespace Rapsodia.Red.Application.Services;

public class ScanService : IScanService
{
    private readonly IScanPort _scanPort;
    private readonly IGraphPort _graphPort;
    private readonly ILogger<ScanService> _logger;
    private readonly Dictionary<Guid, ScanResultDTO> _scans = new();

    public ScanService(IScanPort scanPort, IGraphPort graphPort, ILogger<ScanService> logger)
    {
        _scanPort = scanPort;
        _graphPort = graphPort;
        _logger = logger;
    }

    public async Task<Result<ScanResultDTO>> StartScanAsync(ScanRequest request, CancellationToken ct)
    {
        var scanId = Guid.NewGuid();
        var target = ScanTarget.Create(request.Target);
        
        var scan = new ScanResultDTO
        {
            ScanId = scanId,
            Target = request.Target,
            ScanType = request.ScanType,
            Status = "Running",
            StartedAt = DateTime.UtcNow,
            Findings = new List<ScanFindingDTO>()
        };
        _scans[scanId] = scan;

        try
        {
            _logger.LogInformation("Iniciando scan {ScanId} em {Target}", scanId, request.Target);
            var success = await _scanPort.RunScanAsync(target);
            
            if (success)
            {
                scan.Status = "Completed";
                scan.CompletedAt = DateTime.UtcNow;
                scan.Findings = await GenerateMockFindings(request);
                
                await _graphPort.ConnectAsync(
                    Guid.NewGuid(), "Scan",
                    scanId, "Target",
                    "SCANNED"
                );
                
                _logger.LogInformation("Scan {ScanId} concluído com {Count} findings", scanId, scan.Findings.Count);
            }
            else
            {
                scan.Status = "Failed";
                scan.CompletedAt = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scan {ScanId} falhou", scanId);
            scan.Status = "Failed";
            scan.CompletedAt = DateTime.UtcNow;
        }

        return Result<ScanResultDTO>.Ok(scan);
    }

    public async Task<Result<ScanResultDTO>> GetScanStatusAsync(Guid scanId, CancellationToken ct)
    {
        await Task.CompletedTask;
        return _scans.TryGetValue(scanId, out var scan)
            ? Result<ScanResultDTO>.Ok(scan)
            : Result<ScanResultDTO>.Fail($"Scan {scanId} não encontrado");
    }

    public async Task<Result<PagedResult<ScanResultDTO>>> ListScansAsync(ScanFilterDTO filter, CancellationToken ct)
    {
        await Task.CompletedTask;
        var items = _scans.Values
            .Where(s => string.IsNullOrEmpty(filter.Status) || s.Status == filter.Status)
            .Where(s => string.IsNullOrEmpty(filter.ScanType) || s.ScanType == filter.ScanType)
            .ToList();

        return Result<PagedResult<ScanResultDTO>>.Ok(new PagedResult<ScanResultDTO>
        {
            Items = items.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToList(),
            TotalCount = items.Count,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    public async Task<Result<ScanResultDTO>> StopScanAsync(Guid scanId, CancellationToken ct)
    {
        await Task.CompletedTask;
        if (!_scans.TryGetValue(scanId, out var scan))
            return Result<ScanResultDTO>.Fail($"Scan {scanId} não encontrado");

        scan.Status = "Stopped";
        scan.CompletedAt = DateTime.UtcNow;
        return Result<ScanResultDTO>.Ok(scan);
    }

    public async Task<Result<List<ScanFindingDTO>>> GetFindingsAsync(Guid scanId, CancellationToken ct)
    {
        await Task.CompletedTask;
        return _scans.TryGetValue(scanId, out var scan)
            ? Result<List<ScanFindingDTO>>.Ok(scan.Findings)
            : Result<List<ScanFindingDTO>>.Fail($"Scan {scanId} não encontrado");
    }

    private async Task<List<ScanFindingDTO>> GenerateMockFindings(ScanRequest request)
    {
        await Task.Delay(100);
        return new List<ScanFindingDTO>
        {
            new() { Port = 22, Service = "ssh", Version = "OpenSSH 7.4", State = "open", Vulnerabilities = new List<string> { "CVE-2018-15473" } },
            new() { Port = 80, Service = "http", Version = "Apache 2.4.6", State = "open", Vulnerabilities = new List<string> { "CVE-2021-41773" } },
            new() { Port = 443, Service = "https", Version = "nginx 1.14", State = "open" },
            new() { Port = 3306, Service = "mysql", Version = "MySQL 5.7", State = "open", Vulnerabilities = new List<string> { "CVE-2020-14750" } },
            new() { Port = 8080, Service = "http-proxy", Version = "Tomcat 9.0", State = "open", Vulnerabilities = new List<string> { "CVE-2020-1938" } }
        };
    }
}