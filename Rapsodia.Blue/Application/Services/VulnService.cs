// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Th1eros

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Common;
using Rapsodia.Blue.Domain.Entities;

namespace Rapsodia.Blue.Application.Services;

public class VulnService : IVulnService
{
    private readonly IVulnRepositoryPort _vulnRepository;
    private readonly IAssetRepositoryPort _assetRepository;

    public VulnService(IVulnRepositoryPort vulnRepository, IAssetRepositoryPort assetRepository)
    {
        _vulnRepository = vulnRepository ?? throw new ArgumentNullException(nameof(vulnRepository));
        _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
    }

    public async Task<ResponseModel<List<VulnResponse>>> ListAsync(CancellationToken ct = default)
    {
        try
        {
            var vulns = await _vulnRepository.ListAllAsync();
            var vulnList = vulns.Where(v => v.DeletedAt == null).Select(MapToResponse).ToList();
            return ResponseModel<List<VulnResponse>>.CreateSuccess(vulnList);
        }
        catch (Exception ex)
        {
            return ResponseModel<List<VulnResponse>>.CreateError($"Erro ao listar vulnerabilidades: {ex.Message}");
        }
    }

    public async Task<ResponseModel<VulnResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var vuln = await _vulnRepository.GetByIdWithRelationsAsync(id);
            if (vuln == null || vuln.DeletedAt != null)
                return ResponseModel<VulnResponse>.CreateError("Vulnerabilidade nao encontrada");

