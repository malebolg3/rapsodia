using Rapsodia.DTO.Response;
using Rapsodia.DTO.TelemetryDTO;
using Rapsodia.Models.Entity;
using Rapsodia.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Rapsodia.Services.Telemetries
{
    public class TelemetryService : ITelemetryService
    {
        private readonly AppDbContext _context;
        private readonly string _agentApiKey;

        public TelemetryService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _agentApiKey = configuration["Security:AgentApiKey"] ?? configuration["AGENT_APIKEY"] ?? "";
            Console.WriteLine($"🔍 [AUTH] Chave esperada: '{_agentApiKey}'");
        }

        public async Task<ResponseModel<TelemetryResponseDTO>> CreateTelemetry(TelemetryCreateDTO dto, string? providedKey)
        {
            if (providedKey?.Trim() != _agentApiKey.Trim())
                return new ResponseModel<TelemetryResponseDTO> { Status = false, Mensagem = "Acesso negado: Agente não autorizado." };

            if (dto.EntropyValue < 0 || dto.EntropyValue > 10)
                return new ResponseModel<TelemetryResponseDTO> { Status = false, Mensagem = "Valor de entropia inválido." };

            try
            {
                var entity = new Telemetry
                {
                    AgentId = dto.AgentId,
                    SessionId = dto.SessionId,
                    TargetFilePath = dto.TargetFilePath,
                    EntropyValue = dto.EntropyValue,
                    AnalysisTimestamp = dto.AnalysisTimestamp,
                    ProcessingTimeMs = dto.ProcessingTimeMs,
                    RiskLevel = dto.RiskLevel.Trim().ToUpper()
                };

                _context.Telemetries.Add(entity);
                await _context.SaveChangesAsync();

                return new ResponseModel<TelemetryResponseDTO>
                {
                    Status = true,
                    Dados = MapToResponse(entity)
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<TelemetryResponseDTO> { Status = false, Mensagem = $"Erro ao persistir: {ex.Message}" };
            }
        }

        public async Task<ResponseModel<List<TelemetryResponseDTO>>> GetLatestTelemetries(int count)
        {
            var data = await _context.Telemetries
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync();

            return new ResponseModel<List<TelemetryResponseDTO>>
            {
                Status = true,
                Dados = data.Select(MapToResponse).ToList()
            };
        }

        public async Task<ResponseModel<TelemetryResponseDTO>> GetTelemetryById(int id)
        {
            var t = await _context.Telemetries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return new ResponseModel<TelemetryResponseDTO> { Status = false, Mensagem = "Não encontrado" };
            return new ResponseModel<TelemetryResponseDTO> { Status = true, Dados = MapToResponse(t) };
        }

        public async Task<ResponseModel<bool>> DeleteTelemetry(int id)
        {
            var t = await _context.Telemetries.FindAsync(id);
            if (t == null) return new ResponseModel<bool> { Status = false, Dados = false };
            _context.Telemetries.Remove(t);
            await _context.SaveChangesAsync();
            return new ResponseModel<bool> { Status = true, Dados = true };
        }


        public async Task<ResponseModel<TelemetryStatsDTO>> GetStats()
        {
            try
            {
                var query = _context.Telemetries.AsNoTracking();
                var data = await _context.Telemetries.AsNoTracking().ToListAsync();
                if (!data.Any()) return new ResponseModel<TelemetryStatsDTO> { Status = true, Dados = new TelemetryStatsDTO() };

                var stats = new TelemetryStatsDTO
                {
                    TotalRecords = data.Count,
                    CriticalCount = data.Count(t => t.RiskLevel == "CRITICAL"),
                    HighCount = data.Count(t => t.RiskLevel == "HIGH"),
                    MediumCount = data.Count(t => t.RiskLevel == "MEDIUM"),
                    LowCount = data.Count(t => t.RiskLevel == "LOW"),
                    AverageEntropy = data.Average(t => t.EntropyValue),
                    MaxEntropy = data.Max(t => t.EntropyValue),
                    MinEntropy = data.Min(t => t.EntropyValue),
                    AverageProcessingMs = (long)data.Average(t => t.ProcessingTimeMs),
                    UniqueAgents = data.Select(t => t.AgentId).Distinct().Count(),
                    UniqueSessions = data.Select(t => t.SessionId).Distinct().Count(),
                    LastEventAt = data.Max(t => t.CreatedAt)
                };

                return new ResponseModel<TelemetryStatsDTO> { Status = true, Dados = stats };
            }
            catch (Exception ex)
            {
                return new ResponseModel<TelemetryStatsDTO> { Status = false, Mensagem = ex.Message };
            }
        }

        public async Task<ResponseModel<List<TelemetryResponseDTO>>> GetSince(DateTime? since)
        {
            var query = _context.Telemetries.AsNoTracking();
            if (since.HasValue) query = query.Where(x => x.AnalysisTimestamp > since.Value);
            var list = await query.ToListAsync();
            return new ResponseModel<List<TelemetryResponseDTO>> { Status = true, Dados = list.Select(MapToResponse).ToList() };
        }

        private TelemetryResponseDTO MapToResponse(Telemetry t) => new TelemetryResponseDTO
        {
            Id = t.Id,
            AgentId = t.AgentId,
            SessionId = t.SessionId,
            RiskLevel = t.RiskLevel,
            TargetFilePath = t.TargetFilePath, 
            EntropyValue = t.EntropyValue,
            AnalysisTimestamp = t.AnalysisTimestamp,
            ProcessingTimeMs = t.ProcessingTimeMs,
            CreatedAt = t.CreatedAt
        };
    }
}