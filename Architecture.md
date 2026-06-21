# Arquitetura Rapsodia (Malebolge Engine)

## Princípios

- **Hexagonal:** Domínio purificado e 100% isolado de detalhes de infraestrutura.
- **DDD (Domain-Driven Design):** Linguagem ubíqua expressa em código nas quatro divisões do ecossistema.
- **DevSecOps:** Hardening de rede via Docker, imagens Jammy controladas e isolamento de portas.
- **Open Source (AGPLv3):** Extensível, auditável e desenhado para colaboração em segurança cibernética.

---

## Papel no Ecossistema

**Malebolge** é o codinome do ambiente/projeto de infraestrutura que orquestra a engine do **Rapsodia** (o backend do ecossistema **aBitat**). Ele atua como o motor autofágico de segurança: as capacidades ofensivas coletam inteligência para atualizar dinamicamente as defesas do sistema principal.

---

## Hexagonal Architecture (Ports & Adapters)
┌────────────────────────────────────────────────────────┐
                        │                     DOMÍNIO PURO                       │
                        │                  (Rapsodia.Domain)                     │
                        │                                                        │
                        │   • O que é um ataque?       • Como mitigar ameaças?   │
                        │   • Quando conter incidentes? • Como simular cenários? │
                        └──────────────────────────┬─────────────────────────────┘
                                                   │
                            ┌──────────────────────┴──────────────────────┐
                            ▼                                             ▼
                    [ Portas Primárias ]            [ Portas Secundárias ]
                   (Rapsodia.Presentation)         (Rapsodia.Infrastructure)
                   ┌───────────────────────────┐   ┌───────────────────────────┐
                   │ • REST API / SignalR Hubs │   │ • Oracle Autonomous DB    │
                   │ • HTMX / Web Frontends    │   │ • Redis Cache / Valkey    │
                   │ • CLI Automation Tools    │   │ • Microsoft Orleans Silos │
                   │ • Test Suites / QA-Argus  │   │ • OpenTelemetry / Grafana │
                   └───────────────────────────┘   └───────────────────────────┘

> **Garantia de Hardening:** O isolamento via acoplamento frouxo permite que os adaptadores de infraestrutura lidem com segredos e conexões seguras sem vazar vetores para o domínio corporativo.

---

## Domain-Driven Design (DDD)

### Linguagem Ubíqua

O código reflete a realidade tática das operações Blue Team e Red Team do ecossistema:

| Conceito | Realização | Módulo Associado | Significado Tático |
| :--- | :--- | :--- | :--- |
| Honeypot.Deceive() | Entidade | 🔵 Gerent | Enganar e capturar ações do atacante. |
| Incident.Contain() | Agregado | 🔵 Gerent | Conter a propagação de uma intrusão detectada. |
| Push.Attack() | Agregado | 🔴 Push | Disparar força ofensiva (scan/exploit) autorizada. |
| Lab.Provision() | Agregado | 🟣 Limbo | Criar ambientes efêmeros em containers (Kali, DVWA). |
| In_telektus.Process() | Serviço de Domínio | ⚪ In_telectus | Orquestração de agentes via atores virtuais Orleans. |

```csharp
// ❌ Código Acoplado / Genérico
var sql = "INSERT INTO alerts VALUES (...)";

// ✅ Alinhado à Linguagem Ubíqua e DDD
var alert = new SecurityAlert(
    severity: ThreatLevel.Critical,
    source: "Honeypot SSH",
    action: AlertAction.BlockImmediately
);
_securityContext.RaiseAlert(alert);

Estrutura de Camadas (Rapsodia.sln)
Rapsodia/
├── 🧠 Rapsodia.Domain/
│   ├── Entities/          # Honeypot.cs, Incident.cs, ThreatActor.cs
│   ├── ValueObjects/      # IPAddress.cs, ThreatLevel.cs, PortNumber.cs
│   ├── Services/          # ThreatAnalysisService.cs, ExploitOrchestrator.cs
│   └── Interfaces/        # IThreatRepository.cs, IMsfRpcClient.cs
│
├── 🔧 Rapsodia.Application/
│   ├── UseCases/          # DeployHoneypotUseCase.cs, ExecuteScanUseCase.cs
│   ├── DTOs/              # AttackPatternDto.cs, TelemetryPayload.cs
│   └── Interfaces/        # IOrleansOrchestrator.cs, ITelemetryService.cs
│
├── 🔌 Rapsodia.Infrastructure/
│   ├── Persistence/       # OracleDbContext.cs, RedisCacheService.cs
│   ├── Adapters/          # MetasploitRpcAdapter.cs, ObsidianGraphAdapter.cs
│   └── Telemetry/         # OpenTelemetrySetup.cs, GrafanaMetrics.cs
│
└── 🌐 Rapsodia.Presentation/
    ├── Controllers/       # AttackController.cs, LabController.cs
    ├── Hubs/              # SignalR Real-Time Incident Streamdv
    └── Middleware/        # RateLimitingMiddleware, DevSecOpsHardeningMiddleware

    Regra de Ouro: As dependências do projeto apontam estritamente para dentro. Domain possui dependência zero de bibliotecas externas de infraestrutura.
    
    Padrões Táticos Aggregate Roots C#
    
    public class Lab

{
    public LabId Id { get; }
    public LabStatus Status { get; private set; }
    private readonly List<Container> _containers = new();
    
    public void Provision(Template template)
    {
        if (Status != LabStatus.Ready)
            throw new DomainException("Laboratório não inicializado.");
            
        _containers.Add(Container.FromTemplate(template));
        Status = LabStatus.Running;
    }
}
Value Objects (Imutáveis e Validados)C#public class IPAddress : ValueObject
{
    public string Value { get; }
    
    public IPAddress(string ip)
    {
        if (!IsValid(ip))
            throw new DomainException($"IP Inválido: {ip}");
        Value = ip;
    }
    
    public bool IsPrivate() => Value.StartsWith("192.168.") || Value.StartsWith("10.");
}
Domain Events & Decoupled HandlersC#public class HoneypotBreachedEvent : IDomainEvent
{
    public HoneypotId HoneypotId { get; }
    public IPAddress AttackerIP { get; }
    public DateTime DetectedAt { get; }
}

public class AutoBlockOnBreachHandler : INotificationHandler<HoneypotBreachedEvent>
{
    public Task Handle(HoneypotBreachedEvent evt, CancellationToken ct)
    {
        _firewallAdapter.Block(evt.AttackerIP);
        _telemetry.TrackMitigation(evt.HoneypotId, evt.AttackerIP);
        return Task.CompletedTask;
    }
}

---
| Cenário de Evolução / Manutenção | Tempo Estimado | Impacto no Domínio Puro | Camada Afetada |
| :--- | :--- | :--- | :--- |
| Substituir Oracle por PostgreSQL | 2 horas | Zero | Infrastructure.Persistence |
| Migrar endpoints REST para HTMX | 4 horas | Zero | Presentation |
| Acoplar engine do Metasploit Framework | 1 dia | Zero (Apenas interface) | Infrastructure.Adapters |
| Escalar processamento de logs via Orleans | 3 dias | Zero | Infrastructure.Telemetry |
| Atualizar infraestrutura Nginx para Caddy | 2 horas | Zero | Docker / DevOps Only |
| Injetar Agentes Inteligentes no SOC | 3 dias | Reutiliza Entidades | Application / Domain |