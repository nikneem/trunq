# Contributing to Trunq

Thanks for your interest in contributing! This document explains how to propose changes, report issues, and submit pull requests in a way that keeps the project healthy and maintainable.

## Code of Conduct
Be respectful, constructive, and inclusive. Harassment or discrimination of any form is not tolerated. (If a formal CoC is needed later we can adopt Contributor Covenant.)

## Ways to Contribute
- Bug reports
- Feature requests / design suggestions
- Documentation improvements
- Code enhancements (refactors, tests, features)
- Tooling / DX (developer experience) improvements

## Ground Rules
- Keep changes focused and minimal. Split large work into multiple PRs.
- Avoid introducing external dependencies without prior discussion (open an issue first).
- Prefer clarity over cleverness; optimize for readability.
- Uphold existing architecture boundaries (API vs Resolver vs Core vs Aspire host).
- Avoid logging full sensitive URLs (truncate or hash if necessary).

## Development Setup
1. Install .NET 9 SDK
2. Clone the repo: `git clone https://github.com/nikneem/trunq.git`
3. Restore & build: `dotnet restore src/Trunq.sln && dotnet build src/Trunq.sln`
4. Run locally with Aspire AppHost: `dotnet run --project src/Aspire/HexMaster.Trunq.Aspire/HexMaster.Trunq.Aspire.AppHost/HexMaster.Trunq.Aspire.AppHost.csproj`
5. (Optional) Start the Angular frontend: `cd src/Frontend && npm install && npm start`

## Branching Strategy
- `main` is the active integration branch (may be unstable during early alpha).
- Create feature branches from `main`: `feature/<short-description>`
- Rebase (preferred) or merge `main` before finalizing your PR to reduce noise.

## Commit Messages
Use imperative mood and concise description:
```
Add short code generator interface
Fix URL validation for uppercase schemes
Refactor resolver pipeline for DI clarity
```
Reference issues when applicable: `Fix #42: Add collision retry logic`.

## Pull Requests
Checklist before opening:
- Build passes locally
- New / changed logic has tests (if testable)
- Public behavior documented (README or XML comments where appropriate)
- No unrelated formatting churn
- Squash commits if there is excessive noise / fixups

Template (feel free to copy):
```
### Summary
Short explanation of what the change accomplishes.

### Motivation
Why is this needed? Link to issue/discussion if applicable.

### Changes
- Bullet list of key changes

### Testing
Describe how you validated (unit tests, manual steps, etc.)

### Checklist
- [ ] Tests added/updated (n/a if not testable)
- [ ] Docs updated (README / inline)
- [ ] No new external dependencies (or discussed)
- [ ] Follows architecture & style guidelines
```

## Testing Guidance
(Initial tests forthcoming.) When present, run: `dotnet test src/Trunq.sln`.
Add tests for:
- URL validation / normalization
- Short code generation (length, alphabet, collisions)
- Endpoint result codes & ProblemDetails where applicable

Avoid over-mocking; prefer testing observable behavior.

## Adding Dependencies
Open an issue first. Provide:
- Use case
- Alternatives considered
- Maintenance / security implications
If accepted, pin versions and keep transitive impact low.

## Issue Triage Labels (Planned)
| Label | Meaning |
|-------|---------|
| `bug` | Confirmed defect |
| `enhancement` | Feature or improvement |
| `needs-info` | Awaiting reporter clarification |
| `good-first-issue` | Beginner-friendly task |
| `investigate` | Requires spike / research |
| `blocked` | Pending external dependency or decision |

## Security
No secrets in the repo. Report sensitive security concerns privately (contact repo owner via GitHub profile) rather than in a public issue.

## Release Process (Future)
Early alpha: ad-hoc tags. Later: semantic versioning once API surface stabilizes.

## Roadmap Collaboration
See README roadmap section; propose changes through Discussions or labeled enhancement issues.

## Style & Patterns
- File-scoped namespaces
- Async APIs where I/O bound
- Minimal APIs unless complexity justifies controllers
- Structured logging placeholders: `logger.LogInformation("Short code {Code} created", code);`

## Thank You
Your time and effort make Trunq better. 🙏
