<!--
Sync Impact Report
- Version change: template → 0.1.0
- Modified principles: placeholders → defined (5 principles)
- Added sections: Architecture Constraints, Workflow & Quality Gates
- Removed sections: none
- Templates requiring updates:
	- ✅ .specify/templates/spec-template.md
	- ✅ .specify/templates/plan-template.md (reference now backed by commands doc)
	- ⚠ pending: .specify/templates/tasks-template.md (no change required)
	- ✅ .specify/templates/commands/plan.md (created)
- Follow-up TODOs: none
-->

# AI Slide Builder Constitution

## Core Principles

### I. Confirm Before Generate (Outline + Style Brief)
Every presentation feature MUST have explicit user-confirmation gates before “full generation”:
- Outline MUST be drafted, shown, editable, and explicitly approved.
- Style Brief MUST be drafted, shown, editable, and explicitly approved.
- If the user skips either gate, the system MUST still present a derived draft for quick confirmation.
Rationale: Slide output quality depends on structure + style; confirmation reduces rework.

### II. Durable Work Products (Resumable, Versioned)
All authoring must be resumable across sessions/devices:
- The system MUST persist outline, style brief, conversation, revisions, and job progress.
- A user reopening a project MUST see the latest known progress and the latest usable preview.
- Regeneration MUST be attributable to a new revision/job so users can compare outputs.
Rationale: Users will close browsers/devices; continuity is core UX.

### III. MVP-First Simplicity (NON-NEGOTIABLE)
Start with the smallest architecture that can validate the product:
- Prefer coarse-grained jobs/stages over fine-grained orchestration.
- Avoid introducing extra services unless they remove clear, demonstrated pain.
- Any added complexity MUST be justified in the plan’s “Complexity Tracking” section.
Rationale: This is an MVP; complexity can be added later, but is hard to remove.

### IV. Provider-Agnostic AI Integration
Model access MUST be swappable by configuration:
- Use an OpenAI-compatible API surface behind a single abstraction boundary.
- Prompts MUST request structured, machine-checkable outputs (e.g., JSON + code blocks), not free-form file trees.
- Model/provider identifiers MUST NOT leak into core domain objects.
Rationale: Avoid lock-in and keep the system testable.

### V. Security and Data Handling
Protect user content and credentials:
- Secrets (API keys, storage keys) MUST NOT be committed; use configuration mechanisms.
- Logs MUST avoid including secrets and should minimize sensitive user content.
- Object storage MUST be treated as untrusted input; validate and handle missing/corrupt assets gracefully.
Rationale: Presentations can contain sensitive material; secure-by-default avoids rework.

## Architecture Constraints

- Frontend MUST be Vue 3 and MUST use Pinia for app state.
- Backend MUST be .NET (targeting .NET 10).
- Pug/CSS/JS are first-class generated artifacts; compilation to HTML MUST be repeatable.
- A Node-based Pug compilation tool MAY exist as an internal build tool, not a separately deployed service.
- Object storage MUST be abstracted behind a provider interface.
- MVP implementation MUST target OSS first.
- The abstraction MUST allow switching to S3 later without changing domain logic.

## Workflow & Quality Gates

- All work starts from a feature spec in specs/; non-trivial decisions MUST be recorded in a Clarifications section.
- Implementation plans MUST include a Constitution Check section and address any violations.
- Changes that affect multiple layers (frontend/backend/storage/AI integration) MUST include a small integration validation step.
- If a change introduces new configuration/secrets, it MUST document required environment variables.

## Governance

- This constitution supersedes templates, plans, and tasks.
- Amendments MUST be made via PR with:
	- A clear rationale
	- A compatibility/migration note (if behavior changes)
	- An updated version number
- Versioning policy:
	- MAJOR: Removing/weakening a principle or making governance less strict
	- MINOR: Adding a new principle/section or materially expanding requirements
	- PATCH: Clarifications, wording improvements, no semantic change
- Reviews MUST include an explicit “Constitution Check” pass/fail statement.

**Version**: 0.1.0 | **Ratified**: 2025-12-28 | **Last Amended**: 2025-12-28
