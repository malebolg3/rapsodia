using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rapsodia.Red.Application;
using Rapsodia.Red.Infrastructure;
using Rapsodia.Red.Infrastructure.Exploitation;
using Rapsodia.Red.Infrastructure.Exploitation.PostExploit;
using Rapsodia.Red.Presentation;
using Rapsodia.Silver.Infrastructure.Extensions;
using Rapsodia.Red.Domain.Interfaces;
using Rapsodia.Red.Infrastructure.Adapters;
using Rapsodia.Red.Application.Services;
using Rapsodia.Red.Presentation.Workers;

namespace Rapsodia.Red;

public class Sword
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        var environmentName = Environment.GetEnvironmentVariable("ENV") ?? "Development";

        var envPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env"));
        if (File.Exists(envPath)) Env.Load(envPath);

        var envFile = $".env.{environmentName.ToLower()}";
        var envSpecificPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", envFile));
        if (File.Exists(envSpecificPath)) Env.Load(envSpecificPath);

        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        builder.Services.AddCoreServices(builder.Configuration, builder.Environment);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var aiUrl = Environment.GetEnvironmentVariable("AI_URL") ?? throw new InvalidOperationException("AI_URL nao definida.");
        var adapterType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => typeof(IAiAnalystPort).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            ?? throw new InvalidOperationException("Adaptador dinamico para IAiAnalystPort nao encontrado.");

        var method = typeof(HttpClientFactoryServiceCollectionExtensions)
            .GetMethods()
            .First(m => m.Name == "AddHttpClient" && m.IsGenericMethod && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 1);

        var httpClientBuilder = (IHttpClientBuilder)method.MakeGenericMethod(typeof(IAiAnalystPort), adapterType).Invoke(null, new object[] { builder.Services })!;
        httpClientBuilder.ConfigureHttpClient(c => c.BaseAddress = new Uri(aiUrl));

        var msfHost = Environment.GetEnvironmentVariable("SEC_HOST") ?? "localhost";
        var msfPort = Environment.GetEnvironmentVariable("SEC_PORT") ?? "55553";
        builder.Services.AddHttpClient<MetasploitRpcClient>(c =>
        {
            c.BaseAddress = new Uri($"http://{msfHost}:{msfPort}");
        });

        var origins = Environment.GetEnvironmentVariable("CORS")
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(o => o.Trim())
            .ToArray() ?? Array.Empty<string>();

        builder.Services.AddCors(o => o.AddPolicy("RedPolicy", p => p
            .WithOrigins(origins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));

        builder.Services.AddSingleton<KaliToolRunner>();
        builder.Services.AddSingleton<IScanPort, KaliToolsAdapter>();
        builder.Services.AddSingleton<IPostExploitPort, SharpSploitService>();
        builder.Services.AddScoped<ISecurityOrchestrator, SecurityOrchestrator>();
        builder.Services.AddHostedService<RedScanWorker>();

        var app = builder.Build();
        
        app.UseCors("RedPolicy");

        app.MapGet("/", () => Results.Redirect("/swagger/index.html"));
        
        app.MapGet("/status", () => Results.Ok(new 
        { 
            service = "Red Sword API",
            version = "1.0",
            environment = app.Environment.EnvironmentName,
            timestamp = DateTime.UtcNow,
            worker = "RedScanWorker",
            status = "operational"
        }));

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Red Sword API v1");
                c.RoutePrefix = "swagger";
            });
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
                var allowExploit = bool.Parse(user.FindFirst("allow_exploit")?.Value ?? "false");
                var allowPostExploit = bool.Parse(user.FindFirst("allow_post_exploit")?.Value ?? "false");
                var path = context.Request.Path.Value?.ToLower() ?? "";

                if (!scope?.StartsWith("red:") ?? true)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { error = "Insufficient scope", required = "red:*" });
                    return;
                }

                if (path.Contains("/api/exploit/exploit") && !allowExploit)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { error = "Exploit nao incluso no seu plano" });
                    return;
                }

                if (path.Contains("/api/exploit/post") && !allowPostExploit)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { error = "Pos-Exploit nao incluso no seu plano" });
                    return;
                }
            }

            await next();
        });

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

        var port = Environment.GetEnvironmentVariable("PORT_RED") ?? throw new InvalidOperationException("PORT_RED nao definida.");
        Console.WriteLine($"Red Sword API iniciando em http://0.0.0.0:{port}");
        Console.WriteLine($"Swagger: http://0.0.0.0:{port}/swagger");        
        await app.RunAsync($"http://+:{port}");
    }
}