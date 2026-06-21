// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;

namespace Rapsodia.Blue.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("Api")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserFilterDTO filter, CancellationToken ct)
    {
        var result = await _service.ListAsync(filter, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("Auth")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EditUserRequest request, CancellationToken ct)
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

    [HttpPost("{id:int}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddRole(int id, [FromBody] AddRoleRequest request, CancellationToken ct)
    {
        var result = await _service.AddRoleAsync(id, request, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}/roles/{roleId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveRole(int id, int roleId, CancellationToken ct)
    {
        var result = await _service.RemoveRoleAsync(id, roleId, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}