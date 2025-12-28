# Quickstart: AI Slide HTML Presentation Builder (MVP)

This quickstart describes the intended run workflow for the MVP. Code scaffolding will follow the structure in plan.md.

## Prerequisites

- .NET 10 SDK installed
- Node.js installed (for internal Pug compilation tool)
- OSS credentials available
- OpenAI-compatible model endpoint credentials available

## Configuration (environment variables)

Backend configuration should be provided via environment variables (no secrets committed).

- `AI_BASE_URL` (OpenAI-compatible base URL)
- `AI_API_KEY`
- `AI_MODEL`
- `OSS_ENDPOINT`
- `OSS_ACCESS_KEY_ID`
- `OSS_ACCESS_KEY_SECRET`
- `OSS_BUCKET`
- `OSS_PREFIX` (optional)

## Run (intended)

- Start backend API (ASP.NET Core)
- Start frontend dev server (Vite + Vue)
- Open the frontend UI and:
  - Create a project
  - Draft and approve outline
  - Draft and approve style brief
  - Start generation job
  - Watch progress events and open preview

## MVP Validation Checklist

- Outline approval gate blocks generation by default
- Style brief approval gate blocks CSS/JS finalization by default
- Starting a job persists stage progress and survives browser restart
- Preview is compilable and reflects the latest revision
- OSS asset upload/download works via provider abstraction
