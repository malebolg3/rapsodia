
```bash
git clone [https://github.com/ab1tat/rapsodia.git](https://github.com/ab1tat/rapsodia.git)
cd rapsodia
git checkout develop
2. Ramificação (Branching)
Crie uma ramificação a partir da branch develop utilizando a convenção de prefixos:

Bash
git checkout -b feature/seu-recurso
# ou
git checkout -b fix/seu-bug
3. Commits Telegráficos
Os commits devem ser diretos, objetivos, escritos no presente e conter no máximo 60 caracteres. Não utilize descrições auto-referenciais como "Implementei X".

Plaintext
feat: honeypot SSH detecção por timing
fix: JWT issuer validation falso positivo
test: BruteForceSpecification com 5 casos
refactor: ThreatAnalyzer simplificado
4. Estrutura do Pull Request (PR)
Todo PR precisa fornecer o contexto exato do ciclo de ataque ou defesa modificado:

Plaintext
## O Quê
Adiciona detecção de brute force no honeypot SSH.
Markdown
# Contribuindo para Malebolge

## Antes de Começar

Leia o [ARCHITECTURE.md](ARCHITECTURE.md). É mandatório compreender o isolamento do Domínio Puro via Arquitetura Hexagonal (Ports & Adapters) e os limites táticos do Domain-Driven Design (DDD) adotados no ecossistema.

---

## Fluxo de Trabalho

### 1. Preparação do Ambiente
## Por Quê
Honeypot atual registra tentativas, mas não bloqueia IP automaticamente.

## Como Testei
- 50 tentativas SSH falhadas em 5min → IP bloqueado
- Tentativa legítima após 1h → IP desbloqueado

Closes #42
Padrões de Código
Matriz de Localização

| Intenção do Código | Camada Destino |
| :--- | :--- |
| Regra de negócio pura (ex: brute force) | Domain/Services/ |
| Fluxo ou caso de uso (ex: análise ameaça) | Application/UseCases/ |
| Integração com banco, caches ou APIs | Infrastructure/Adapters/ |
| Exposição de endpoints HTTP / Rotas | Presentation/Controllers/ |
| Testes unitários isolados de lógica | Tests.Unit/Domain/ |

Regras do Domínio Puro (Domain)

❌ NUNCA
C#
// Acoplamento com drivers de infraestrutura
using System.Data.SqlClient; 
public class Honeypot { ... }

// Persistência interna ou lógica em estruturas de dados
public class AlertDto 
{ 
    public void Save() { ... } 
} 

// Clientes de rede ou IO direto
public class ThreatClassifier 
{ 
    private readonly HttpClient _client; 
}


✅ SEMPRE
C#
// Expressar eventos baseados na linguagem ubíqua
public class HoneypotDeployed : IDomainEvent { }

// Garantir invariantes de negócio na instanciação
public class ThreatLevel : ValueObject { }

// Inversão de dependência via contratos abstratos
public interface IThreatRepository { }
public interface IBlockDecisionService { }
Orquestração na Camada de Aplicação (Application)

C#
// Fluxo limpo: recupera, executa a regra e despacha os efeitos colaterais
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

// Record estruturado e imutável para transferência de dados

public record DeployHoneypotRequest(string Name, int Port);

Adaptadores na Camada de Infraestrutura (Infrastructure)
C#
// Implementação dos contratos definidos no Domínio
public class SqlThreatRepository : IThreatRepository
{
    private readonly DbContext _ctx;
    public async Task Save(Threat threat) { ... }
}

// Adaptação de SDKs e ferramentas de observabilidade externas
public class GrafanaMetricsAdapter : IMetricsPublisher
{
    public async Task PublishMetric(Metric metric) { ... }
}
Magreza na Camada de Apresentação (Presentation)
C#
// Controlador desacoplado delegando a execução para os UseCases
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
Convenções de Nomenclatura

| Elemento | Padrão de Sufixo | Exemplo |
| :--- | :--- | :--- |
| Entidade | Singular (sem sufixo) | Honeypot, Incident |
| Value Object | Singular (sem sufixo) | ThreatLevel, IPAddress |
| Agregado | Raiz singular (sem sufixo) | Lab, ThreatActor |
| Repository | I{Entidade}Repository | IHoneypotRepository |
| Serviço Domínio | {Ação}Service | ThreatAnalysisService |
| Use Case | {Ação}{Recurso}UseCase | DeployHoneypotUseCase |
| DTO | {Recurso}{Operação}Request/Response | DeployHoneypotRequest |
| Evento | {Entidade}{Ação}Event | HoneypotDeployedEvent |
| Handler | {Ação}On{Evento}Handler | BlockIPOnBreachHandler |

Estratégia de Testes

Organização de Diretórios

Plaintext

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

Padrão AAA & Isolamento Tático

C#

[Fact]
public void Honeypot_WhenBreached_RaisesEvent()
{
    // Arrange
    var honeypot = Honeypot.Create("SSH", 22);
    var attacker = new IPAddress("192.168.1.100");
    
    // Act
    honeypot.RecordBreach(attacker);
    
    // Assert
    Assert.Single(honeypot.DomainEvents);
    Assert.IsType<HoneypotBreachedEvent>(honeypot.DomainEvents.First());
}

Restrição de Mocks

❌ PROIBIDO: Mockar entidades, especificações ou value objects do Domínio Puro. Use instâncias reais.

✅ PERMITIDO: Mockar interfaces de IO (IThreatRepository, IMsfRpcClient) localizadas nas bordas externas.

Hardening & Segurança

Proteção de Segredos Estáticos

Bash
# ❌ NUNCA realize o commit de arquivos .env contendo chaves explícitas
JWT_KEY=super_secret_key

# ✅ Injete em tempo de execução via variáveis do ambiente do contêiner
export JWT_KEY=$(openssl rand -base64 32)

# ✅ Utilize o armazenamento de segredos nativo do ecossistema .NET em desenvolvimento
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 32)"

Validações de Fronteira e Tipagem Forte

C#
// Validação estrutural rigorosa e imutável no construtor do tipo
public class IPAddress : ValueObject
{
    public IPAddress(string ip)
    {
        if (!IPAddress.TryParse(ip, out _))
            throw new DomainException("Formato de endereço IP inválido.");
        Value = ip;
    }
}

// Sanitização de fluxos na entrada do Caso de Uso
public async Task<HoneypotId> Execute(DeployHoneypotRequest req)
{
    if (string.IsNullOrWhiteSpace(req.Name))
        throw new ValidationException("O nome do recurso é obrigatório.");
}

Criptografia de Dados em Repouso

C#
// Proteção de dados confidenciais coletados (PII / Evidências de Ataque)
public class Breach : Entity
{
    public string AttackerIPEncrypted { get; private set; }
    
    public string DecryptIP(IAesGcmHelper crypto)
        => crypto.Decrypt(AttackerIPEncrypted);
}

Otimização & Performance

Prevenção de Gargalos em Queries (N+1)

C#
// ❌ Abordagem ineficiente que multiplica conexões ao banco de dados
var honeypots = _repo.GetAll();
foreach (var hp in honeypots)
    var breaches = _repo.GetBreaches(hp.Id);
    
// ✅ Abordagem otimizada via Eager Loading em uma única transação
var honeypots = _repo.GetAllWithBreaches();
Estratégia de Caching Volátil
C#
// Cacheamento rápido em memória/Redis para mitigar ataques de negação de serviço (DoS)
var key = $"brute_force:{ip}";
var attempts = _cache.GetOrSet(
    key,
    () => _repo.GetFailedAttempts(ip, last5Min),
    TimeSpan.FromMinutes(5)
);

Documentação Viva e Comentários

Código Autoexplicativo
C#
// ❌ Evite comentários que apenas repetem a ação do operador do código
// Incrementa contador
counter++;

// ✅ Estruture o método refletindo a semântica do domínio técnico
public void RecordFailedAttempt()
{
    _failedAttemptsInWindow++;
    if (_failedAttemptsInWindow > MaxAttempts)
        Breach();
}

Uso Restrito de Comentários (Apenas o "Por Quê")
C#
// ❌ Não explique o que a linha de código estrutural já expressa
if (!IsValidIP(ip)) throw;

// ✅ Explique decisões de design baseadas em RFCs, mitigações ou restrições de segurança

// RFC 3986: Alguns IPs privados realizam bypass do rate limit interno da infraestrutura

if (ip.IsPrivate()) return;

Checklist de Revisão (Pré-PR)

[ ] O código respeita o isolamento e as setas de dependência da arquitetura.

[ ] A camada Domain possui dependência zero de pacotes ou frameworks de terceiros.

[ ] Os testes unitários cobrem mais de 80% das modificações de lógica.

[ ] Foram removidas marcações do tipo // TODO ou // FIXME.

[ ] O histórico de commits está limpo, linear e em formato telegráfico.

[ ] Arquivos de configuração contendo credenciais não foram expostos ou commitados.

[ ] A ramificação de origem aponta estritamente para develop.

Trilhas de Contribuição (Roadmap)

| Complexidade | Escopo Sugerido |
| :--- | :--- |
| Fácil | Melhorias em documentação viva, testes de unidade do Domain, novos VOs. |
| Médio | Criação de UseCases (ex: ExportAuditLog) ou implementação de Adapters. |
| Difícil | Introdução de padrões avançados (Event Sourcing ou Agregados complexos). |