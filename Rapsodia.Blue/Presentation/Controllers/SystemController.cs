// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.AspNetCore.Mvc;
using Rapsodia.Blue.Infrastructure.Data;

namespace Rapsodia.Blue.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    [HttpPost("toggle-offline")]
    public IActionResult ToggleOffline()
    {
        DatabaseToggle.SetAutoFallback(false);
        DatabaseToggle.UseSqlite = !DatabaseToggle.UseSqlite;

        return Ok(new
        {
            offline = DatabaseToggle.UseSqlite,
            autoFallback = DatabaseToggle.IsAutoFallback,
            provider = DatabaseToggle.UseSqlite ? "SQLite" : "Oracle",
            message = DatabaseToggle.UseSqlite ? "Modo offline ativado (Auto-Fallback desativado)" : "Modo online ativado (Auto-Fallback desativado)"
        });
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            offline = DatabaseToggle.UseSqlite,
            autoFallback = DatabaseToggle.IsAutoFallback,
            provider = DatabaseToggle.UseSqlite ? "SQLite" : "Oracle"
        });
    }
}