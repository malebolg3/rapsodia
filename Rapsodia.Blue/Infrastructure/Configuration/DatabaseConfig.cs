// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.Extensions.Configuration;

namespace Rapsodia.Blue.Infrastructure.Configuration;

public class DatabaseConfigService
{
    public string PrincipalHost { get; }
    public string PrincipalPort { get; }
    public string PrincipalDatabase { get; }
    public string PrincipalUsername { get; }
    public string PrincipalConnectionString { get; }
    public string LabsHost { get; }
    public string LabsPort { get; }
    public string LabsDatabase { get; }
    public string LabsUsername { get; }
    public string LabsConnectionString { get; }
    public string BlueConnectionString { get; }
    public string RedisConnectionString { get; }
    public bool SilverEnabled { get; }
    public bool RedisEnabled { get; }
    public bool OrleansEnabled { get; }
    public string Environment { get; }

    public DatabaseConfigService(IConfiguration configuration)
    {
        Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        PrincipalHost = System.Environment.GetEnvironmentVariable("OCI_PRINCIPAL_HOST") ?? "localhost";
        PrincipalPort = System.Environment.GetEnvironmentVariable("OCI_PRINCIPAL_PORT") ?? "1521";
        PrincipalDatabase = System.Environment.GetEnvironmentVariable("OCI_PRINCIPAL_DATABASE") ?? "rapsodia_db";
        PrincipalUsername = System.Environment.GetEnvironmentVariable("OCI_PRINCIPAL_USERNAME") ?? "admin";

        LabsHost = System.Environment.GetEnvironmentVariable("OCI_LABS_HOST") ?? "localhost";
        LabsPort = System.Environment.GetEnvironmentVariable("OCI_LABS_PORT") ?? "1521";
        LabsDatabase = System.Environment.GetEnvironmentVariable("OCI_LABS_DATABASE") ?? "rapsodia_labs";
        LabsUsername = System.Environment.GetEnvironmentVariable("OCI_LABS_USERNAME") ?? "admin";

        var principalPassword = System.Environment.GetEnvironmentVariable("OCI_PRINCIPAL_PASSWORD") ?? "";
        PrincipalConnectionString = $"Host={PrincipalHost};Port={PrincipalPort};Database={PrincipalDatabase};Username={PrincipalUsername};Password=***";

        var labsPassword = System.Environment.GetEnvironmentVariable("OCI_LABS_PASSWORD") ?? "";
        LabsConnectionString = $"Host={LabsHost};Port={LabsPort};Database={LabsDatabase};Username={LabsUsername};Password=***";

        BlueConnectionString = System.Environment.GetEnvironmentVariable("BLUE_DB_CONNECTION") ?? "InMemory";
        if (BlueConnectionString.Contains("Password="))
        {
            var parts = BlueConnectionString.Split(';');
            BlueConnectionString = string.Join(";", parts.Select(p =>
                p.StartsWith("Password=") ? "Password=***" : p));
        }

        RedisConnectionString = System.Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "localhost:6379";
        SilverEnabled = System.Environment.GetEnvironmentVariable("SILVER_ENABLED") == "true";
        RedisEnabled = System.Environment.GetEnvironmentVariable("REDIS_ENABLED") == "true";
        OrleansEnabled = System.Environment.GetEnvironmentVariable("ORLEANS_ENABLED") == "true";
    }
}