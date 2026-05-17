using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Diagnostics;
using Rapsodia.Data;
using Rapsodia.DTO.Response;
using Rapsodia.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using rapsodia.Services.Telemetry;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile)) Env.Load(envFile);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); 

var dbPassword = builder.Configuration["DB_PASSWORD"] ?? throw new InvalidOperationException("❌ DB_PASSWORD não encontrada!");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("❌ ConnectionString não encontrada!");

if (connectionString.Contains("{DB_PASSWORD}")) connectionString = connectionString.Replace("{DB_PASSWORD}", dbPassword);
var dbPort = builder.Configuration["DB_PORT_RUNTIME"] ?? "5432";
if (connectionString.Contains("{DB_PORT_RUNTIME}")) connectionString = connectionString.Replace("{DB_PORT_RUNTIME}", dbPort);

connectionString = PostgresConnectionString.Normalize(connectionString);

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddAppServices(builder.Configuration, builder.Environment);

builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddScoped<ObsidianService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger/index.html");
    return Task.CompletedTask;
});

app.MapGet("/test-telemetry", async (GeminiService gemini, ObsidianService obsidian) =>
{
    try
    {
        var logs = new[] {
            "User 'employee' accessed /admin/dashboard without authorization. Role: user, accessed: admin_panel. ACL bypass detected.",
            "Database backup found in /var/tmp/backup_2024.sql (passwords stored in MD5 hash). TLS downgrade attempt on port 443.",
            "SQL Injection: /api/users?id=1' OR '1'='1' executed. Input: user_id not sanitized. Query executed on prod database.",
            "Password reset token valid for 24 hours without rate limiting. User can enumerate tokens via brute-force. No CSRF protection.",
            "S3 bucket 'company-backups' configured with public-read ACL. 2.3TB of sensitive data exposed. AWS credentials in git repository root.",
            "Apache Struts 2.3.15 detected (CVE-2017-5638 RCE vulnerability). OpenSSL 1.0.1 with Heartbleed (CVE-2014-0160). No patch management.",
            "Brute force attack: 2000 login attempts from 192.168.1.50 in 5 minutes. No account lockout after 10 failures. Session tokens not rotated.",
            "Package downloaded from PyPI with tampered checksum. npm dependency 'lodash@4.17.0' replaced with malicious version. No signature verification.",
            "Critical admin login at 3:00 AM from unknown IP 203.0.113.5. No alerts triggered. Logs deleted from /var/log/auth.log. SIEM offline for 6 hours.",
            "Internal request to http://169.254.169.254/latest/meta-data/ from web application. AWS IAM credentials leaked. Attacker accessed internal APIs."
        };

        var random = new Random();
        var logSelecionado = logs[random.Next(logs.Length)];
        var report = await gemini.AnalyzeSecurityThreat(logSelecionado, "Warzone");
        await obsidian.SaveNote(report, $"Alert-OWASP-{DateTime.Now:yyyyMMdd-HHmmss}");

        return Results.Ok(new { log = logSelecionado, analysis = report });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

MigrationRunner.ApplyMigrations(app.Services, connectionString!, dbPassword);

app.Run();
