using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Rapsodia.Blue.Application.DTOs.Olimpo;
using Rapsodia.Blue.Application.Interfaces.Olimpo;
using Rapsodia.Blue.Domain.Common;
using Rapsodia.Blue.Domain.Entities.Olimpo;

namespace Rapsodia.Blue.Application.Services.Olimpo;

public class OlimpoService : IOlimpoService
{
    private readonly IOlimpoRepositoryPort _repository;
    private readonly IEncryptionService _encryption;
    private readonly ITotpService _totp;
    private readonly IFileStorageService _storage;

    public OlimpoService(IOlimpoRepositoryPort repository, IEncryptionService encryption, ITotpService totp, IFileStorageService storage)
    {
        _repository = repository;
        _encryption = encryption;
        _totp = totp;
        _storage = storage;
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value;
        return Guid.Parse(claim!);
    }

    public Task<Result<OlimpoLoginResponse>> LoginAsync(OlimpoLoginRequest request, CancellationToken ct)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        return Task.FromResult(Result<OlimpoLoginResponse>.Ok(new OlimpoLoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        }));
    }

    public async Task<Result<OlimpoSecretResponse>> GetCiSecretAsync(ClaimsPrincipal user, string key, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var credential = await _repository.GetCredentialByKeyAsync(userId, key, "production", ct);
        if (credential is null)
            return Result<OlimpoSecretResponse>.Fail("Secret nao encontrado");

        return Result<OlimpoSecretResponse>.Ok(new OlimpoSecretResponse
        {
            Id = credential.Id.ToString(),
            Key = credential.Key,
            Value = _encryption.Decrypt(credential.EncryptedValue),
            Environment = credential.Environment,
            Category = credential.Category,
            CreatedAt = credential.CreatedAt,
            UpdatedAt = credential.UpdatedAt
        });
    }

    public async Task<Result<OlimpoCiEnvResponse>> ExportCiEnvAsync(ClaimsPrincipal user, string environment, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var credentials = await _repository.GetAllCredentialsAsync(userId, ct);
        var filtered = credentials.Where(c => c.Environment == environment);

        return Result<OlimpoCiEnvResponse>.Ok(new OlimpoCiEnvResponse
        {
            Environment = environment,
            Variables = filtered.ToDictionary(c => c.Key, c => _encryption.Decrypt(c.EncryptedValue))
        });
    }

    public async Task<Result<IEnumerable<OlimpoSecretResponse>>> GetAllSecretsAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var credentials = await _repository.GetAllCredentialsAsync(userId, ct);
        var response = credentials.Select(c => new OlimpoSecretResponse
        {
            Id = c.Id.ToString(),
            Key = c.Key,
            Value = _encryption.Decrypt(c.EncryptedValue),
            Environment = c.Environment,
            Category = c.Category,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });

        return Result<IEnumerable<OlimpoSecretResponse>>.Ok(response);
    }

    public async Task<Result<OlimpoSecretResponse>> GetSecretByIdAsync(ClaimsPrincipal user, string id, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var credential = await _repository.GetCredentialByIdAsync(userId, Guid.Parse(id), ct);
        if (credential is null)
            return Result<OlimpoSecretResponse>.Fail("Secret nao encontrado");

        return Result<OlimpoSecretResponse>.Ok(new OlimpoSecretResponse
        {
            Id = credential.Id.ToString(),
            Key = credential.Key,
            Value = _encryption.Decrypt(credential.EncryptedValue),
            Environment = credential.Environment,
            Category = credential.Category,
            CreatedAt = credential.CreatedAt,
            UpdatedAt = credential.UpdatedAt
        });
    }

    public async Task<Result<OlimpoSecretResponse>> CreateSecretAsync(ClaimsPrincipal user, OlimpoCreateSecretRequest request, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var encrypted = _encryption.Encrypt(request.Value);
        var credential = OlimpoCredential.Create(userId, request.Key, encrypted, request.Environment, request.Category);
        await _repository.AddCredentialAsync(credential, ct);

        return Result<OlimpoSecretResponse>.Ok(new OlimpoSecretResponse
        {
            Id = credential.Id.ToString(),
            Key = credential.Key,
            Value = request.Value,
            Environment = credential.Environment,
            Category = credential.Category,
            CreatedAt = credential.CreatedAt,
            UpdatedAt = credential.UpdatedAt
        });
    }

    public async Task<Result<OlimpoSecretResponse>> UpdateSecretAsync(ClaimsPrincipal user, string id, OlimpoUpdateSecretRequest request, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var credential = await _repository.GetCredentialByIdAsync(userId, Guid.Parse(id), ct);
        if (credential is null)
            return Result<OlimpoSecretResponse>.Fail("Secret nao encontrado");

        credential.UpdateValue(_encryption.Encrypt(request.Value));
        await _repository.UpdateCredentialAsync(credential, ct);

        return Result<OlimpoSecretResponse>.Ok(new OlimpoSecretResponse
        {
            Id = credential.Id.ToString(),
            Key = credential.Key,
            Value = request.Value,
            Environment = credential.Environment,
            Category = credential.Category,
            CreatedAt = credential.CreatedAt,
            UpdatedAt = credential.UpdatedAt
        });
    }

    public async Task<Result<bool>> DeleteSecretAsync(ClaimsPrincipal user, string id, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var credential = await _repository.GetCredentialByIdAsync(userId, Guid.Parse(id), ct);
        if (credential is null)
            return Result<bool>.Fail("Secret nao encontrado");

        await _repository.DeleteCredentialAsync(credential, ct);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<OlimpoTotpResponse>> GenerateTotpCodeAsync(ClaimsPrincipal user, string id, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var account = await _repository.GetTotpAccountByIdAsync(userId, Guid.Parse(id), ct);
        if (account is null)
            return Result<OlimpoTotpResponse>.Fail("Conta TOTP nao encontrada");

        var code = _totp.GenerateCode(account.Secret, account.Algorithm, account.Digits, account.Period);
        var remaining = account.Period - (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % account.Period);

        return Result<OlimpoTotpResponse>.Ok(new OlimpoTotpResponse
        {
            Id = account.Id.ToString(),
            Issuer = account.Issuer,
            Label = account.Label,
            Code = code,
            RemainingSeconds = remaining
        });
    }

    public async Task<Result<OlimpoTotpResponse>> AddTotpAccountAsync(ClaimsPrincipal user, OlimpoAddTotpRequest request, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var uri = new Uri(request.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var secret = query["secret"]!;
        var algorithm = query["algorithm"] ?? "SHA1";
        var digits = int.TryParse(query["digits"], out var d) ? d : 6;
        var period = int.TryParse(query["period"], out var p) ? p : 30;

        var account = OlimpoTotpAccount.Create(userId, request.Uri, secret, request.Label, request.Issuer, algorithm, digits, period);
        await _repository.AddTotpAccountAsync(account, ct);

        var code = _totp.GenerateCode(secret, algorithm, digits, period);
        return Result<OlimpoTotpResponse>.Ok(new OlimpoTotpResponse
        {
            Id = account.Id.ToString(),
            Issuer = account.Issuer,
            Label = account.Label,
            Code = code,
            RemainingSeconds = period
        });
    }

    public async Task<Result<bool>> DeleteTotpAccountAsync(ClaimsPrincipal user, string id, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var account = await _repository.GetTotpAccountByIdAsync(userId, Guid.Parse(id), ct);
        if (account is null)
            return Result<bool>.Fail("Conta TOTP nao encontrada");

        await _repository.DeleteTotpAccountAsync(account, ct);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<IEnumerable<OlimpoDocumentResponse>>> GetDocumentsAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var documents = await _repository.GetAllDocumentsAsync(userId, ct);
        var response = documents.Select(d => new OlimpoDocumentResponse
        {
            Id = d.Id.ToString(),
            Name = d.Name,
            FileName = d.FileName,
            Category = d.Category,
            Size = d.Size,
            Url = $"/api/olimpo/documents/{d.Id}/download",
            CreatedAt = d.CreatedAt
        });

        return Result<IEnumerable<OlimpoDocumentResponse>>.Ok(response);
    }

    public async Task<Result<OlimpoDocumentResponse>> UploadDocumentAsync(ClaimsPrincipal user, OlimpoUploadDocumentRequest request, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var path = await _storage.SaveAsync(request.File, "olimpo/documents", ct);
        var document = OlimpoDocument.Create(userId, request.Name, request.File.FileName, request.File.ContentType, request.File.Length, path, request.Category, request.Tags);
        await _repository.AddDocumentAsync(document, ct);

        return Result<OlimpoDocumentResponse>.Ok(new OlimpoDocumentResponse
        {
            Id = document.Id.ToString(),
            Name = document.Name,
            FileName = document.FileName,
            Category = document.Category,
            Size = document.Size,
            Url = $"/api/olimpo/documents/{document.Id}/download",
            CreatedAt = document.CreatedAt
        });
    }

    public async Task<Result<bool>> DeleteDocumentAsync(ClaimsPrincipal user, string id, CancellationToken ct)
    {
        var userId = GetUserId(user);
        var document = await _repository.GetDocumentByIdAsync(userId, Guid.Parse(id), ct);
        if (document is null)
            return Result<bool>.Fail("Documento nao encontrado");

        await _storage.DeleteAsync(document.StoragePath, ct);
        await _repository.DeleteDocumentAsync(document, ct);
        return Result<bool>.Ok(true);
    }
}