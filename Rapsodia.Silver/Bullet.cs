using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using DotNetEnv;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orleans.Configuration;
using Rapsodia.Silver.Application.Interfaces;
using Rapsodia.Silver.Application.Services;
using Rapsodia.Silver.Infrastructure.Extensions;
using Rapsodia.Silver.Infrastructure.Services;
using Rapsodia.Silver.Spart;
using Rapsodia.Silver.Spart.Grains;
using Rapsodia.Silver.Spart.Interfaces;
using Orleans.Streams;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var environmentName = Environment.GetEnvironmentVariable("ENV") ?? "Development";

var envPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env"));
if (File.Exists(envPath)) Env.Load(envPath);

var envFile = $".env.{environmentName.ToLower()}";
var envSpecificPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", envFile));
if (File.Exists(envSpecificPath)) Env.Load(envSpecificPath);

var builder = WebApplication.CreateBuilder(args);
builder.Environment.EnvironmentName = environmentName;

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName.ToLower()}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Silver Bullet API", 
        Version = "v1",
        Description = "Core infrastructure, AI orchestration, and multi-agent system"
    });
});

builder.Services.AddCoreServices(builder.Configuration, builder.Environment);
builder.Services.AddSingleton<EventPublisher>();
builder.Services.AddSingleton<TelemetryService>();
builder.Services.AddHttpClient<AiClient>();
builder.Services.AddHttpClient("AIAgent", c =>
{
    c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("AI_URL") ?? "http://localhost:11434");
    c.Timeout = TimeSpan.FromMinutes(5);
});

builder.Services.AddHttpClient<IObsidianService, ObsidianService>(c =>
{
    c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("DOC_URL") ?? "http://localhost:27124");
    c.DefaultRequestHeaders.Add("X-API-Key", Environment.GetEnvironmentVariable("DOC_KEY") ?? "");
});

var origins = Environment.GetEnvironmentVariable("CORS")
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(o => o.Trim())
    .ToArray() ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

builder.Services.AddSingleton<IObsidianService, ObsidianService>();

var enableOrleans = Environment.GetEnvironmentVariable("ENB_ORLN") ?? builder.Configuration["ENB_ORLN"];
if (enableOrleans?.ToLower() == "true")
{
    var clusterId = Environment.GetEnvironmentVariable("ORLN_CLUSTER") ?? "silver-cluster";
    var serviceId = Environment.GetEnvironmentVariable("ORLN_SERVICE") ?? "silver-service";
    
    builder.Services.AddOrleans(silo => 
    {
        silo.UseLocalhostClustering();
        silo.AddMemoryGrainStorage("Default");
        silo.AddMemoryStreams("Default");
        silo.Configure<SiloOptions>(options => { options.SiloName = "SilverSilo"; });
        silo.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = clusterId;
            options.ServiceId = serviceId;
        });
    });
    
    builder.Services.AddSingleton<GrainRouter>();
    builder.Services.AddHostedService<OrleansHostedService>();
}

var telemetryEndpoint = Environment.GetEnvironmentVariable("TLMT_OTLP") ?? builder.Configuration["TLMT_OTLP"] ?? "http://localhost:4317";
var serviceName = Environment.GetEnvironmentVariable("TLMT_SVC") ?? builder.Configuration["TLMT_SVC"] ?? "Silver";
var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => 
        resource.AddService(
            serviceName: serviceName, 
            serviceVersion: serviceVersion,
            serviceInstanceId: Environment.MachineName
        ))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter(o => 
        {
            o.Endpoint = new Uri(telemetryEndpoint);
            o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

app.UseCors("DefaultPolicy");

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/health", () => Results.Ok(new 
{ 
    service = "Silver Bullet",
    version = serviceVersion,
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    status = "healthy",
    orleans = enableOrleans?.ToLower() == "true" ? "enabled" : "disabled",
    telemetry = new { endpoint = telemetryEndpoint, prometheus = "/metrics" }
}));

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Silver Bullet API v1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();

var authMode = Environment.GetEnvironmentVariable("AUTH_MODE") ?? "jwt";

if (authMode != "standalone")
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.Use(async (context, next) =>
{
    if (authMode == "standalone")
    {
        await next();
        return;
    }

    var user = context.User;
    if (user?.Identity?.IsAuthenticated == true)
    {
        var scope = user.FindFirst("scope")?.Value;
        var maxAgents = int.Parse(user.FindFirst("max_agents")?.Value ?? "0");
        var allowOrch = bool.Parse(user.FindFirst("allow_orch")?.Value ?? "false");
        var allowObsidian = bool.Parse(user.FindFirst("allow_obsidian")?.Value ?? "false");
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (!scope?.StartsWith("silver:") ?? true)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Insufficient scope", required = "silver:*" });
            return;
        }

        if (path.Contains("/api/orchestration") && !allowOrch)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Orquestracao nao inclusa no seu plano" });
            return;
        }

        if (path.Contains("/api/agent/create") && maxAgents == 0)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Agentes IA nao inclusos no seu plano" });
            return;
        }

        if (path.Contains("/api/obsidian") && !allowObsidian)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Obsidian nao incluso no seu plano" });
            return;
        }
    }

    await next();
});

app.UseRateLimiter();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapControllers();

app.Use(async (context, next) =>
{
    await next();
    
    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = 404;
        await context.Response.WriteAsJsonAsync(new 
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            title = "Not Found",
            status = 404,
            detail = $"Endpoint {context.Request.Method} {context.Request.Path} nao encontrado",
            documentation = "/swagger"
        });
    }
});

var port = Environment.GetEnvironmentVariable("PORT_SLV") ?? throw new InvalidOperationException("PORT_SLV nao definida.");
Console.WriteLine($"Silver Bullet API iniciando em http://0.0.0.0:{port}");
Console.WriteLine($"Swagger: http://0.0.0.0:{port}/swagger");
await app.RunAsync($"http://0.0.0.0:{port}");