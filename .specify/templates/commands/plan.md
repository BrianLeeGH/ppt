# /speckit.plan Command Workflow

This document is referenced by `.specify/templates/plan-template.md`.

## Purpose

Generate a feature implementation plan (`plan.md`) from a completed feature specification (`spec.md`).

## Inputs

- Feature spec: `specs/[###-feature-name]/spec.md`
- Constitution: `.specify/memory/constitution.md`

## Output

- `specs/[###-feature-name]/plan.md`

## Required Checks

- Include a **Constitution Check** section in the plan.
- If the plan violates any constitution rule, record it in **Complexity Tracking** with rationale.

## Guidance

- Prefer MVP-first sequencing (P1 story first).
- Keep architecture minimal unless justified.
- Identify cross-cutting concerns (storage, configuration, security) early.
