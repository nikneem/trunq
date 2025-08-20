<div align="center">

# 🐟 Trunq
<strong>A modern, extensible, open‑source URL shortener powered by .NET 9 & .NET Aspire</strong>

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE) 
[![Built with .NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/) 
[![Status: Early Alpha](https://img.shields.io/badge/status-early%20alpha-orange)](#roadmap) 
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](#-contributing)

</div>

---

## 📌 Overview
Trunq is a service that accepts long URLs and returns short, shareable identifiers ("trunqs") that redirect to the original targets. It is designed for clarity, testability, and future extensibility (e.g., analytics, custom domains, rate limiting) without premature complexity.

The solution uses **.NET Aspire** for local multi‑service orchestration and follows a clean separation of responsibilities between:

| Service | Responsibility |
|---------|----------------|
| `HexMaster.Trunq.Api` | Public HTTP API surface (create/manage short links) |
| `HexMaster.Trunq.Resolver` | (Resolution layer) Redirect short codes to full targets |
| `Aspire.AppHost` | Local composition/orchestration (wiring services, telemetry defaults) |
| `ServiceDefaults` | Shared service configuration (logging, telemetry scaffolding, resilience) |

> Persistence, caching, analytics, and custom domain modules are intentionally deferred until requirements are explicit.

---

## ✨ Current Features
* Create short links via API (initial scaffolding)
* Structured architecture ready for persistence layer introduction
* Clear layering for future analytics & observability
* .NET 9 / C# 13 ready codebase

## 🛠 Planned / Roadmap (High‑Level)
| Phase | Item | Notes |
|-------|------|-------|
| 1 | URL validation & normalization | Enforce http/https, strip tracking params (optional) |
| 1 | Short code generation strategy | Base62 (no ambiguous chars), collision handling |
| 2 | Pluggable persistence | Start with lightweight (e.g., in‑memory / SQLite) behind abstraction |
| 2 | Resolution endpoint integration | End‑to‑end redirect path |
| 3 | Expiration support | Optional per‑link TTL |
| 3 | Basic metrics & tracing | OpenTelemetry spans: `shorturl.create`, `shorturl.resolve` |
| 4 | Custom slugs (user supplied) | Validation + reserved words list |
| 4 | Rate limiting / abuse prevention | Opt‑in policies |
| 5 | Click analytics | Aggregation + privacy mindful design |

*You can help shape this list—open a [discussion](https://github.com/nikneem/trunq/discussions) or [issue](#-reporting-issues).* 

---

## 🧱 Architecture Principles
* **Single responsibility services** – API vs resolver vs orchestration.
* **Abstraction before commitment** – Introduce persistence interfaces first.
* **Secure & strict input handling** – Only valid syntactic URLs; avoid open redirects.
* **Observability‑friendly** – Structured logs & readiness for OpenTelemetry.
* **Extensible design** – Small interfaces; pluggable code generation strategy.
* **Privacy aware** – Minimize logging of raw user URLs.

---

## 🗂 Directory Structure (Key Parts)
```
src/
	Aspire/
		HexMaster.Trunq.Aspire.AppHost/        # Orchestrates local services (Aspire)
		HexMaster.Trunq.Aspire.ServiceDefaults/# Shared service defaults (logging, etc.)
	Backend/
		HexMaster.Trunq.Api/                   # Public API surface (creation endpoints)
		HexMaster.Trunq.Resolver/              # Short code resolution logic (in progress)
		HexMaster.Trunq.Core/                  # Core contracts (CQRS abstractions, markers)
	Frontend/                                # Angular client (future UI)
```

---

## 🚀 Getting Started (Local Development)

### Prerequisites
* [.NET 9 SDK](https://dotnet.microsoft.com/) (preview if not GA)
* Node.js 20+ & npm (for the Angular frontend)
* Modern browser

### Clone
```powershell
git clone https://github.com/nikneem/trunq.git
cd trunq
```

### Restore & Build Backend
```powershell
dotnet restore .\src\Trunq.sln
dotnet build .\src\Trunq.sln -c Debug
```

### Run with Aspire AppHost
```powershell
dotnet run --project .\src\Aspire\HexMaster.Trunq.Aspire\HexMaster.Trunq.Aspire.AppHost\HexMaster.Trunq.Aspire.AppHost.csproj
```
This wires together the API (and, as it matures, the resolver) using Aspire's local orchestrator.

### Frontend (Optional / Early)
```powershell
cd .\src\Frontend
npm install
npm start  # or: npx ng serve --open
```

---

## 🧪 Testing (Coming Soon)
Initial unit tests will cover:
* Short code generation (length, alphabet, collision retries)
* URL validation & normalization
* API contract behaviors (201 Created + Location header)

> When tests are added you will run them with: `dotnet test src/Trunq.sln`.

---

## 📡 API Usage (Preview)
Example (subject to change as endpoints solidify):

`POST /shortlinks`
```jsonc
{
	"url": "https://example.com/some/very/long/path?with=query&tracking=123"
}
```
Response `201 Created`
```jsonc
{
	"code": "fA72kX9",
	"originalUrl": "https://example.com/some/very/long/path?with=query&tracking=123",
	"shortUrl": "https://localhost:5001/fA72kX9",
	"createdAt": "2025-08-20T12:34:56Z"
}
```
Follow the `shortUrl` (or GET `/fA72kX9`) to be redirected (302) to the stored original URL.

---

## 🔐 Security & Input Handling
* Only `http` / `https` schemes will be accepted initially.
* Future: optional stripping of known analytics query parameters.
* Short codes will avoid ambiguous characters (e.g., 0/O, 1/l) when readability mode is enabled.
* No secrets are stored in the repository. Keep environment‑specific values out of source.

---

## 🤝 Contributing
We welcome contributions of all sizes—from typo fixes to architectural features.

### Quick Start (Happy Path)
1. Fork the repo & create a feature branch: `git checkout -b feature/meaningful-name`
2. Make focused changes (keep diffs tight)
3. Add or update tests (if the behavior is observable)
4. Run build + tests locally
5. Open a Pull Request (PR) referencing any related issue

### Commit Style
Use clear, imperative commit messages (present tense). Examples:
* `Add URL normalization utility`
* `Refactor short code generator to support custom alphabet`

### PR Expectations
* Describe motivation & context
* Include before/after behavior if relevant
* Note any breaking changes explicitly
* Keep PRs under ~400 lines net change where practical (split bigger work)

### Development Guidance
* Follow existing namespace & file‑scoped conventions
* Prefer small interfaces rather than premature frameworks
* Avoid introducing external dependencies without prior discussion (open an issue first)
* Do **not** log full sensitive URLs—truncate if added to logs

See detailed guidance in [`CONTRIBUTING.md`](CONTRIBUTING.md).

---

## 🐞 Reporting Issues
High quality issues speed up resolution.

### Before Filing
* Search existing [issues](https://github.com/nikneem/trunq/issues) (open & closed)
* Reproduce on the latest `main`

### Provide
| Field | Why |
|-------|-----|
| Summary | One sentence problem statement |
| Steps to Reproduce | Enumerated, minimal | 
| Expected vs Actual | Clarifies delta |
| Environment | OS, .NET SDK version, browser (if UI) |
| Logs / Trace IDs | Redact sensitive data |

Use the **Bug Report** or **Feature Request** template—automatically presented when creating a new issue.

---

## 🗣 Discussions & Ideas
For open‑ended design questions or roadmap suggestions, use [Discussions](https://github.com/nikneem/trunq/discussions) instead of issues.

---

## 📄 License
Distributed under the MIT License. See the [`LICENSE`](LICENSE) file.

---

## 🙌 Acknowledgements / Inspirations
* Simplicity & separation inspired by clean architecture principles.
* Uses .NET Aspire for local multi‑service development.

---

## 📬 Contact / Support
* Open an issue for bugs
* Start a discussion for feature shaping
* PRs welcome for incremental improvements

If you find this project useful, starring the repository helps visibility. ⭐

---

## 🧭 At a Glance
| Aspect | Status |
|--------|--------|
| Core URL creation | In progress |
| Resolver wiring | Scaffolded |
| Persistence | Pending design |
| Analytics | Future |
| Custom domains | Future |
| Test coverage | Bootstrapping |

---

Happy shortening!

— The Trunq Project
