using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Rapsodia.Red.Infrastructure;

public class KaliToolRunner
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<KaliToolRunner> _logger;
    private readonly bool _mock;

    public KaliToolRunner(IConfiguration cfg, ILogger<KaliToolRunner> logger)
    {
        _cfg = cfg;
        _logger = logger;
        _mock = cfg["KALI_MOCK"] == "true" || string.IsNullOrEmpty(cfg["KALI_HOST"]);
    }

    public record ToolResult(string Stdout, string Stderr, int ExitCode);

    public async Task<ToolResult> RunAsync(string tool, string arguments)
    {
        if (_mock)
        {
            await Task.Delay(200);
            return new ToolResult(MockOutput(tool), "", 0);
        }

        var host = _cfg["KALI_HOST"] ?? "localhost";
        var user = _cfg["KALI_USER"] ?? "root";
        
        var psi = new ProcessStartInfo
        {
            FileName = "ssh",
            Arguments = $"{user}@{host} '{tool} {arguments}'",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        var stdout = await process.StandardOutput.ReadToEndAsync();
        var stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ToolResult(stdout, stderr, process.ExitCode);
    }

    public async Task<string> ExecuteAsync(string tool, string arguments, CancellationToken ct)
    {
        var result = await RunAsync(tool, arguments);
        return string.IsNullOrEmpty(result.Stdout) ? result.Stderr : result.Stdout;
    }

    public async Task<bool> IsToolAvailableAsync(string tool)
    {
        if (_mock) return true;
        var result = await RunAsync("which", tool);
        return !string.IsNullOrEmpty(result.Stdout) && !result.Stdout.Contains("not found");
    }

    private static string MockOutput(string tool) => tool switch
    {
        "nmap" => "PORT     STATE    SERVICE      VERSION\n22/tcp   open     ssh          OpenSSH 7.4\n80/tcp   open     http         Apache 2.4.6\n443/tcp  open     ssl/http     nginx 1.14\n3306/tcp open     mysql        MySQL 5.7\n\nVULNERABILITIES:\nCVE-2018-15473\nCVE-2021-41773\nCVE-2020-14750",
        "gobuster" => "/admin (301)\n/login (200)\n/api (403)\n/backup (200)\n/.env (200)",
        "nikto" => "+ OSVDB-112004: /phpmyadmin/\n+ OSVDB-3092: /test/\n+ OSVDB-3233: /cgi-bin/",
        "hydra" => "[22][ssh] host: 127.0.0.1   login: admin   password: P@ssw0rd",
        "sqlmap" => "[INFO] GET parameter 'id' is vulnerable\n[INFO] back-end DBMS: MySQL 5.7",
        "wfuzz" => "{\"total_requests\": 150, \"found\": [\"/admin\", \"/api\", \"/config\"]}",
        _ => $"[{tool}] Execution completed successfully."
    };
}