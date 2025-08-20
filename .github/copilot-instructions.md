# Copilot Project Instructions: Trunq

These instructions define the boundaries, goals, architecture, conventions, and safe operating guidelines for GitHub Copilot when assisting on the Trunq project.

---
## 1. Project Summary
Trunq is a URL shortener application. It accepts long URLs and generates short, shareable identifiers ("trunqs") that redirect clients to the original target. The solution is built with .NET 9 (C# 13) and is orchestrated locally with .NET Aspire for multi-service development.

Core aims:
- Fast creation and resolution of short URLs.
- Clear separation of concerns between API, resolution, and orchestration layers.
- Observability, resilience, and secure handling of user input.
- Extensibility for analytics, rate limiting, and custom domains (future).

Do NOT introduce unrelated domains (e.g., authentication providers, billing, AI content generation) unless explicitly requested.

---
## 2. Current Solution Structure (High-Level)
```
src/
  Aspire/
    HexMaster.Trunq.Aspire.AppHost/        -> Aspire AppHost (composition/orchestration)
    HexMaster.Trunq.Aspire.ServiceDefaults/-> Common service setup (e.g., telemetry/logging/ext.)
  Backend/
    HexMaster.Trunq.Api/                   -> Public-facing HTTP API for creating/managing short URLs
    HexMaster.Trunq.Resolver/              -> (Planned/used) Service for resolving short codes to target URLs
```
Additional services (analytics, admin UI, etc.) are future scope and must be user-requested before creation.

---
## 3. Architectural Boundaries & Guidance
- Follow a service-per-responsibility model; avoid God services.
- Aspire is used only for local dev/service composition (not a production orchestrator).
- Keep cross-service contracts explicit (DTOs / HTTP endpoints). Avoid implicit shared state.
- Introduce persistence abstractions before choosing a concrete data store if not yet present.
- Input sanitation & validation mandatory (URLs must be syntactically valid, normalized, and length-checked).
- Short code generation must be:
  - Collision-resistant (check uniqueness)
  - Deterministic only if explicitly requested (default: random/opaque)
  - Free of ambiguous characters if human readability is a concern (configurable later)
- Avoid premature optimization; prefer clarity and test coverage.

Forbidden without explicit user approval:
- Adding external SaaS dependencies.
- Introducing message brokers / queues / caches / databases not already discussed.
- Embedding secrets or production connection strings.

---
## 4. Language & Framework Constraints
- Target Framework: net9.0.
- Language: C# 13. You may use new features (e.g., enhancements to primary constructors, interceptors if stabilized) but only when they add readability.
- Prefer minimal APIs where consistent with existing code; otherwise conventional controllers if complexity grows.
- Use async/await end-to-end for I/O paths.
- Favor built-in BCL and ASP.NET Core features before adding libraries.

---
## 5. Coding Conventions
- Namespaces: file-scoped.
- Nullable reference types: enabled by default (assume on; preserve). Avoid suppressions; model optionality explicitly.
- Dependency Injection: use constructor injection; avoid service-location patterns.
- Logging: use structured logging placeholders (e.g., logger.LogInformation("Short code {Code} created", code)). Do not log full original URLs if privacy-sensitive (truncate or hash if necessary).
- Configuration: use strongly typed options classes where configuration complexity increases.
- Validation: use minimal custom validators or FluentValidation if requested; currently prefer lightweight manual validation for simplicity.
- Error handling: map validation errors to 400; not-found to 404; internal issues 500; avoid leaking stack traces in responses.
- Return ProblemDetails when appropriate.

---
## 6. Testing Guidance
When adding functionality, propose or create:
- Unit tests for short code generation, validation, normalization.
- Integration tests for API endpoints (using WebApplicationFactory) only after API stabilizes.
- Avoid over-mocking; test observable behavior.
Do not fabricate tests for features not yet present.

---
## 7. Security & Privacy Boundaries
- Never store or echo secrets, API keys, or connection strings in code or docs.
- Treat all URLs as untrusted input (validate scheme: http/https unless user requests broader support).
- Prevent open redirect abuses (ensure resolved URL is exactly stored target).
- Rate limiting and abuse detection are future enhancements; do not stub unless asked.
- Do not log personally identifiable information; minimize logging of raw URLs.

---
## 8. Performance & Scalability (Early Stage)
- Optimize for clarity over micro-optimizations.
- Anticipate future introduction of caching (e.g., in-memory or distributed) for hot short codes but do not add now without instruction.
- Keep interfaces small and purposeful to enable future substitutions (e.g., persistence, encoding strategies).

---
## 9. Observability
- Encourage adoption of OpenTelemetry (traces, metrics, logs) through ServiceDefaults if/when instrumentation is added.
- Use activity names and tags that reflect domain concepts: e.g., shorturl.create, shorturl.resolve.
- Do not invent instrumentation without confirming existing packages or user desire.

---
## 10. Use of Microsoft Docs MCP Server
Always leverage the microsoft.docs.mcp server for authoritative .NET / ASP.NET Core knowledge when:
- Unsure about APIs or attributes (.NET 9 / C# 13 specifics).
- Providing examples involving hosting, minimal APIs, dependency injection, HttpResults, configuration, OpenTelemetry, or logging.
Behavior:
1. Query the MCP server before speculating about framework capabilities.
2. Prefer official patterns surfaced via docs (e.g., WebApplicationBuilder, ProblemDetails, output caching patterns once stable).
3. Cite (briefly) that guidance is based on official docs when summarizing.
4. Avoid referencing deprecated APIs if the docs show newer guidance.

If documentation is ambiguous, ask the user for clarification before generating large changes.

---
## 11. Feature Introduction Policy
Before adding a new component (e.g., custom domain management, analytics service, admin dashboard):
1. Confirm requirement with user.
2. Outline proposed design (responsibilities, data contracts, persistence impacts).
3. Await approval before implementation.

---
## 12. Short Code Generation Guidelines
- Encoded alphabet: propose a default (e.g., Base62 without confusing chars) only after user alignment.
- Length: start with a fixed length (e.g., 7–8) unless requirement changes.
- Collision Handling: re-generate or escalate after reasonable retry threshold.
- Do not embed sequential database IDs directly.

---
## 13. Persistence (Placeholder Guidance)
If/when persistence is introduced:
- Abstract via repository or data service interface; keep domain logic separate.
- Support migration strategy discussion before picking EF Core / Dapper / other.
- Avoid committing large schema assumptions prematurely.

---
## 14. API Design Guidelines
- Stable route prefix: /api/v1 for public endpoints once versioning needed (add only when requested).
- Responses: return canonical representation of a short URL object (id/code, originalUrl, createdAt, expiresAt? optional).
- Use 201 Created with Location header for creation.
- Idempotency: Not required by default; future enhancement if custom slugs allowed.

---
## 15. Documentation & Comments
- Keep inline comments purposeful (why > what).
- Update README only with user-approved scope changes.
- Avoid generating speculative architecture diagrams without consensus.

---
## 16. What NOT To Do
- Do not introduce auth/security libraries without request.
- Do not generate unrelated sample apps or multi-language ports.
- Do not refactor broadly (rename namespaces, restructure folders) without explicit rationale and user consent.
- Do not fabricate capabilities (e.g., analytics, QR generation) not present or requested.

---
## 17. Interaction Style for Copilot
When responding:
- Be concise and neutral.
- Offer a short rationale before code changes.
- Propose small, reviewable diffs.
- Ask clarifying questions when requirements are ambiguous.
- Reference having consulted microsoft.docs.mcp where relevant.

---
## 18. Incremental Evolution Path (For Future Discussion Only)
(Only act when user requests.) Potential next steps:
- Persistence layer (e.g., PostgreSQL / SQLite for dev).
- Caching (e.g., Redis) for hot lookups.
- Custom slug support & reserved word handling.
- Analytics events (click counts, referrers) through an event pipeline.
- Expiration & pruning strategy.
- Domain-based multi-tenancy.

---
## 19. Acceptance of These Instructions
These boundaries supersede generic suggestions. If conflicts arise, ask the user for clarification rather than assuming.

---
## 20. Quick Checklist Before Suggesting Code
[ ] Is the change within the URL shortening domain?  
[ ] Does it respect current architecture & not add unapproved services?  
[ ] Are inputs validated & errors mapped correctly?  
[ ] Could official docs clarify anything? (Consult microsoft.docs.mcp if yes.)  
[ ] Is code minimal, testable, and consistent with conventions?  

---
End of copilot-instructions.
