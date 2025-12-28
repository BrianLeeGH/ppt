# Research: AI Slide HTML Presentation Builder

**Feature**: specs/001-ai-slide-builder/spec.md
**Date**: 2025-12-28

## Decision 1: OSS-first storage with provider abstraction

- Decision: Implement object storage using an OSS provider first, but require a storage provider interface so we can switch to S3 later.
- Rationale: Meets the user’s immediate preference (OSS) while preserving future flexibility.
- Alternatives considered:
  - S3-compatible everywhere (rejected: user explicitly wants OSS-first, not S3)
  - Directly coding OSS SDK throughout codebase (rejected: would make S3 migration expensive)

## Decision 2: OpenAI-compatible model endpoint behind a single client interface

- Decision: Use an OpenAI-compatible HTTP API surface behind `IModelClient`.
- Rationale: Allows easy model/provider switching via configuration without touching domain logic.
- Alternatives considered:
  - Provider-specific SDKs throughout (rejected: increases lock-in)

## Decision 3: Microsoft.Agent.Framework for orchestration (MVP)

- Decision: Start with `Microsoft.Agent.Framework` to turn chat into structured actions + generation calls.
- Rationale: Helps enforce structured outputs (JSON + code blocks) and tool boundaries.
- Alternatives considered:
  - Pure state-machine without agent layer (viable fallback if agent approach proves hard to control)

## Decision 4: Pug compilation as an internal tool

- Decision: Keep Pug compilation as an internal build tool (invoked by backend when producing preview/export), not a separately deployed service.
- Rationale: Minimizes operational complexity while supporting the user’s Pug requirement.
- Alternatives considered:
  - Deploy a separate Node compilation service (rejected: extra service complexity for MVP)
  - Avoid Pug altogether (rejected: user explicitly wants Pug)

## Decision 5: Real-time progress updates

- Decision: Use push-style progress updates (SignalR) for job stage/progress and preview-ready events.
- Rationale: Improves UX for long-running generation and supports reopen/resume experience.
- Alternatives considered:
  - Polling (simpler, acceptable fallback if SignalR is problematic)
