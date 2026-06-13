using Rapsodia.Red.Domain.Entities;
using Rapsodia.Red.Domain.Interfaces;

namespace Rapsodia.Red.Infrastructure.Adapters;

public class KaliToolsAdapter : IScanPort
{
    private readonly KaliToolRunner _runner;
    private readonly IAiAnalystPort _ai;
    private readonly ILogger<KaliToolsAdapter> _logger;
    private readonly bool _mock;

    public KaliToolsAdapter(KaliToolRunner runner, IAiAnalystPort ai, ILogger<KaliToolsAdapter> logger, IConfiguration cfg)
    {
        _runner = runner;
        _ai = ai;
        _logger = logger;
        _mock = cfg["KALI_MOCK"] == "true" || cfg["RED_MOCK"] == "true";
    }

    public async Task<bool> RunScanAsync(ScanTarget target)
    {
        if (_mock)
        {
            _logger.LogInformation("Mock: Scan simulado em {Target}", target.Host);
            target.UpdateStatus("Completed");
            return true;
        }

        try
        {
            _logger.LogInformation("Iniciando scan completo em {Target}", target.Host);

            var nmapResult = await ScanNmapAsync(target.Host);
            var gobusterResult = await ScanGobusterAsync(target.Host);
            var niktoResult = await ScanNiktoAsync(target.Host);

            var combinedResults = $"NMAP:\n{nmapResult}\n\nGOBUSTER:\n{gobusterResult}\n\nNIKTO:\n{niktoResult}";
            await _ai.AnalyzeScanResultsAsync("Multi-Tool Scan", combinedResults);

            target.UpdateStatus("Completed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha no scan de {Target}", target.Host);
            target.UpdateStatus("Failed");
            return false;
        }
    }

    public async Task<string> ScanNmapAsync(string target) =>
        (await _runner.RunAsync("nmap", $"-sV -sC -O -T4 --script vuln -oX - {target}")).Stdout;

    public async Task<string> ScanNiktoAsync(string target) =>
        (await _runner.RunAsync("nikto", $"-h {target} -Format json -output -")).Stdout;

    public async Task<string> ScanSqlmapAsync(string url) =>
        (await _runner.RunAsync("sqlmap", $"-u \"{url}\" --batch --level=3 --risk=2")).Stdout;

    public async Task<string> ScanGobusterAsync(string target, string wordlist = "/usr/share/wordlists/dirbuster/directory-list-2.3-medium.txt") =>
        (await _runner.RunAsync("gobuster", $"dir -u {target} -w {wordlist} -q")).Stdout;

    public async Task<string> ScanHydraAsync(string target, string service, string userList, string passList) =>
        (await _runner.RunAsync("hydra", $"-L {userList} -P {passList} {target} {service}")).Stdout;

    public async Task<string> ScanWfuzzAsync(string url, string wordlist) =>
        (await _runner.RunAsync("wfuzz", $"-w {wordlist} --hc 404 json {url}/FUZZ")).Stdout;
}