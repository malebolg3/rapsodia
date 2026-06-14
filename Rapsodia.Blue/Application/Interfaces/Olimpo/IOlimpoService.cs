using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Rapsodia.Blue.Application.DTOs.Olimpo;
using Rapsodia.Blue.Domain.Common;

namespace Rapsodia.Blue.Application.Interfaces.Olimpo;

public interface IOlimpoService
{
    Task<Result<OlimpoLoginResponse>> LoginAsync(OlimpoLoginRequest request, CancellationToken ct);
    Task<Result<OlimpoSecretResponse>> GetCiSecretAsync(ClaimsPrincipal user, string key, CancellationToken ct);
    Task<Result<OlimpoCiEnvResponse>> ExportCiEnvAsync(ClaimsPrincipal user, string environment, CancellationToken ct);
    Task<Result<IEnumerable<OlimpoSecretResponse>>> GetAllSecretsAsync(ClaimsPrincipal user, CancellationToken ct);
    Task<Result<OlimpoSecretResponse>> GetSecretByIdAsync(ClaimsPrincipal user, string id, CancellationToken ct);
    Task<Result<OlimpoSecretResponse>> CreateSecretAsync(ClaimsPrincipal user, OlimpoCreateSecretRequest request, CancellationToken ct);
    Task<Result<OlimpoSecretResponse>> UpdateSecretAsync(ClaimsPrincipal user, string id, OlimpoUpdateSecretRequest request, CancellationToken ct);
    Task<Result<bool>> DeleteSecretAsync(ClaimsPrincipal user, string id, CancellationToken ct);
    Task<Result<OlimpoTotpResponse>> GenerateTotpCodeAsync(ClaimsPrincipal user, string id, CancellationToken ct);
    Task<Result<OlimpoTotpResponse>> AddTotpAccountAsync(ClaimsPrincipal user, OlimpoAddTotpRequest request, CancellationToken ct);
    Task<Result<bool>> DeleteTotpAccountAsync(ClaimsPrincipal user, string id, CancellationToken ct);
    Task<Result<IEnumerable<OlimpoDocumentResponse>>> GetDocumentsAsync(ClaimsPrincipal user, CancellationToken ct);
    Task<Result<OlimpoDocumentResponse>> UploadDocumentAsync(ClaimsPrincipal user, OlimpoUploadDocumentRequest request, CancellationToken ct);
    Task<Result<bool>> DeleteDocumentAsync(ClaimsPrincipal user, string id, CancellationToken ct);
}