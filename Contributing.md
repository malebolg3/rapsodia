# Contribuindo para Rapsodia

## Antes de Começar

Leia [ARCHITECTURE.md](ARCHITECTURE.md). Entenda DDD e Hexagonal.

## Fluxo de Trabalho

1. **Fork + Clone**
   ```bash
   git clone https://github.com/ab1tat/rapsodia.git
   cd rapsodia
   ```

2. **Branch**
   ```bash
   git checkout -b feature/seu-recurso
   # ou
   git checkout -b fix/seu-bug
   ```

3. **Commit telegráfico**
   ```
   feat: honeypot SSH detecção por timing
   fix: JWT issuer validation falso positivo
   test: BruteForceSpecification com 5 casos
   refactor: ThreatAnalyzer simplificado
   ```
   - Sem "Implementei X"
   - Máximo 60 caracteres
   - Use prefixo: feat/fix/test/refactor/docs

4. **PR com contexto**
   ```
   ## O Quê
   Adiciona detecção de brute force no honeypot SSH.
   
   ## Por Quê
   Honeypot atual registra tentativas, mas não bloqueia IP automaticamente.
   
   ## Como Testei
   - 50 tentativas SSH falhadas em 5min → IP bloqueado
   - Tentativa legítima após 1h → IP desbloqueado
   
   Closes #42
   ```

## Padrões de Código

### Localização Correta

```
Adicionar regra de negócio (brute force)?      → Domain/Services/
Criar fluxo de app (análise de ameaça)?        → Application/UseCases/
Integração com banco/API externa?              → Infrastructure/Adapters/
Endpoint HTTP novo?                             → Presentation/Controllers/
Testes do domínio?                             → Tests.Unit/Domain/
```

### Domain (Nunca)

```csharp
/* ❌ NUNCA: using System.Data.SqlClient
using System.Data.SqlClient;
public class Honeypot { ... }

 ❌ NUNCA: Logic em DTOs
public class AlertDto
{
    public void Save() { ... }  
}

 ❌ NUNCA: HttpClient direto
public class ThreatClassifier
{
    private readonly HttpClient _client;
}
```

### Domain (Sempre)

```csharp
/* ✅ SEMPRE: Linguagem ubíqua
public class HoneypotDeployed : IDomainEvent { }

 ✅ SEMPRE: Value Objects validados
public class ThreatLevel : ValueObject { }

 ✅ SEMPRE: Interfaces para inversão
public interface IThreatRepository { }
public interface IBlockDecisionService { }
```

### Application (Sempre)

```csharp
/* ✅ Orquestração clara
public class DeployHoneypotUseCase
{
    public async Task<HoneypotId> Execute(DeployHoneypotRequest req)
    {
        var honeypot = Honeypot.Create(req.Name);
        await _repo.Save(honeypot);
        await _eventBus.Publish(honeypot.DomainEvents);
        return honeypot.Id;
    }
}

 ✅ DTOs desacoplados do Domain
public record DeployHoneypotRequest(string Name, int Port);
```

### Infrastructure (Sempre)

```csharp
/* ✅ Implementa interfaces do Domain
public class SqlThreatRepository : IThreatRepository
{
    private readonly DbContext _ctx;
    public async Task Save(Threat threat) { ... }
}

 ✅ Adapters para serviços externos
public class GrafanaMetricsAdapter : IMetricsPublisher
{
    public async Task PublishMetric(Metric metric) { ... }
}
```

### Presentation (Sempre)

```csharp
/* ✅ Controller slim, delega para UseCases
[ApiController]
[Route("api/[controller]")]
public class HoneypotController
{
    public async Task<IActionResult> Deploy([FromBody] DeployHoneypotRequest req)
    {
        var useCase = _mediator.Send(req);
        return Ok(new { id = useCase.Id });
    }
}
```

## Nomes e Convenções

| Elemento | Padrão | Exemplo |
|----------|--------|---------|
| Entidade | Singular | `Honeypot`, `Incident` |
| Value Object | Singular | `ThreatLevel`, `IPAddress` |
| Agregado | Singular | `Lab`, `ThreatActor` |
| Repository | `I{Entidade}Repository` | `IHoneypotRepository` |
| Serviço Domínio | `{Ação}Service` | `ThreatAnalysisService` |
| Use Case | `{Ação}{Recurso}UseCase` | `DeployHoneypotUseCase` |
| DTO | `{Recurso}{Operação}Request/Response` | `DeployHoneypotRequest` |
| Event | `{Entidade}{Ação}Event` | `HoneypotDeployedEvent` |
| Handler | `{Ação}On{Event}Handler` | `BlockIPOnBreachHandler` |

