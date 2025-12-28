# Implementation Plan: AI Slide HTML Presentation Builder

**Branch**: `001-ai-slide-builder` | **Date**: 2025-12-28 | **Spec**: specs/001-ai-slide-builder/spec.md
**Input**: Feature specification from `specs/001-ai-slide-builder/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Deliver an MVP system that lets users author slide-style HTML presentations via chat, with explicit confirmation gates (Outline + Style Brief), durable/resumable jobs across sessions, a compiled preview that matches final output, and OSS-first storage with a swappable abstraction for future S3 migration.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: .NET 10 (backend) + TypeScript (frontend)  
**Primary Dependencies**: ASP.NET Core Web API, SignalR, EF Core (SQLite), Microsoft.Agent.Framework, Vue 3, Pinia, Vite; OpenAI-compatible HTTP client abstraction; OSS SDK behind storage abstraction  
**Storage**: SQLite for metadata (projects/revisions/jobs) + OSS for assets/exports/previews (via provider abstraction)  
**Testing**: xUnit (backend), Vitest (frontend)  
**Target Platform**: Server: Windows/Linux; Client: modern browsers
**Project Type**: web (frontend + backend)  
**Performance Goals**: MVP responsiveness for typical decks (≤ 20 slides, ≤ 30 images); fast “time to first preview”  
**Constraints**: Must be resumable across sessions; confirmation gates for Outline + Style Brief; provider-agnostic AI endpoint; OSS-first but swappable storage provider  
**Scale/Scope**: Single-user workspace per installation (MVP); multi-user/collab out of scope

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- I. Confirm Before Generate: PASS (Outline + Style Brief are explicit gates in spec; impact-summary confirmations included)
- II. Durable Work Products: PASS (Jobs + checkpoints + revisions required; reopen shows last progress + preview)
- III. MVP-First Simplicity: PASS (coarse-grained stages; avoid extra services; Pug compile may be an internal tool)
- IV. Provider-Agnostic AI Integration: PASS (OpenAI-compatible endpoint behind `IModelClient` abstraction)
- V. Security and Data Handling: PASS (secrets via config; storage treated as untrusted; error handling required)
- Architecture Constraints: PASS (Vue+Pinia, .NET 10, OSS-first storage + provider abstraction)

## Project Structure

### Documentation (this feature)

```text
specs/001-ai-slide-builder/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── openapi.yaml
└── checklists/
  └── requirements.md
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
backend/
├── src/
│   ├── SlideBuilder.Api/
│   ├── SlideBuilder.Core/
│   └── SlideBuilder.Infrastructure/
└── tests/
  └── SlideBuilder.Tests/

frontend/
├── src/
│   ├── app/
│   ├── pages/
│   ├── components/
│   └── stores/
└── tests/

tools/
└── pug-compiler/
```

**Structure Decision**: Web application with separated frontend/backend plus a small internal tool folder for Pug compilation.

## Phased Plan (MVP)

### Phase 0: Research (output: research.md)

- Confirm .NET 10 + Microsoft.Agent.Framework integration approach (minimal agent + tool calls)
- Decide OSS SDK choice and define storage provider abstraction boundary
- Decide preview pipeline approach (compile-for-preview and asset URL strategy)

### Phase 1: Design (outputs: data-model.md, contracts/openapi.yaml, quickstart.md)

- Define entities and relationships for Project/Deck/Outline/StyleBrief/Slide/Revision/Job/Checkpoint/Asset
- Define HTTP API contracts for authoring, approval gates, jobs, preview, export, assets
- Define SignalR events for job progress updates

### Phase 2: Planning Deliverables (in this plan)

- Identify MVP slice order aligned to spec priorities (P1 outline → P2 style brief → P3 generation → P4 export → P5 assets)
- Define minimal implementation sequence with clear checkpoints and rollback paths

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
