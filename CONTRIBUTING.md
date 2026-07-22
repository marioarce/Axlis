# Contributing to Axlis

Thank you for your interest in contributing! Please read this guide before opening a PR.

---

## Branch Strategy

```
main        (protected — production, never commit directly)
  └─ develop        (integration branch — base for all PRs)
       └─ feature/<N>-<short-description>   (one issue per branch)
```

- Always branch from `develop`.
- Branch name format: `feature/<issue-number>-<short-description>` (e.g. `feature/3-abstractions`).
- **Never commit directly to `main` or `develop`.**

---

## Commit Format

We use [Conventional Commits](https://www.conventionalcommits.org/) with a **GitHub issue number** scope:

```
<type>(#<issue>): <short imperative summary>

[optional body — what and why, not how]
```

### Types

| Type | When |
|---|---|
| `feat` | New feature / public API addition |
| `fix` | Bug fix |
| `refactor` | Code change that is not a bug fix or feature |
| `chore` | Tooling, dependencies, repo scaffolding |
| `docs` | Documentation only |
| `style` | Formatting, naming, editorconfig (no logic change) |
| `test` | Adding or fixing tests |
| `ci` | CI/CD pipeline changes |
| `perf` | Performance improvement |

### Examples

```
feat(#4): add AxesAdapter with Parent, Children, and Siblings

fix(#12): correct IsFullyLoaded check when parent is root

docs(#7): add Getting Started guide to /docs

ci(#2): add GitHub Actions CI workflow
```

### Rules

- Subject line: **imperative mood**, ≤ 72 characters, no period at the end.
- Keep commits small and focused — one logical concern per commit.
- Reference the issue in every commit scope (`(#N)` is mandatory).

---

## Pull Request Checklist

Before marking a PR ready for review:

- [ ] Branch is up to date with `develop`.
- [ ] `dotnet build --configuration Release` passes with **0 warnings, 0 errors**.
- [ ] `dotnet test --configuration Release` passes.
- [ ] All public APIs have XML doc comments.
- [ ] No copyright references in shipped code.
- [ ] PR title follows the same Conventional Commit format.
- [ ] PR body includes `Closes #<N>` to auto-close the issue on merge.

---

## Code Style

- File-scoped namespaces (`namespace Axlis.ORM.Core;`).
- Private fields: `_camelCase`.
- `ConfigureAwait(false)` in all library `await` calls.
- Typed `ILogger<T>` — never `Console.Write` in library code.
- No magic strings — constants or static readonly fields for repeated literals.

---

## Development Setup

```bash
git clone https://github.com/marioarce/Axlis.git
cd Axlis
dotnet restore
dotnet build --configuration Release
dotnet test
```

---

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By contributing you agree to abide by its terms.
