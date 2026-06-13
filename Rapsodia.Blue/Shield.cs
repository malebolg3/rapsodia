using System.Text;
using System.Threading.RateLimiting;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Rapsodia.Blue.Infrastructure.Configuration;
using Rapsodia.Silver.Infrastructure.Extensions;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Application.Services;
using Rapsodia.Blue.Application.Middleware;

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
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName.ToLower()}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddBlueDatabase(builder.Configuration);
builder.Services.AddBlueServices();
builder.Services.AddBlueHttpClients(builder.Configuration);
builder.Services.AddBlueSwagger();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IAuthService, AuthAppService>();
builder.Services.AddSingleton<DatabaseConfigService>();
builder.Services.AddSingleton<TenantService>();

var authKey = Environment.GetEnvironmentVariable("AUTH_KEY") ?? throw new InvalidOperationException("AUTH_KEY obrigatoria.");
var authIss = Environment.GetEnvironmentVariable("AUTH_ISS") ?? throw new InvalidOperationException("AUTH_ISS obrigatoria.");
var authAud = Environment.GetEnvironmentVariable("AUTH_AUD") ?? throw new InvalidOperationException("AUTH_AUD obrigatoria.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey)),
            ValidateIssuer = true,
            ValidIssuer = authIss,
            ValidateAudience = true,
            ValidAudience = authAud,
            ValidateLifetime = true,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("Global", config =>
    {
        config.PermitLimit = 100;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("Auth", config =>
    {
        config.PermitLimit = 20;
        config.Window = TimeSpan.FromMinutes(5);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 5;
    });
    options.AddFixedWindowLimiter("Api", config =>
    {
        config.PermitLimit = 100;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 10;
    });
});

var origins = Environment.GetEnvironmentVariable("CORS")?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()).ToArray() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddPolicy("BluePolicy", p => p.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

var app = builder.Build();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blue API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHttpsRedirection();
}

var authMode = Environment.GetEnvironmentVariable("AUTH_MODE") ?? "jwt";

if (authMode != "standalone")
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseTenantLimits();

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
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (path.Contains("/api/auth/sessions") && (!scope?.StartsWith("blue:") ?? true))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Insufficient scope",
                required = "blue:*",
                current = scope ?? "none"
            });
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

var port = Environment.GetEnvironmentVariable("PORT_BLU") ?? throw new InvalidOperationException("PORT_BLU nao definida.");
Console.WriteLine($"Blue Shield pronto em http://localhost:{port}");
Console.WriteLine($"Swagger: http://localhost:{port}/swagger");
await app.RunAsync($"http://localhost:{port}");