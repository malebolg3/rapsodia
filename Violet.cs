using System.Text;
using System.Threading.RateLimiting;
using DotNetEnv;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Rapsodia.Silver.Infrastructure.Extensions;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var environmentName = Environment.GetEnvironmentVariable("ENV") ?? "Development";

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath)) Env.Load(envPath);

var envFile = $".env.{environmentName.ToLower()}";
var envSpecificPath = Path.Combine(Directory.GetCurrentDirectory(), envFile);
if (File.Exists(envSpecificPath)) Env.Load(envSpecificPath);

var builder = WebApplication.CreateBuilder(args);
builder.Environment.EnvironmentName = environmentName;

builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName.ToLower()}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddCoreServices(builder.Configuration, builder.Environment);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

var app = builder.Build();

app.UseCors("DefaultPolicy");

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/health", () => Results.Ok(new 
{ 
    service = "Violet",
    version = "1.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    status = "healthy"
}));

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Violet Lab API v1");
    c.RoutePrefix = "swagger";
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

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
        var maxLabs = int.Parse(user.FindFirst("max_labs")?.Value ?? "0");
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (!scope?.StartsWith("violet:") ?? true)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Insufficient scope", required = "violet:*" });
            return;
        }

        if (path.Contains("/api/lab/create") && maxLabs == 0)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Labs nao inclusos no seu plano" });
            return;
        }
    }

    await next();
});

app.UseRateLimiter();
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

var url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:10000";
Console.WriteLine($"Violet Lab API iniciando em {url}");
Console.WriteLine($"Swagger: {url}/swagger");
await app.RunAsync(url);