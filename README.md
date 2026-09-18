# VaultBank — Lab OWASP Top 10 2025

API de banco digital **intencionalmente vulnerável**, para treinamento de segurança.
As vulnerabilidades emergem das regras de negócio (contas, PIX, cartões, KYC) e cobrem
todo o **OWASP Top 10 2025**, com cenários encadeados de nível pentest.

> ⚠️ Uso exclusivo em ambiente de laboratório. Não expor em rede/produção.

## Setup
1. Runtime .NET (9 ou superior — o projeto tem target `net9.0`; com SDK mais novo use `DOTNET_ROLL_FORWARD=LatestMajor`).
2. Variáveis de ambiente (já em `Properties/launchSettings.json`):
   - `SqliteDatabase` → caminho do `Database/brokenaccesscontrol.db`
   - `JWTSecret`, `ASPNETCORE_ENVIRONMENT`
3. (Re)gerar o banco com dados de seed:
   ```bash
   sqlite3 Database/brokenaccesscontrol.db < Database/seed.sql
   ```
4. Rodar:
   ```bash
   export SqliteDatabase="./Database/brokenaccesscontrol.db"
   dotnet run --project brokenaccesscontrol.csproj
   ```
   Swagger em `http://localhost:5127/swagger`.

## Usuários de seed
| login | senha | role |
|---|---|---|
| admin | admin123 | admin |
| suporte | support123 | support |
| marcelo | marcelo | customer |
| ana.silva | Cliente@123 | customer |
| joao | senha123 | customer |
| teste | teste | customer |

## Mapa das vulnerabilidades
Guia de exploração completo por categoria (PoC + impacto + fix) em
[`docs/WRITEUPS.md`](docs/WRITEUPS.md). Plano/arquitetura em
[`OWASP2025-PLAN.md`](OWASP2025-PLAN.md).

| # | Categoria 2025 | Rota principal |
|---|---|---|
| A01 | Broken Access Control | `GET /api/accounts/{id}`, `PATCH /api/User/me` |
| A02 | Security Misconfiguration | `GET /api/status/debug`, CORS/JWT em `Program.cs` |
| A03 | Software Supply Chain Failures | `brokenaccesscontrol.csproj`, `.github/` |
| A04 | Cryptographic Failures | `GET /api/User`, `GET /api/cards/{id}` |
| A05 | Injection | `POST /api/authentication/loginsql`, `GET /api/statements/export/{doc}` |
| A06 | Insecure Design | `POST /api/transfers`, `POST /api/coupons/apply` |
| A07 | Authentication Failures | `POST /api/authentication/login` |
| A08 | Software/Data Integrity Failures | `POST /api/import/restore`, `POST /api/webhooks/pix-callback` |
| A09 | Logging & Alerting Failures | `logs/Access.log` (via `/api/statements/export`) |
| A10 | Mishandling of Exceptional Conditions | `GET /api/accounts/{id}` (fail-open), `POST /api/transfers` |

## Regras do laboratório
- Cada categoria tem um controle **presente porém quebrado** — descubra o bypass.
- Rotas base sem auth (login, registro, status) são propositais.
- A cadeia mestra em `docs/WRITEUPS.md` conecta as 10 categorias num único fluxo.
- BOA DIVERSÃO!!!!