## Testes

### Estrutura

```
Tests.Unit/
├── Domain/
│   ├── HoneypotTests.cs
│   ├── ThreatLevelTests.cs
│   └── BruteForceSpecificationTests.cs
├── Application/
│   ├── DeployHoneypotUseCaseTests.cs
│   └── AnalyzeThreatUseCaseTests.cs
└── Infrastructure/
    └── SqlThreatRepositoryTests.cs
```

### Padrão AAA (Arrange, Act, Assert)

/*```csharp
[Fact]
public void Honeypot_WhenBreached_RaisesEvent()
{
    Arrange
    var honeypot = Honeypot.Create("SSH", 22);
    var attacker = new IPAddress("192.168.1.100");
    
    Act
    honeypot.RecordBreach(attacker);
    
    Assert
    Assert.Single(honeypot.DomainEvents);
    Assert.IsType<HoneypotBreachedEvent>(honeypot.DomainEvents.First());
}
```

### Mocks (Apenas Infra)

```csharp
// ❌ NÃO mocka entidades do Domain
var mockHoneypot = new Mock<Honeypot>();  // ❌

// ✅ SÓ mocka interfaces (contracts)
var mockRepo = new Mock<IHoneypotRepository>();
mockRepo.Setup(x => x.GetById(It.IsAny<HoneypotId>()))
        .ReturnsAsync(honeypot);
```

## Segurança

### Segredos

/*```bash
/* # ❌ NUNCA em .env commitado
JWT_KEY=super_secret_key

# ✅ SEMPRE em variáveis de ambiente
export JWT_KEY=$(openssl rand -base64 32)

# ✅ Local Secrets (dev)
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 32)"
```

### Validação de Input

```csharp
/* ✅ Value Object valida no construtor
public class IPAddress : ValueObject
{
    public IPAddress(string ip)
    {
        if (!IPAddress.TryParse(ip, out _))
            throw new DomainException("IP inválido");
        Value = ip;
    }
}

/* ✅ Use Case valida request
public async Task<HoneypotId> Execute(DeployHoneypotRequest req)
{
    if (string.IsNullOrWhiteSpace(req.Name))
        throw new ValidationException("Nome obrigatório");
}
```

### Criptografia de Campos Sensíveis

```csharp
/* ✅ Honeypot com IP atacante criptografado
public class Breach : Entity
{
    public string AttackerIPEncrypted { get; private set; }
    
    public string DecryptIP(IAesGcmHelper crypto)
        => crypto.Decrypt(AttackerIPEncrypted);
}
```

## Performance

### Queries

```csharp
/* ❌ N+1 queries
var honeypots = _repo.GetAll();
foreach (var hp in honeypots)
    var breaches = _repo.GetBreaches(hp.Id);
    
 ✅ Eager load
var honeypots = _repo.GetAllWithBreaches();
```

### Caching

```csharp
/* ✅ Cache regra de brute force
var key = $"brute_force:{ip}";
var attempts = _cache.GetOrSet(
    key,
    () => _repo.GetFailedAttempts(ip, last5Min),
    TimeSpan.FromMinutes(5)
);
```

## Documentação

### Código Auto-Explicativo

```csharp
/* ❌ Comentário desnecessário
 Incrementa contador
counter++;

 ✅ Código fala sozinho
public void RecordFailedAttempt()
{
    _failedAttemptsInWindow++;
    if (_failedAttemptsInWindow > MaxAttempts)
        Breach();
}
```

### Comentários (Raros)

```csharp
/* Apenas para "Por Quê", não "O Quê"

 ❌
 Valida IP
if (!IsValidIP(ip)) throw;

 ✅
 RFC 3986: Alguns IPs privados bypass rate limit
if (ip.IsPrivate()) return;
```

## Checklist Antes do PR

- [ ] Código segue padrões DDD/Hexagonal
- [ ] Domain sem dependências externas
- [ ] Tests com 80%+ cobertura
- [ ] Sem `// TODO` ou `// FIXME`
- [ ] Commits telegráficos
- [ ] Segredos NOT commitados
- [ ] Documentação atualizada

## Roadmap para Contribuidores

**Fácil (bom pra começar):**
- [ ] Documentação melhorada
- [ ] Testes do Domain
- [ ] Novo Value Object

**Médio:**
- [ ] Use Case novo (ex: ExportAuditLogUseCase)
- [ ] Adapter para serviço externo

**Difícil:**
- [ ] Novo padrão tático de DDD
- [ ] Event Sourcing em algum Agregado

---

Dúvidas? Abra issue com tag `question`.
Ideias? Abra issue com tag `enhancement`.