            return ResponseModel<VulnResponse>.CreateSuccess(MapToResponse(vuln));
        }
        catch (Exception ex)
        {
            return ResponseModel<VulnResponse>.CreateError($"Erro ao buscar vulnerabilidade: {ex.Message}");
        }
    }

    public async Task<ResponseModel<VulnResponse>> CreateAsync(CreateVulnRequest request, CancellationToken ct = default)
    {
        try
        {
            if (request.RelatedVulnIds is { Count: > 0 })
            {
                var distinctIds = new HashSet<int>(request.RelatedVulnIds);
                if (distinctIds.Count != request.RelatedVulnIds.Count)
                    return ResponseModel<VulnResponse>.CreateError("IDs duplicados na lista de relacionados");
            }

            var vuln = new Vuln(
                code: request.Code.Trim(),
                title: request.Title.Trim(),
                description: request.Description,
                level: request.Level,
                environment: request.Environment,
                parentVulnId: request.ParentVulnId
            );

            if (request.RelatedVulnIds is { Count: > 0 })
            {
                var relatedVulns = await _vulnRepository.GetByIdsAsync(request.RelatedVulnIds);
                foreach (var rv in relatedVulns.Where(v => v.DeletedAt == null))
                    vuln.RelatedVulns.Add(rv);
            }

            if (request.AssetIds is { Count: > 0 })
            {
                foreach (var assetId in request.AssetIds.Distinct())
                {
                    var asset = await _assetRepository.GetByIdAsync(assetId);
                    if (asset is { DeletedAt: null })
                    {
                        vuln.AssetVulns.Add(new AssetVuln(
                            asset: asset,
                            vuln: vuln,
                            status: "Open"
                        ));
                    }
                }
            }

            await _vulnRepository.SaveAsync(vuln);
            return ResponseModel<VulnResponse>.CreateSuccess(MapToResponse(vuln));
        }
        catch (Exception ex)
        {
            return ResponseModel<VulnResponse>.CreateError($"Erro ao criar vulnerabilidade: {ex.Message}");
        }
    }

    public async Task<ResponseModel<VulnResponse>> UpdateAsync(int id, EditVulnRequest request, CancellationToken ct = default)
    {
        try
        {
            var vuln = await _vulnRepository.GetByIdAsync(id);
            if (vuln is not { DeletedAt: null })
                return ResponseModel<VulnResponse>.CreateError("Vulnerabilidade nao encontrada");

            if (request.RelatedVulnIds is { Count: > 0 })
            {
                if (request.RelatedVulnIds.Contains(id))
                    return ResponseModel<VulnResponse>.CreateError("Auto-relacionamento nao permitido");

                var distinctIds = new HashSet<int>(request.RelatedVulnIds);
                if (distinctIds.Count != request.RelatedVulnIds.Count)
                    return ResponseModel<VulnResponse>.CreateError("IDs duplicados na lista de relacionados");
            }

            if (request.ParentVulnId.HasValue && request.ParentVulnId.Value == id)
                return ResponseModel<VulnResponse>.CreateError("Uma vulnerabilidade nao pode ser pai de si mesma");

            vuln.Update(
                code: request.Code?.Trim(),
                title: request.Title?.Trim(),
                description: request.Description,
                level: request.Level,
                environment: request.Environment,
                parentVulnId: request.ParentVulnId
            );

            if (request.RelatedVulnIds != null)
            {
                vuln.RelatedVulns.Clear();
                var relatedVulns = await _vulnRepository.GetByIdsAsync(request.RelatedVulnIds);
                foreach (var rv in relatedVulns.Where(v => v is { DeletedAt: null } && v.Id != id))
                    vuln.RelatedVulns.Add(rv);
            }

            await _vulnRepository.UpdateAsync(vuln);
            return ResponseModel<VulnResponse>.CreateSuccess(MapToResponse(vuln));
        }
        catch (Exception ex)
        {
            return ResponseModel<VulnResponse>.CreateError($"Erro ao atualizar vulnerabilidade: {ex.Message}");
        }
    }

    public async Task<ResponseModel<bool>> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var vuln = await _vulnRepository.GetByIdAsync(id);
            if (vuln == null)
                return ResponseModel<bool>.CreateError("Vulnerabilidade nao encontrada");
            if (vuln.DeletedAt != null)
                return ResponseModel<bool>.CreateError("Vulnerabilidade ja esta removida");

            vuln.MarkAsDeleted();
            vuln.MarkAsUpdated();
            await _vulnRepository.UpdateAsync(vuln);
            return ResponseModel<bool>.CreateSuccess(true);
        }
        catch (Exception ex)
        {
            return ResponseModel<bool>.CreateError($"Erro ao remover vulnerabilidade: {ex.Message}");
        }
    }

    public async Task<ResponseModel<bool>> RestoreAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var vuln = await _vulnRepository.GetByIdAsync(id);
            if (vuln == null)
                return ResponseModel<bool>.CreateError("Vulnerabilidade nao encontrada");
            if (vuln.DeletedAt == null)
                return ResponseModel<bool>.CreateError("Vulnerabilidade ja esta ativa");

            vuln.MarkAsRestored();
            vuln.MarkAsUpdated();
            await _vulnRepository.UpdateAsync(vuln);
            return ResponseModel<bool>.CreateSuccess(true);
        }
        catch (Exception ex)
        {
            return ResponseModel<bool>.CreateError($"Erro ao restaurar vulnerabilidade: {ex.Message}");
        }
    }

    public async Task<ResponseModel<VulnResponse>> AddToAssetAsync(int assetId, AddVulnToAssetRequest request, CancellationToken ct = default)
    {
        try
        {
            var asset = await _assetRepository.GetByIdAsync(assetId);
            if (asset is not { DeletedAt: null })
                return ResponseModel<VulnResponse>.CreateError("Asset nao encontrado ou desabilitado");

            var vuln = await _vulnRepository.GetByIdAsync(request.VulnId);
            if (vuln is not { DeletedAt: null })
                return ResponseModel<VulnResponse>.CreateError("Vulnerabilidade nao encontrada ou removida");

            if (!vuln.AssetVulns.Any(av => av.AssetId == assetId))
            {
                vuln.AssetVulns.Add(new AssetVuln(
                    asset: asset,
                    vuln: vuln,
                    status: "Open",
                    notes: request.Notes ?? string.Empty
                ));
                vuln.MarkAsUpdated();
                await _vulnRepository.UpdateAsync(vuln);
            }

            return ResponseModel<VulnResponse>.CreateSuccess(MapToResponse(vuln));
        }
        catch (Exception ex)
        {
            return ResponseModel<VulnResponse>.CreateError($"Erro ao associar vulnerabilidade ao asset: {ex.Message}");
        }
    }

    public async Task<Result<VulnStatsDTO>> GetStatsAsync(CancellationToken ct = default)
    {
        try
        {
            var vulns = await _vulnRepository.ListAllAsync();
            var active = vulns.Where(v => v.DeletedAt == null).ToList();

            var topSignatures = active
                .GroupBy(v => v.Title)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new AttackSignatureDTO
                {
                    Name = g.Key,
                    Count = g.Count().ToString(),
                    Percentage = active.Count > 0 ? (int)((double)g.Count() / active.Count * 100) : 0
                })
                .ToList();

            return Result<VulnStatsDTO>.Ok(new VulnStatsDTO
            {
                TotalCount = active.Count,
                TopSignatures = topSignatures
            });
        }
        catch (Exception ex)
        {
            return Result<VulnStatsDTO>.Fail($"Erro ao buscar estatísticas de vulnerabilidades: {ex.Message}");
        }
    }

    public async Task<Result<VulnTrendDTO>> GetTrendAsync(TrendFilterDTO filter, CancellationToken ct = default)
    {
        try
        {
            var vulns = await _vulnRepository.ListAllAsync();
            var active = vulns.Where(v => v.DeletedAt == null).ToList();

            var startDate = filter.StartDate ?? DateTime.UtcNow.AddDays(-30);
            var endDate = filter.EndDate ?? DateTime.UtcNow;

            var dataPoints = active
                .Where(v => v.CreatedAt >= startDate && v.CreatedAt <= endDate)
                .GroupBy(v => v.CreatedAt.Date)
                .Select(g => new TrendDataPointDTO
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Label = g.Key.ToString("dd/MM")
                })
                .OrderBy(d => d.Date)
                .ToList();

            return Result<VulnTrendDTO>.Ok(new VulnTrendDTO
            {
                DataPoints = dataPoints,
                MetricType = filter.MetricType ?? "vulnerabilities"
            });
        }
        catch (Exception ex)
        {
            return Result<VulnTrendDTO>.Fail($"Erro ao buscar tendências de vulnerabilidades: {ex.Message}");
        }
    }

    private static VulnResponse MapToResponse(Vuln vuln)
    {
        return new VulnResponse(
            vuln.Id,
            vuln.Code,
            vuln.Title,
            vuln.Description,
            vuln.Level,
            vuln.Environment,
            vuln.ParentVulnId,
            vuln.ParentVuln?.Title ?? string.Empty,
            vuln.ChildVulns?
                .Where(c => c.DeletedAt == null)
                .Select(c => new VulnChildResponse(c.Id, c.Code, c.Title, c.Level, c.Environment))
                .ToList() ?? [],
            vuln.RelatedVulns?
                .Where(r => r.DeletedAt == null)
                .Select(r => new VulnRelatedResponse(r.Id, r.Code, r.Title, r.Level, r.Environment, "Related"))
                .ToList() ?? [],
            vuln.AssetVulns?
                .Where(av => av.Asset?.DeletedAt == null)
                .Select(av => new VulnAssetResponse(
                    av.AssetId,
                    av.Asset?.Name ?? "Desconhecido",
                    av.Asset?.AssetType?.Name ?? "Desconhecido",
                    av.Status,
                    av.DiscoveredAt))
                .ToList() ?? [],
            vuln.CreatedAt
        );
    }
}