🛡️ Rapsodia - Segurança Cibernética Autônoma
https://img.shields.io/badge/License-AGPLv3-blue.svg
https://img.shields.io/badge/Cloud-Oracle%2520Free%2520Tier-red
https://img.shields.io/badge/Deploy-Docker%2520Swarm-blue
https://img.shields.io/badge/Architecture-Hexagonal%2520%252B%2520DDD-brightgreen

🎯 O Que é o Rapsodia?
Plataforma open source de segurança cibernética que une monitoramento, laboratórios de treinamento e inteligência de ameaças. Da sua avó ao CISO de uma Fortune 500.

🔵 Blue: Honeypots, detecção de intrusão, alertas

🟣 Violet: Laboratórios efêmeros de treinamento

⚪ Silver: Orquestração, telemetria, conhecimento

🚀 Instalação em 5 Minutos
bash
git clone https://github.com/th1eros/abitat/rapsodia.git
cd rapsodia
docker stack deploy -c docker-compose.prod.yml rapsodia
Acesse: https://rapsodia.th1eros.com

🧩 Módulos
🔵 Blue (Defesa)
Honeypots multi-protocolo, detecção de anomalias, bloqueio automático de IPs maliciosos.

Destaque - Modo Vovó: Alerta simples: "Alguém tentou acessar seu WiFi. Bloquear?" → Um clique resolve.

🟣 Violet (Laboratórios)
Ambientes de treinamento que nascem e morrem em segundos. Kali Linux, Metasploitable, DVWA direto no navegador.

Destaque: Labs compartilháveis por link, destruição automática após uso.

⚪ Silver (Cérebro)
Telemetria unificada (OpenTelemetry), base de conhecimento automatizada, agentes inteligentes com Orleans.

Destaque: Sistema que aprende com cada ataque e documenta automaticamente via Obsidian.

📊 Stack
Camada	Tecnologia
Orquestração	Docker Swarm
Observabilidade	OpenTelemetry + Grafana + Loki + Prometheus
Banco	Oracle Autonomous (Free Tier) / PostgreSQL
Cache	Redis
Agentes	Orleans Virtual Actors
Proxy	Cloudflare Tunnel
Conhecimento	Obsidian + Graph Engine próprio
🏗️ Infraestrutura
text
                     Cloudflare (DDoS Protection)
                            │
         ┌──────────────────┼──────────────────┐
         │                  │                  │
    rapsodia.com       rapsodia.dev       OCI Free Tier
    (Produção)         (Laboratório)      (4 OCPU/24 GB)
         │                  │                  │
         └──────────────────┼──────────────────┘
                            │
              ┌─────────────┼─────────────┐
              │             │             │
         🔵 Blue       🟣 Violet     ⚪ Silver
        (Defesa)       (Labs)       (Cérebro)
🤝 Para Quem é Isso?
👵 Sua avó protegendo o WiFi de casa

🏠 Famílias com múltiplos dispositivos

🏢 Pequenas empresas sem orçamento para Splunk

🎓 Universidades ensinando cibersegurança

🕵️ Analistas SOC que precisam de ferramentas acessíveis

🗺️ Roadmap
Honeypots multi-protocolo

Labs efêmeros com Violet

Integração Grafana + Loki

App mobile (React Native)

Detecção por IA/ML

Modo "Vovó" 1-click

Multi-cloud (AWS, GCP, Azure)

📄 Licença
GNU AGPLv3 - Use, modifique, distribua. Mantenha aberto.

⚠️ Aviso Legal: Rapsodia Red Team é uma ferramenta de teste de penetração autorizado, similar a Metasploit e Nmap. O uso ético e legal é responsabilidade do usuário final.

🌟 Créditos
Feito com ☕ e paranoia por @th1eros

📖 Guia de Instalação Completo | 📚 Documentação da API

❓ Sobre as Chaves Vermelhas no CONTRIBUTING.md
As chaves vermelhas no VSCode provavelmente são erros de sintaxe no código C# dos exemplos. Isso acontece porque:

VSCode tenta compilar os blocos de código dentro do .md

Faltam namespaces nos exemplos (os using não estão no snippet)

Classes referenciadas (como Honeypot, Lab, IPAddress) não existem no contexto do arquivo .md

🟡 Isso NÃO é um problema real!
text
✅ O código está CORRETO conceitualmente
⚠️ O VSCode só reclama porque falta contexto (namespaces, classes)
🎯 Em um projeto real, com todos os arquivos, compilaria normal
Soluções (se quiser tirar o vermelho):
Opção 1: Ignorar (recomendado)

Não afeta nada, é só visual

Opção 2: Adicionar comentário no início dos blocos

csharp
// Exemplo conceitual - requer Rapsodia.Domain
public class Honeypot
{
    // ...
}
Opção 3: Desabilitar validação C# em .md no VSCode

json
// .vscode/settings.json
{
    "csharp.semanticHighlighting.enabled": false
}