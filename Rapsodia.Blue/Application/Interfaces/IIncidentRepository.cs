// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using Rapsodia.Blue.Domain.Entities;

namespace Rapsodia.Blue.Application.Interfaces;

public interface IIncidentRepository
{
    Task<Incident?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Incident>> ListAllAsync(CancellationToken ct = default);
    Task<Incident> SaveAsync(Incident incident, CancellationToken ct = default);
    Task<Incident> UpdateAsync(Incident incident, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}