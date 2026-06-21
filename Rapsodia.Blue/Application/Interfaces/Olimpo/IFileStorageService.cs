// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Rapsodia.Blue.Application.Interfaces.Olimpo;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile file, string path, CancellationToken ct);
    Task DeleteAsync(string path, CancellationToken ct);
}