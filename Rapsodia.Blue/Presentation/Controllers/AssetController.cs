// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;

namespace Rapsodia.Blue.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("Api")]
public class AssetController : ControllerBase
{
    private readonly IAssetService _service;

    public AssetController(IAssetService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _service.ListAsync(ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAssetRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EditAssetRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateAsync(id, request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Disable(int id, CancellationToken ct)
    {
        var result = await _service.DisableAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPatch("{id:int}/enable")]
    public async Task<IActionResult> Enable(int id, CancellationToken ct)
    {
        var result = await _service.EnableAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:int}/related/{relatedId:int}")]
    public async Task<IActionResult> AddRelated(int id, int relatedId, CancellationToken ct)
    {
        var result = await _service.AddRelatedAsync(id, relatedId, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}/related/{relatedId:int}")]
    public async Task<IActionResult> RemoveRelated(int id, int relatedId, CancellationToken ct)
    {
        var result = await _service.RemoveRelatedAsync(id, relatedId, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}