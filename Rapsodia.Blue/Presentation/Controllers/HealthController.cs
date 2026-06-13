using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rapsodia.Blue.Infrastructure.Configuration;

namespace Rapsodia.Blue.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class HealthController : ControllerBase
{
    private readonly DatabaseConfigService _dbConfig;

    public HealthController(DatabaseConfigService dbConfig)
    {
        _dbConfig = dbConfig ?? throw new ArgumentNullException(nameof(dbConfig));
    }

    [HttpGet]
    [Produces("text/html")]
    public IActionResult Get()
    {
        var html = $@"
            <div class='tel-item'><span class='status-dot pulse-violet'></span>ENV: <strong style='color:var(--text-main);'>{_dbConfig.Environment.ToUpper()}</strong></div>
            <div class='tel-item'>SILVER: <strong style='color:{(_dbConfig.SilverEnabled ? "var(--agent-color)" : "var(--text-muted)")};'>{(_dbConfig.SilverEnabled ? "ACTIVE" : "DISABLED")}</strong></div>
            <div class='tel-item'>REDIS: <strong style='color:{(_dbConfig.RedisEnabled ? "var(--agent-color)" : "var(--text-muted)")};'>{(_dbConfig.RedisEnabled ? "ACTIVE" : "DISABLED")}</strong></div>
            <div class='tel-item'>ORLEANS: <strong style='color:{(_dbConfig.OrleansEnabled ? "var(--agent-color)" : "var(--text-muted)")};'>{(_dbConfig.OrleansEnabled ? "ACTIVE" : "DISABLED")}</strong></div>";

        return Content(html, "text/html");
    }
}