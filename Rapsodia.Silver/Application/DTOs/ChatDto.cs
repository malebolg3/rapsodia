using System.ComponentModel.DataAnnotations;

namespace Rapsodia.Silver.Application.DTOs;

public record ChatRequest(
    [property: Required]
    Guid ConversationId,

    [property: Required, StringLength(8000, MinimumLength = 1)]
    string Message
);

public record ChatResponse(
    string Content,
    double ContextEntropy,
    RiskAssessment Risk,
    List<string> RecommendedTools,
    DateTime Timestamp
);

public record RiskAssessment(
    double ShannonEntropy,
    double RiskScore,
    string SeverityLevel,
    List<string> RiskNarrative,
    List<ChainedVulnerability> VulnerabilityChains
);

public record ChainedVulnerability(string TriggerCve, string ConsequenceCve, double AmplificationFactor, string ChainType);

public record DiagnosticQuestion(string Question, List<string> SuggestedAnswers, int Priority);

public record RemediationCommand(int Priority, string Step, string Command, string Tool, string Verification, string OS);

public record ConversationSummary(
    Guid ConversationId,
    Guid UserId,
    int MessageCount,
    double MaxEntropyReached,
    double FinalRiskScore,
    List<string> AssetsAnalyzed,
    List<string> VulnerabilitiesFound,
    DateTime CreatedAt,
    DateTime UpdatedAt
);