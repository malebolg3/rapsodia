using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rapsodia.Blue.Application.DTOs;
using Rapsodia.Blue.Application.Interfaces;
using Rapsodia.Blue.Domain.Entities;

namespace Rapsodia.Blue.Application.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepositoryPort _repository;

    public AssetService(IAssetRepositoryPort repository)
    {
        _repository = repository;
    }

    public async Task<ResponseModel<List<AssetResponse>>> ListAsync(CancellationToken ct = default)
    {
        try
        {
            var assets = await _repository.ListAllActiveAsync();
            var assetList = assets.Where(a => a.DeletedAt == null).Select(MapToResponse).ToList();
            return ResponseModel<List<AssetResponse>>.CreateSuccess(assetList);
        }
        catch (Exception ex)
        {
            return ResponseModel<List<AssetResponse>>.CreateError($"Erro ao listar assets: {ex.Message}");
        }
    }

    public async Task<ResponseModel<AssetResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var asset = await _repository.GetByIdWithVulnsAsync(id);
            if (asset == null || asset.DeletedAt != null)
                return ResponseModel<AssetResponse>.CreateError("Asset nao encontrado");

            return ResponseModel<AssetResponse>.CreateSuccess(MapToResponse(asset));
        }
        catch (Exception ex)
        {
            return ResponseModel<AssetResponse>.CreateError($"Erro ao buscar asset: {ex.Message}");
        }
    }

    public async Task<ResponseModel<AssetResponse>> CreateAsync(CreateAssetRequest request, CancellationToken ct = default)
    {
        try
        {
            if (await _repository.ExistsByNameAsync(request.Name))
                return ResponseModel<AssetResponse>.CreateError("Ja existe um asset com este nome");

            var asset = new Asset();
                asset.Update(
                    name: request.Name.Trim(),
                    assetTypeId: request.TypeId,
                    environment: request.Environment,
                    isEnabled: request.Enabled,
                    parentAssetId: request.ParentAssetId
                );

            if (request.RelatedAssetIds?.Count > 0)
            {
                foreach (var relatedId in request.RelatedAssetIds.Distinct())
                {
                    var related = await _repository.GetByIdAsync(relatedId);
                    if (related != null && related.DeletedAt == null)
                        asset.RelatedAssets.Add(related);
                }
            }

            await _repository.SaveAsync(asset);
            return ResponseModel<AssetResponse>.CreateSuccess(MapToResponse(asset));
        }
        catch (Exception ex)
        {
            return ResponseModel<AssetResponse>.CreateError($"Erro ao criar asset: {ex.Message}");
        }
    }

    public async Task<ResponseModel<AssetResponse>> UpdateAsync(int id, EditAssetRequest request, CancellationToken ct = default)
    {
        try
        {
            var asset = await _repository.GetByIdAsync(id);
            if (asset == null || asset.DeletedAt != null)
                return ResponseModel<AssetResponse>.CreateError("Asset nao encontrado");

            if (request.Name != null)
            {
                if (await _repository.ExistsByNameExceptIdAsync(request.Name, id))
                    return ResponseModel<AssetResponse>.CreateError("Nome ja esta em uso");
            }

            asset.Update(
                name: request.Name?.Trim(),
                assetTypeId: request.TypeId,
                environment: request.Environment,
                isEnabled: request.Enabled,
                parentAssetId: request.ParentAssetId
            );

            if (request.RelatedAssetIds != null)
            {
                if (request.RelatedAssetIds.Contains(id))
                    return ResponseModel<AssetResponse>.CreateError("Auto-relacionamento nao permitido");

                asset.RelatedAssets.Clear();
                foreach (var relatedId in request.RelatedAssetIds.Distinct())
                {
                    var related = await _repository.GetByIdAsync(relatedId);
                    if (related != null && related.DeletedAt == null && related.Id != id)
                        asset.RelatedAssets.Add(related);
                }
            }

            await _repository.UpdateAsync(asset);
            return ResponseModel<AssetResponse>.CreateSuccess(MapToResponse(asset));
        }
        catch (Exception ex)
        {
            return ResponseModel<AssetResponse>.CreateError($"Erro ao atualizar asset: {ex.Message}");
        }
    }

    public async Task<ResponseModel<bool>> DisableAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var asset = await _repository.GetByIdAsync(id);
            if (asset == null)
                return ResponseModel<bool>.CreateError("Asset nao encontrado");
            if (asset.DeletedAt != null)
                return ResponseModel<bool>.CreateError("Asset ja esta desabilitado");

            asset.MarkAsDeleted();
            asset.Update(isEnabled: false);
            await _repository.UpdateAsync(asset);
            return ResponseModel<bool>.CreateSuccess(true);
        }
        catch (Exception ex)
        {
            return ResponseModel<bool>.CreateError($"Erro ao desabilitar asset: {ex.Message}");
        }
    }

    public async Task<ResponseModel<bool>> EnableAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var asset = await _repository.GetByIdAsync(id);
            if (asset == null)
                return ResponseModel<bool>.CreateError("Asset nao encontrado");
            if (asset.DeletedAt == null)
                return ResponseModel<bool>.CreateError("Asset ja esta habilitado");

            asset.MarkAsRestored();
            asset.Update(isEnabled: true);
            await _repository.UpdateAsync(asset);
            return ResponseModel<bool>.CreateSuccess(true);
        }
        catch (Exception ex)
        {
            return ResponseModel<bool>.CreateError($"Erro ao habilitar asset: {ex.Message}");
        }
    }

    public async Task<ResponseModel<AssetResponse>> AddRelatedAsync(int assetId, int relatedAssetId, CancellationToken ct = default)
    {
        try
        {
            if (assetId == relatedAssetId)
                return ResponseModel<AssetResponse>.CreateError("Auto-relacionamento nao permitido");

            var asset = await _repository.GetByIdAsync(assetId);
            if (asset == null || asset.DeletedAt != null)
                return ResponseModel<AssetResponse>.CreateError("Asset nao encontrado ou desabilitado");

            var related = await _repository.GetByIdAsync(relatedAssetId);
            if (related == null || related.DeletedAt != null)
                return ResponseModel<AssetResponse>.CreateError("Asset relacionado nao encontrado ou desabilitado");

            if (!asset.RelatedAssets.Any(a => a.Id == relatedAssetId))
            {
                asset.RelatedAssets.Add(related);
                asset.MarkAsUpdated();
                await _repository.UpdateAsync(asset);
            }

            return ResponseModel<AssetResponse>.CreateSuccess(MapToResponse(asset));
        }
        catch (Exception ex)
        {
            return ResponseModel<AssetResponse>.CreateError($"Erro ao relacionar assets: {ex.Message}");
        }
    }

    public async Task<ResponseModel<AssetResponse>> RemoveRelatedAsync(int assetId, int relatedAssetId, CancellationToken ct = default)
    {
        try
        {
            var asset = await _repository.GetByIdAsync(assetId);
            if (asset == null || asset.DeletedAt != null)
                return ResponseModel<AssetResponse>.CreateError("Asset nao encontrado ou desabilitado");

            var related = asset.RelatedAssets.FirstOrDefault(a => a.Id == relatedAssetId);
            if (related != null)
            {
                asset.RelatedAssets.Remove(related);
                asset.MarkAsUpdated();
                await _repository.UpdateAsync(asset);
            }

            return ResponseModel<AssetResponse>.CreateSuccess(MapToResponse(asset));
        }
        catch (Exception ex)
        {
            return ResponseModel<AssetResponse>.CreateError($"Erro ao remover relacao: {ex.Message}");
        }
    }

    private static AssetResponse MapToResponse(Asset asset)
    {
        return new AssetResponse(
            Id: asset.Id,
            Name: asset.Name,
            TypeName: asset.AssetType?.Name ?? "Desconhecido",
            Environment: asset.Environment,
            Enabled: asset.IsEnabled,
            CreatedAt: asset.CreatedAt,
            ParentAssetId: asset.ParentAssetId,
            ParentAssetName: asset.ParentAsset?.Name ?? string.Empty,
            ChildAssets: asset.ChildAssets?
                .Where(c => c.DeletedAt == null)
                .Select(c => new AssetChildResponse(
                    Id: c.Id,
                    Name: c.Name,
                    TypeName: c.AssetType?.Name ?? "Desconhecido",
                    Environment: c.Environment,
                    Enabled: c.IsEnabled
                )).ToList() ?? new List<AssetChildResponse>(),
            RelatedAssets: asset.RelatedAssets?
                .Where(r => r.DeletedAt == null)
                .Select(r => new AssetRelatedResponse(
                    Id: r.Id,
                    Name: r.Name,
                    TypeName: r.AssetType?.Name ?? "Desconhecido",
                    Environment: r.Environment,
                    Enabled: r.IsEnabled
                )).ToList() ?? new List<AssetRelatedResponse>(),
            Vulns: asset.AssetVulns?
                .Where(av => av.Vuln != null && av.Vuln.DeletedAt == null)
                .Select(av => new VulnResponse(
                    Id: av.Vuln.Id,
                    Code: av.Vuln.Code,
                    Title: av.Vuln.Title,
                    Description: av.Vuln.Description,
                    Level: av.Vuln.Level,
                    Environment: av.Vuln.Environment,
                    ParentVulnId: av.Vuln.ParentVulnId,
                    ParentVulnTitle: av.Vuln.ParentVuln?.Title ?? string.Empty,
                    ChildVulns: new List<VulnChildResponse>(),
                    RelatedVulns: new List<VulnRelatedResponse>(),
                    Assets: new List<VulnAssetResponse>
                    {
                        new VulnAssetResponse(
                            AssetId: asset.Id,
                            AssetName: asset.Name,
                            AssetType: asset.AssetType?.Name ?? "Desconhecido",
                            Status: av.Status,
                            DiscoveredAt: av.DiscoveredAt
                        )
                    },
                    CreatedAt: av.Vuln.CreatedAt
                )).ToList() ?? new List<VulnResponse>()
        );
    }
}