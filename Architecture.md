# Arquitetura Rapsodia

## Princípios

- **Hexagonal:** Domínio isolado de infraestrutura
- **DDD:** Linguagem ubíqua em todas as camadas
- **DevSecOps:** Segredos protegidos, hardening por padrão

## Hexagonal Architecture (Ports & Adapters)

```
┌──────────────────────┐
│  DOMÍNIO PURO        │
│  (Regras de Negócio) │
│                      │
│  • O que é ataque?   │
│  • Quando bloquear?  │
│  • Como classificar? │
└──────┬────────┬──────┘
       │        │
   ┌───▼────┐  ┌───▼──────────┐
   │ Portas │  │ Portas       │
   │Primárias│ │Secundárias   │
   ├────────┤  ├──────────────┤
   │REST API│  │PostgreSQL    │
   │GraphQL │  │Oracle        │
   │gRPC    │  │Redis         │
   │CLI     │  │Grafana       │
   │Tests   │  │Obsidian      │
   └────────┘  └──────────────┘
```

**Vantagem:** Trocar Oracle por PostgreSQL = 2 horas, zero mudanças no domínio.

## Domain-Driven Design (DDD)

### Linguagem Ubíqua

Falamos a linguagem do domínio em TODA camada de código:

| Conceito | Realização | Significado |
|----------|-----------|-------------|
| `Honeypot.deceive()` | Entidade | Enganar atacante |
| `Incident.contain()` | Agregado | Conter incidente |
| `ThreatActor.analyze()` | Serviço Domínio | Analisar ameaça |
| `Lab.provision()` | Agregado | Criar laboratório |

```csharp
// ❌ Ruim: Linguagem técnica
var sql = "INSERT INTO alerts VALUES (...)";

// ✅ Bom: Linguagem ubíqua
var alert = new SecurityAlert(
    severity: ThreatLevel.Critical,
    source: "Honeypot SSH",
    action: AlertAction.BlockImmediately
);
_securityContext.RaiseAlert(alert);
```

### Camadas do Projeto

```
Rapsodia/
├── 🧠 Domain (Rapsodia.Domain)
│   ├── Entities/        # Honeypot, Incident, ThreatActor
│   ├── ValueObjects/    # IPAddress, ThreatLevel, AlertSeverity
│   ├── Services/        # ThreatAnalysisService, BlockDecisionService
│   └── Interfaces/      # IThreatRepository, IAlertDispatcher
│
├── 🔧 Application (Rapsodia.Application)
│   ├── UseCases/        # DeployHoneypotUseCase, AnalyzeThreatUseCase
│   ├── DTOs/            # HoneypotDto, AlertDto
│   └── Interfaces/      # IAuthService, ITelemetryService
│
├── 🔌 Infrastructure (Rapsodia.Infrastructure)
│   ├── Persistence/     # OracleDbContext, RedisCache
│   ├── Adapters/        # GrafanaAdapter, ObsidianAdapter
│   └── Services/        # EmailNotificationService
│
└── 🌐 Presentation (Rapsodia.Presentation)
    ├── Controllers/     # HoneypotController, LabController
    ├── Hubs/            # SignalR para real-time
    └── Middleware/      # RateLimit, TenantResolver
```

**Regra de Ouro:** Setas de dependência SEMPRE apontam para dentro.
- Domain ≠ conhece Infrastructure
- Infrastructure = conhece Domain
- Presentation = conhece Application
- Application = conhece Domain

### Padrões Táticos

#### Aggregate Roots
```csharp
public class Lab
{
    public LabId Id { get; }
    public LabStatus Status { get; private set; }
    private List<Container> _containers;
    
    public void Provision(Template template)
    {
        if (Status != LabStatus.Ready)
            throw new DomainException("Lab não pronto");
        _containers.Add(Container.FromTemplate(template));
        Status = LabStatus.Running;
    }
}
```

#### Value Objects (Imutáveis e Validados)
```csharp
public class IPAddress : ValueObject
{
    public string Value { get; }
    
    public IPAddress(string ip)
    {
        if (!IsValid(ip))
            throw new DomainException($"IP inválido: {ip}");
        Value = ip;
    }
    
    public bool IsPrivate() => Value.StartsWith("192.168.") || 
                                Value.StartsWith("10.");
}
```

#### Domain Events
```csharp
public class HoneypotBreachedEvent : IDomainEvent
{
    public HoneypotId HoneypotId { get; }
    public IPAddress AttackerIP { get; }
    public DateTime DetectedAt { get; }
}

public class AutoBlockOnBreachHandler : INotificationHandler<HoneypotBreachedEvent>
{
    public Task Handle(HoneypotBreachedEvent evt, CancellationToken ct)
    {
        _firewall.Block(evt.AttackerIP);
        _notificationService.Send($"IP {evt.AttackerIP} bloqueado");
        return Task.CompletedTask;
    }
}
```

#### Specification Pattern (Regras)
```csharp
var rule = new BruteForceSpecification(
    maxAttempts: 10,
    timeWindow: TimeSpan.FromMinutes(5)
);

if (rule.IsSatisfiedBy(ipAddress))
    _firewall.Block(ipAddress);
```

#### Domain vs Application Services
```csharp
// Domain Service (lógica pura, sem I/O)
public class ThreatClassifier
{
    public ThreatLevel Classify(AttackPattern pattern) { ... }
}

// Application Service (orquestra Domain + Infra)
public class AnalyzeThreatUseCase
{
    public async Task<Alert> Execute(ThreatData data)
    {
        var level = _classifier.Classify(data.Pattern);  // Domain
        await _alertRepo.Save(alert);                     // Infra
        await _notification.Send(alert);                  // Infra
    }
}
```

## Vantagens Práticas

| Cenário | Tempo | Mudanças no Domain |
|---------|-------|--------------------|
| Trocar Oracle → PostgreSQL | 2h | 0 |
| Trocar REST → gRPC | 4h | 0 |
| Adicionar Modo "Vovó" | 1d | Reutiliza Entities |
| Escalar para 10k empresas (multi-tenant) | 3d | 0 |

## Teste de Estresse

```bash
# Desafio: Trocar TODO o Storage Layer
git checkout experiment/md-storage
dotnet test

# Esperado:
# ✅ Domain:         147/147 ✓
# ✅ Application:     89/89  ✓
# ⚠️  Infrastructure:  45/89  (só adapters novos)
# Tempo: 3 horas
# Alterações Domain: 0
```

Isso prova: arquitetura correta permite trocar banco por arquivos (não recomendado, mas possível).

## Posicionamento Técnico

Essa arquitetura comunica:

- **Para devs:** "Código sério, não spaghetti"
- **Para empresas:** "Qualidade enterprise"
- **Para investidores:** "Fundação escalável"