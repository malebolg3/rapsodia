 Rapsodia - Plataforma de Segurança Cibernética Autônoma

[![License](https://img.shields.io/badge/License-AGPLv3-blue.svg)](LICENSE)
[![OCI](https://img.shields.io/badge/Cloud-Oracle%20Free%20Tier-red)](https://www.oracle.com/cloud/free/)
[![Docker](https://img.shields.io/badge/Deploy-Docker%20Swarm-blue)](https://docs.docker.com/engine/swarm/)

## 🎯 O Que é o Rapsodia?

Plataforma **open source** de segurança cibernética que une monitoramento, laboratórios de treinamento e inteligência de ameaças.

- 🔵 **Blue**: Honeypots, detecção de intrusão, alertas
- 🔴 **Red**: Testes de penetração autorizados
- 🟣 **Violet**: Laboratórios efêmeros de treinamento
- ⚪ **Silver**: Orquestração, telemetria, conhecimento

**Missão:** Tornar segurança cibernética acessível para qualquer pessoa, da sua avó ao CISO de uma Fortune 500.

---

## 🚀 Instalação em 5 Minutos

```bash
git clone https://github.com/ab1tat/rapsodia.git
cd rapsodia
docker compose up -d
Staging: https://violet.th1eros.dev/swagger
Produção: https://violet.th1eros.com/swagger

🧩 Módulos
🔵 Blue (Defesa)
Honeypots multi-protocolo, detecção de anomalias, bloqueio automático de IPs maliciosos.

🔴 Red (Testes Autorizados)
Scanning, exploitation e post-exploitation para auditoria de segurança.

🟣 Violet (Laboratórios)
Ambientes de treinamento que nascem e morrem em segundos. Kali Linux, Metasploitable, DVWA direto no navegador.

⚪ Silver (Cérebro)
Telemetria unificada (OpenTelemetry), base de conhecimento automatizada, agentes inteligentes com Orleans.

📊 Stack
Camada	Tecnologia
Orquestração	Docker Compose
Observabilidade	OpenTelemetry + Grafana + Loki + Prometheus
Banco	Oracle Autonomous (Free Tier) / PostgreSQL
Cache	Redis
Agentes	Orleans Virtual Actors
Proxy	Cloudflare Tunnel
Conhecimento	Obsidian + Graph Engine próprio
🏗️ Infraestrutura
text
                     Cloudflare Tunnel
                            │
         ┌──────────────────┼──────────────────┐
         │                  │                  │
    th1eros.dev         th1eros.com         OCI Free Tier
    (Staging)           (Produção)          (ARM64/4 OCPU)
         │                  │                  │
         └──────────────────┼──────────────────┘
                            │
              ┌─────────────┼─────────────┐
              │             │             │
         🔵 Blue       🔴 Red       🟣 Violet
        (Defesa)      (Ataque)      (Labs)
                            │
                      ⚪ Silver
                    (Orquestração)
🤝 Para Quem é Isso?
👵 Sua avó protegendo o WiFi de casa

🏢 Pequenas empresas sem orçamento para Splunk

🎓 Universidades ensinando cibersegurança

🕵️ Analistas SOC que precisam de ferramentas acessíveis

📚 Documentação
Arquitetura Detalhada

Como Contribuir

Guia de Instalação Completo

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

