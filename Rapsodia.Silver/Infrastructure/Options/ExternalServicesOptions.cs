namespace Rapsodia.Silver.Infrastructure.Options;

public class ExternalServicesOptions
{
    public const string SectionName = "ExternalServices";
    
    public GrafanaConfig Grafana { get; set; } = new();
    public OpenTelemetryConfig OpenTelemetry { get; set; } = new();
    public ObsidianConfig Obsidian { get; set; } = new();
    public MetasploitConfig Metasploit { get; set; } = new();
    public AiConfig Ai { get; set; } = new(); 

    public void ValidateAll()
    {
        Grafana.Validate();
        Obsidian.Validate();
        Metasploit.Validate();
        Ai.Validate();
    }

    public class GrafanaConfig
    {
        public string Password { get; set; } = string.Empty;
        public void Validate() { if (string.IsNullOrWhiteSpace(Password)) throw new InvalidOperationException("Grafana Password ausente."); }
    }

    public class OpenTelemetryConfig
    {
        public string ServiceName { get; set; } = "rapsodia-api";
        public string OtlpEndpoint { get; set; } = "http://localhost:4317";
        public string LokiEndpoint { get; set; } = "http://localhost:3100";
        public string PrometheusEndpoint { get; set; } = "http://localhost:9090";
    }

    public class ObsidianConfig
    {
        public string ApiUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public void Validate() { if (string.IsNullOrWhiteSpace(ApiKey)) throw new InvalidOperationException("Obsidian ApiKey ausente."); }
    }

    public class MetasploitConfig
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string User { get; set; } = string.Empty;
        public string Pass { get; set; } = string.Empty;
        public void Validate() { if (string.IsNullOrWhiteSpace(Pass)) throw new InvalidOperationException("Metasploit Pass ausente."); }
    }

    public class AiConfig
    {
        public string ApiUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public void Validate() { if (string.IsNullOrWhiteSpace(ApiKey)) throw new InvalidOperationException("AI ApiKey ausente."); }
    }
}