# Rapsodia - Sistema Autofágico de Segurança Cibernética  

[![License](https://img.shields.io/badge/License-AGPLv3-blue.svg)](LICENSE)
[![OCI](https://img.shields.io/badge/Cloud-Oracle%20Free%20Tier-red)](https://www.oracle.com/cloud/free/)
[![Docker](https://img.shields.io/badge/Deploy-Docker%20Compose-blue)](https://docs.docker.com/compose/)

## 🎯 O Que é o Rapsodia?

Rapsodia é o Backend do Ecosistema de aBitat. 

- 🔵 **Gerent**: Honeypots, detecção de intrusão, alertas
- 🔴 **Push**: Testes de scan/penetração/pós penetração em ambientes autorizados/controlados
- 🟣 **Limbo**: Multiplicador dinâmico de laboratórios em Conteineres
- ⚪ **In_telectus**: Orquestração de agentes IA, telemetria, redes neurais de dados

**Missão:** Validar, quebrar e corrigir antes do deploy final.

---

## 🚀 Instalação em 5 Minutos

```bash
git clone https://github.com/ab1tat/rapsodia.git
cd rapsodia
git checkout develop
docker compose -p malebolge up -d
Acesso: https://rapsodia.th1eros.dev

🧩 Módulos
🔵 Gerent (Gerenciamento)
Honeypots multi-protocolo, detecção de anomalias, bloqueio automático de IPs maliciosos.

🔴 Push (Testes Autorizados)
Scanning, exploitation e post-exploitation para auditoria de segurança.

🟣 Limbo (Laboratórios)
Ambientes de treinamento efêmeros. Kali Linux, Metasploitable, DVWA direto no navegador.

⚪ In_telectus (Gerenciador de Dados)
Telemetria unificada (OpenTelemetry), base de conhecimento automatizada Obisidian, agentes inteligentes com Orleans.

📊 Stack
| Camada | Tecnologia |
| :--- | :--- |
| Orquestração | Docker Compose |
| Observabilidade | OpenTelemetry + Grafana + Loki + Prometheus |
| Banco | Oracle Autonomous (Free Tier) - rainbow_low |
| Cache | Redis |
| Agentes | Orleans Virtual Actors |
| Proxy | Cloudflare Tunnel |
| Conhecimento | Obsidian + Graph Engine próprio |

---
```
## 🏗️ Infraestrutura

```bash

Cloudflare Tunnel
                                │
                      th1eros.dev (Staging)
                                │
                   OCI Free Tier (ARM64/4 OCPU)
                                │
              ┌─────────────────┼─────────────────┐
              │                 │                 │
          🔵 Gerent         🔴 Push           🟣 Limbo
           (10001)           (10002)           (10003)
              │                 │                 │
              └─────────────────┼─────────────────┘
                                │
                      ⚪ In_telectus (10004)
                        (Orquestração)


Portas e Redes
| Serviço | Porta | Rede Docker |
| :--- | :--- | :--- |
| Blue | 10001 | malebolge_net |
| Red | 10002 | malebolge_net |
| Violet | 10003 | malebolge_net |
| Silver | 10004 | malebolge_net |
| Nginx Frontend | 8081 | malebolge_net |

Docker Images
| Serviço | Imagem | Tipo |
| :--- | :--- | :--- |
| Blue | blue:latest | jammy (não-chiseled) |
| Red | red:latest | jammy (não-chiseled) |
| Violet | violet:latest | jammy (não-chiseled) |
| Silver | silver:latest | jammy (não-chiseled) |

Nota: Malebolge usa imagens jammy padrão (não-chiseled) para facilitar debug e acesso ao shell durante desenvolvimento.

Banco de Dados
| Parâmetro | Valor |
| :--- | :--- |
| Provider | Oracle |
| Alias TNS | rainbow_low |
| Usuário | MALEBOLGE |
| Wallet | /app/wallet |

🏢 Pequenas e médias empresas buscando segurança acessível

🎓 Universidades e instituições de ensino em cibersegurança

🕵️ Analistas SOC que precisam de ferramentas open source

🧪 Desenvolvedores e pesquisadores de segurança ofensiva/defensiva

📚 Documentação

Arquitetura Detalhada

Como Contribuir

🗺️ Roadmap
| Milestone / Feature | Descrição Técnica |
| :--- | :--- |
| Honeypots multi-protocolo | Simulação ativa de serviços (SSH, FTP, HTTP, SMB). |
| Labs efêmeros com Violet | Provisionamento dinâmico de containers isolados. |
| Integração Grafana + Loki | Centralização de logs e métricas de ataques em tempo real. |
| App mobile PWA HTMX version | Interface móvel leve baseada em hipertexto, sem frameworks SPA pesados. |
| Detecção por IA/ML Multi Agentes Orleans | Análise comportamental distribuída usando atores virtuais .NET. |
| Multi-cloud (AWS, GCP, OCI, Azure) Documentação Dinâmica | Infraestrutura híbrida distribuída com geração de diagramas e specs viva. |

📄 Licença
GNU AGPLv3 - Use, modifique, distribua. Mantenha aberto.

⚠️ Aviso Legal: Rapsodia Push é uma ferramenta de teste de penetração autorizado, similar a Metasploit e Nmap. O uso ético e legal é responsabilidade do usuário final.