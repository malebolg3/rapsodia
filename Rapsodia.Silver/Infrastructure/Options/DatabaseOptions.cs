namespace Rapsodia.Silver.Infrastructure.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";
    public ConnectionConfig Runtime { get; set; } = new();
    public ConnectionConfig Migrations { get; set; } = new();

    public class ConnectionConfig
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 5432;
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SslMode { get; set; } = "Require";

        public string BuildConnectionString() =>
            $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};SslMode={SslMode};";

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Host))
                throw new InvalidOperationException("Database Host ausente");
            if (string.IsNullOrWhiteSpace(Database))
                throw new InvalidOperationException("Database Name ausente");
            if (string.IsNullOrWhiteSpace(Username))
                throw new InvalidOperationException("Database Username ausente");
            if (string.IsNullOrWhiteSpace(Password))
                throw new InvalidOperationException("Database Password ausente");
        }
    }
}