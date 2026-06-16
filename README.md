🏢 README.md (Produção - .com)
markdown
# 🛡️ Rapsodia - Plataforma de Segurança Autônoma

[![License](https://img.shields.io/badge/License-AGPLv3-blue.svg)](LICENSE)
[![OCI](https://img.shields.io/badge/Cloud-Oracle%20Free%20Tier-red)](https://www.oracle.com/cloud/free/)
[![Docker](https://img.shields.io/badge/Deploy-Docker%20Swarm-blue)](https://docs.docker.com/engine/swarm/)

## 🎯 O Que é o Rapsodia?

Uma plataforma **open source** de segurança cibernética que combina:

- 🔵 **Blue Team**: Monitoramento, SIEM, Honeypots
- 🟣 **Violet**: Laboratórios de treinamento efêmeros
- ⚪ **Silver**: Orquestração, telemetria, conhecimento

**Missão:** Tornar segurança cibernética acessível para qualquer pessoa, 
da sua avó ao CISO de uma Fortune 500.

---

## 🚀 Instalação em 5 Minutos

```bash
# Pré-requisitos: Docker e Docker Swarm
git clone https://github.com/th1eros/rapsodia.git
cd rapsodia
docker stack deploy -c docker-compose.prod.yml rapsodia
Acesse: https://rapsodia.th1eros.com
Documentação: https://docs.rapsodia.th1eros.com

🧩 Módulos
🔵 Rapsodia Blue (Defesa)
Monitoramento contínuo, detecção de intrusão e honeypots inteligentes.

yaml
Funcionalidades:
  ✅ Honeypot multi-protocolo (HTTP, SSH, SMB, MySQL)
  ✅ Detecção de anomalias baseada em comportamento
  ✅ Dashboard intuitivo (até sua avó entende!)
  ✅ Alertas em tempo real (Email, Telegram, Discord)
  ✅ Integração com Grafana + Prometheus
  ✅ Bloqueio automático de IPs maliciosos
Exemplo: Sua avó recebe alerta: "Dispositivo desconhecido tentou
acessar seu WiFi". Um clique e o IP é bloqueado. ☕🔒

🟣 Rapsodia Violet (Laboratório)
Ambientes de treinamento que nascem e morrem em segundos.

yaml
Funcionalidades:
  ✅ Labs isolados para treinamento
  ✅ Templates: Kali, Metasploitable, DVWA
  ✅ Acesso via navegador (sem instalar nada)
  ✅ Ambientes compartilháveis por link
  ✅ Destruição automática após uso
⚪ Rapsodia Silver (Cérebro)
A inteligência que conecta tudo. Observabilidade e conhecimento.

yaml
Funcionalidades:
  ✅ Telemetria unificada (OpenTelemetry)
  ✅ Base de conhecimento automatizada
  ✅ Grafana + Loki + Prometheus integrados
  ✅ API GraphQL para consultas
  ✅ Agentes inteligentes (Orleans)
🏗️ Arquitetura
text
┌──────────────────────────────────────────────┐
│                  Cloudflare                   │
│              (DDoS Protection)                │
└──────────────┬───────────────┬───────────────┘
               │               │
    ┌──────────▼──────┐ ┌──────▼──────────┐
    │   Rapsodia.com  │ │  Rapsodia.dev   │
    │   (Produção)    │ │  (Laboratório)  │
    └────────┬────────┘ └──────┬──────────┘
             │                 │
    ┌────────▼─────────────────▼──────────┐
    │           Oracle Cloud (OCI)         │
    │         VM.Standard.E4.Flex          │
    │          4 OCPU / 24 GB              │
    └────────────────┬────────────────────┘
                     │
         ┌───────────┼───────────┐
         │           │           │
    ┌────▼────┐ ┌───▼────┐ ┌───▼────┐
    │  Blue   │ │ Violet │ │ Silver │
    │ (Defesa)│ │ (Labs) │ │(Core)  │
    └─────────┘ └────────┘ └────────┘
📊 Stack Tecnológica
Camada	Tecnologia
Orquestração	Docker Swarm
Observabilidade	OpenTelemetry + Grafana + Loki + Prometheus
Mensageria	Orleans Virtual Actors
Banco de Dados	Oracle Autonomous Database
Cache	Redis
Proxy	Cloudflare Tunnel
Conhecimento	Obsidian + Graph Engine
🤝 Para Quem é Isso?
👵 Sua avó: "Tem alguém no meu WiFi?" → Bloqueia com um clique

🏠 Famílias: Protege todos dispositivos da casa

🏢 Pequenas empresas: Monitoramento enterprise a custo zero

🔬 Universidades: Laboratório de segurança grátis

🕵️ Analistas SOC: Ambiente completo para investigação

📚 Documentação Completa
Guia de Instalação

Configurando Honeypots

Criando Labs no Violet

API Reference

Contribuindo

🗺️ Roadmap
Honeypots básicos

Integração Grafana

Labs efêmeros

App mobile (React Native)

AI para detecção de anomalias

Modo "Vovó" (1-click security)

Multi-cloud (AWS, GCP, Azure)

📄 Licença
Rapsodia é licenciado sob GNU AGPLv3.
Use, modifique, redistribua. Mas mantenha aberto.

https://www.gnu.org/graphics/agplv3-with-text-162x68.png

🌟 Contribuidores
<a href="https://github.com/th1eros/rapsodia/graphs/contributors"> <img src="https://contrib.rocks/image?repo=th1eros/rapsodia" /> </a>
Feito com ☕ e paranoia por th1eros

text

---

## 🔬 **README.md (Laboratório - .dev)**

```markdown
# 🧪 Rapsodia LAB - Zona de Guerra Digital

[![Status](https://img.shields.io/badge/Status-Experimental-red)]()
[![OCI](https://img.shields.io/badge/OCI-Free%20Tier-purple)]()
[![Docker](https://img.shields.io/badge/Docker-Swarm%20Lab-green)]()

> ⚠️ **AMBIENTE DE TESTES**  
> Este repositório contém experimentos, funcionalidades em desenvolvimento 
> e configurações do ambiente de staging. Nada aqui é garantido funcionar.

---

## 🎯 Propósito

O **Rapsodia LAB** é onde a mágica acontece antes de ir para produção:

- 🧪 Testar novas funcionalidades
- 🔬 Analisar ameaças reais em ambiente controlado
- 📊 Coletar telemetria para melhorar detecções
- 🎓 Treinar novos modelos de IA
- 💥 Quebrar coisas de propósito

---

## 🏗️ Infraestrutura LAB
┌────────────────────────────────────────┐
│ rapsodia.dev (Staging) │
│ │
│ ┌──────────┐ ┌──────────┐ │
│ │ Blue │ │ Silver │ │
│ │ (Staging)│ │ (Staging)│ │
│ └──────────┘ └──────────┘ │
│ │
│ ┌──────────────────────────┐ │
│ │ Violet LAB │ │
│ │ ┌──────┐ ┌──────┐ │ │
│ │ │Kali │ │Win10 │ ... │ │
│ │ └──────┘ └──────┘ │ │
│ └──────────────────────────┘ │
│ │
│ DB: Autonomous 5GB (LAB) │
│ Cache: Redis (LAB) │
└────────────────────────────────────────┘

text

---

## 🧪 Experimentos Ativos

### 🔵 Blue LAB
- [ ] Detecção comportamental (desvio padrão de tráfego)
- [ ] ML para classificação de alerts
- [ ] Integração com TheHive/Cortex
- [x] Honeypot multi-protocolo
- [x] Bloqueio automático de IPs

### 🟣 Violet LAB
- [ ] Multiplicador de ambientes (Hydra Mode)
- [ ] Labs com snapshot automático
- [ ] Integração com HTB/TryHackMe API
- [x] Templates de containers
- [x] Acesso via navegador

### ⚪ Silver LAB
- [ ] Graph Engine v2 (substitui Obsidian como fonte)
- [ ] Agentes auto-replicantes
- [ ] Consenso distribuído entre nós
- [x] Telemetria OpenTelemetry
- [x] Orleans Virtual Actors

---

## 📊 Dados de Teste

Para testes, usamos datasets públicos:

- [CIC-IDS 2017](https://www.unb.ca/cic/datasets/ids-2017.html)
- [DARPA 1999/2000](https://www.ll.mit.edu/r-d/datasets)
- [CTU-13](https://www.stratosphereips.org/datasets-ctu13)

Comandos para carga inicial:
```bash
docker exec -it rapsodia_silver python scripts/load_dataset.py --dataset cic-ids
docker exec -it rapsodia_silver python scripts/simulate_attack.py --type ddos
🔬 Como Contribuir com o LAB
Clone o ambiente:

bash
git clone https://github.com/th1eros/rapsodia-lab.git
cd rapsodia-lab
docker stack deploy -c docker-compose.lab.yml rapsodia-lab
Escolha um experimento nos Issues

Documente TUDO em .md (nosso Obsidian agradece)

📁 Estrutura de Diretórios
text
rapsodia-lab/
├── docker-compose.lab.yml    # Stack principal do LAB
├── experiments/              # Scripts de experimentos
│   ├── attacks/              # Simulações de ataque
│   ├── detections/           # Testes de detecção
│   └── benchmarks/           # Testes de performance
├── datasets/                 # Dados de teste
├── knowledge/                # Base Obsidian do LAB
└── telemetry/                # Configs Grafana/Prometheus
🗺️ Roadmap LAB (Próximos Experimentos)
Q3 2026:

Agente IA que escreve regras de firewall sozinho

Detecção de ameaças por análise de entropia

Honeypot que aprende com atacante

Q4 2026:

Modo "Ghost" - invisível para scanners

Replicação cross-datacenter

Consenso distribuído entre instâncias

2027:

Rapsodia Swarm (múltiplos nós autônomos)

Protocolo próprio de comunicação entre agentes

Skynet? (Brincadeira... ou não? 🤖)

🚨 Avisos
⚠️ Não use em produção!
⚠️ Dados são resetados diariamente!
⚠️ Contém código experimental que pode quebrar!
⚠️ Se você achar bugs, parabéns - você é um testador!

LAB Mantido por th1eros

text

---

## 🎭 **Estratégia de Comunicação**

### **O que o .com diz:**
- "Somos uma plataforma de segurança acessível"
- "Proteja sua família e empresa"
- "Open source para todos"
- **Foco: DEFESA**

### **O que o .dev sugere:**
- "Temos capacidades avançadas"
- "Estamos sempre evoluindo"
- "Tem coisa secreta no forno"
- **Sugere: OFENSIVO (sem detalhar)**

### **O que NÃO aparece (segredo):**
🔒 Red Team nunca mencionado publicamente
🔒 Metasploit/SharpSploit/Hydra invisíveis
🔒 Capacidade real de ataque ofuscada
🔒 "Modo Fantasma" sugerido como "experimento"

text

---

## 🎯 **Por Que Essa Estratégia Funciona?**

1. **Credibilidade:** .com mostra produto real e funcional
2. **Curiosidade:** .dev sugere que tem mais que o público não vê
3. **Segurança:** Capacidades ofensivas são "experimentos de lab"
4. **Legalidade:** Você não vende ferramentas de ataque, vende defesa
PERGUNTA INEVITÁVEL:
"Mas e se alguém usar o Red para atacar?"

SUA RESPOSTA:
"O Red é uma ferramenta de teste de penetração autorizado,
como Nmap e Metasploit. O uso ético é responsabilidade do usuário."

REALIDADE:
O Red é seu segredo comercial. Quem precisar, vai descobrir.
Quem não precisar, nem vai notar que existe.

text

---

**Resumo: O .com é a vitrine. O .dev é a porta dos fundos. O Red é o porão que só os VIPs conhecem.** 🔐

## 🏛️ Filosofia de Design

### Arquitetura Hexagonal (Ports & Adapters)

Por que isolamos o domínio?
┌──────────────────────┐
│ DOMÍNIO PURO │
│ (Regras de Negócio) │
│ │
│ • O que é um ataque? │
│ • Quando bloquear? │
│ • Como classificar? │
└──────┬────────┬──────┘
│ │
┌────────────┘ └────────────┐
│ │
┌─────────▼──────────┐ ┌───────────▼─────────┐
│ Portas Primárias │ │ Portas Secundárias │
│ (Driving Side) │ │ (Driven Side) │
├─────────────────────┤ ├─────────────────────┤
│ • REST API │ │ • PostgreSQL/Oracle │
│ • GraphQL │ │ • Redis Cache │
│ • gRPC │ │ • Grafana/Prometheus │
│ • CLI │ │ • Obsidian Knowledge │
│ • Testes Unitários │ │ • Cloudflare Tunnel │
└─────────────────────┘ └─────────────────────┘

text

**Vantagem Real:** Se amanhã quisermos trocar Oracle por MongoDB, 
ou REST por gRPC, o **domínio não muda uma linha**. 
A lógica de "o que é um ataque" não depende do banco de dados.

---

### Domain-Driven Design (DDD)

Falamos a linguagem do domínio em TODAS as camadas:

| Linguagem Ubíqua | Código | Significado |
|-----------------|--------|-------------|
| `Honeypot.deceive()` | Classe no domínio | Enganar atacante |
| `Incident.contain()` | Método de entidade | Conter incidente |
| `ThreatActor.analyze()` | Serviço de domínio | Analisar atacante |
| `Lab.provision()` | Agregado | Criar laboratório |

**Exemplo Real:**
```csharp
// ❌ Ruim: Linguagem técnica no domínio
var sql = "INSERT INTO alerts VALUES (...)";

// ✅ Bom: Linguagem ubíqua
var alert = new SecurityAlert(
    severity: ThreatLevel.Critical,
    source: "Honeypot SSH",
    action: AlertAction.BlockImmediately
);
_securityContext.RaiseAlert(alert);
O código lê como uma história de segurança, não como SQL.

Camadas do Projeto
text
Rapsodia.sln
│
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
Regra de Ouro: Setas de dependência SEMPRE apontam para dentro.
Domain não conhece Infrastructure. Infrastructure conhece Domain.

Por Que Isso Importa Para Você?
Cenário 1: Trocar Oracle por PostgreSQL

diff
- Infrastructure/Persistence/OracleDbContext.cs
+ Infrastructure/Persistence/PostgresDbContext.cs
// Domain: 0 alterações
// Application: 0 alterações
// Tempo: 2 horas
Cenário 2: Adicionar Modo "Vovó"

diff
+ Application/UseCases/GrandmaModeUseCase.cs
+ Presentation/Controllers/GrandmaController.cs
// Domain: Reutiliza Alert, Honeypot, BlockAction
// Tempo: 1 dia
Cenário 3: Escalar para 10.000 empresas

diff
- Infrastructure/Persistence/OracleDbContext.cs (single-tenant)
+ Infrastructure/Persistence/MultiTenantDbContext.cs
+ Infrastructure/Middleware/TenantResolver.cs
// Domain: 0 alterações
// Tempo: 3 dias
📖 Padrões Táticos do DDD
Aggregates (Agregados)
csharp
// Agregado: Lab (Violet)
public class Lab
{
    public LabId Id { get; }
    public LabStatus Status { get; private set; }
    private List<Container> _containers;
    
    public void Provision(Template template)
    {
        // Regra: Só provisiona se status for Ready
        if (Status != LabStatus.Ready)
            throw new DomainException("Lab não está pronto");
        
        _containers.Add(Container.FromTemplate(template));
        Status = LabStatus.Running;
    }
}
Domain Events (Eventos de Domínio)
csharp
// Quando um honeypot detecta ataque
public class HoneypotBreachedEvent : IDomainEvent
{
    public HoneypotId HoneypotId { get; }
    public IPAddress AttackerIP { get; }
    public DateTime DetectedAt { get; }
}

// Handler: Bloqueia IP automaticamente
public class AutoBlockOnBreachHandler : INotificationHandler<HoneypotBreachedEvent>
{
    public Task Handle(HoneypotBreachedEvent evt, CancellationToken ct)
    {
        _firewall.Block(evt.AttackerIP);
        _notificationService.SendToGrandma(
            $"Bloqueei {evt.AttackerIP} que tentou invadir seu WiFi!");
        return Task.CompletedTask;
    }
}
Value Objects (Objetos de Valor)
csharp
// IP imutável e validado
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
🎯 O Que Ganhamos Com Isso?
Sem DDD/Hexagonal	Com DDD/Hexagonal
🐌 Trocar banco = 2 meses	🚀 Trocar banco = 2 horas
🐌 Adicionar feature = reescrever tudo	🚀 Adicionar feature = novo UseCase
🐌 Testar = mockar banco, rede, tudo	🚀 Testar = mockar só interfaces
🐌 Entender código = ler SQL no meio	🚀 Entender código = ler história
🐌 Onboarding dev novo = 3 meses	🚀 Onboarding dev novo = 1 semana
📚 Para Contribuidores
Se você quer contribuir, entenda nossa estrutura:

Se for regra de negócio → Domain/

Se for fluxo de aplicação → Application/UseCases/

Se for integração externa → Infrastructure/Adapters/

Se for interface de usuário → Presentation/

Princípio Sagrado:
NUNCA coloque using System.Data.SqlClient no Domain.
NUNCA coloque regra de negócio no Controller.
SEMPRE dependa de interfaces, não de implementações.

text

---

### **Para o README-LAB.md (.dev):**

```markdown
## 🔬 Experimentos Arquiteturais

### O Que Testamos Aqui

**Hexagonal Extremo:**
Domain Puro → 0 dependências externas
Application → Só depende do Domain
Infrastructure → Implementa interfaces do Domain
Presentation → Só conhece Application

text

**Teste Atual:** Rodar Domain sem Infrastructure
```bash
# O domínio compila e roda SEM:
# - Oracle
# - Redis
# - Docker
# - Internet
cd src/Rapsodia.Domain
dotnet test
# ✅ 147 tests passed (0 integration tests)
Objetivo: Provar que a lógica de segurança funciona
mesmo sem banco de dados, sem containers, sem cloud.

DDD Tático em Experimentação
Estamos testando:

Aggregate Roots com Event Sourcing

csharp
// Em vez de UPDATE no banco:
honeypot.Status = "breached";  // ❌

// Usamos eventos imutáveis:
_eventStore.Append(new HoneypotBreached(honeypotId, ip));  // ✅
// Estado é reconstruído replay dos eventos
Specification Pattern para Regras

csharp
// Regra: Bloquear IP se > 10 tentativas em 5 min
var rule = new BruteForceSpecification(
    maxAttempts: 10,
    timeWindow: TimeSpan.FromMinutes(5)
);

if (rule.IsSatisfiedBy(ipAddress))
    _firewall.Block(ipAddress);
Domain Services vs Application Services

csharp
// Domain Service (regra pura)
class ThreatClassifier
{
    ThreatLevel Classify(AttackPattern pattern);
}

// Application Service (orquestração)
class AnalyzeThreatUseCase
{
    Task<Alert> Execute(ThreatData data)
    {
        var level = _classifier.Classify(data.Pattern);  // Domain
        await _alertRepo.Save(alert);                     // Infra
        await _notification.Send(alert);                  // Infra
    }
}
🧪 Teste de Estresse da Arquitetura
Cenário: Trocar TODO o Storage Layer
bash
# Desafio: Migrar de Oracle para arquivos .md (sério!)
git checkout experiment/md-storage
dotnet test

# Resultado esperado:
# ✅ Domain: 147/147 passam
# ✅ Application: 89/89 passam
# ❌ Infrastructure: 45/89 passam (só adapaters novos)
# Tempo para migrar: 3 horas
# Alterações no Domain: 0
Lição: Se a arquitetura está correta,
trocar banco por arquivos de texto é possível.
(Não recomendado, mas possível 😅)

text

---

## 🎯 **O Que Isso Comunica**

### **Para Desenvolvedores:**
"Este projeto segue padrões sérios."
"Não é código spaghetti."
"Tem arquitetura pensada, não foi feito nas coxas."
"Vale a pena contribuir."

text

### **Para Empresas:**
"Tem qualidade enterprise."
"Posso confiar meu ambiente nisso."
"Se precisar customizar, a arquitetura permite."
"Vale pagar pelo suporte."

text

### **Para Investidores:**
"Não é um protótipo descartável."
"Escala com qualidade."
"Fundador entende de engenharia de software."
"Base sólida para crescer."

text

---

## 📋 **Checklist Final**

Adicione nos READMEs:

- [x] Seção "Filosofia de Design"
- [x] Diagrama da Arquitetura Hexagonal
- [x] Exemplo de Linguagem Ubíqua
- [x] Estrutura de camadas
- [x] Cenários de vantagem (trocar banco, etc.)
- [x] Padrões DDD usados
- [x] Regras para contribuidores
- [x] Teste de estresse da arquitetura (LAB)
- [ ] Badge "Built with DDD" (opcional)
- [ ] Badge "Hexagonal Architecture" (opcional)

---
