# Quickstart: AI Slide HTML Presentation Builder (MVP)

This quickstart describes the intended run workflow for the MVP. Code scaffolding will follow the structure in plan.md.

## Prerequisites

- .NET 10 SDK installed
- Node.js installed (for internal Pug compilation tool)
- OSS credentials available
- OpenAI-compatible model endpoint credentials available

## Project Structure

- `backend/`: .NET 10 Web API, Core, and Infrastructure.
- `frontend/`: Vue 3 + Vite + TypeScript application.
- `tools/pug-compiler/`: Internal Node.js tool for Pug compilation.

## Local Development

### Backend
```bash
cd backend/src
dotnet run --project SlideBuilder.Api
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

### Pug Compiler
```bash
cd tools/pug-compiler
npm install
node index.js input.pug output.html
```

## Configuration (environment variables)

Backend configuration should be provided via environment variables (no secrets committed).

- `AI_BASE_URL` (OpenAI-compatible base URL)
- `AI_API_KEY`
- `AI_MODEL`
- `OSS_ENDPOINT`
- `OSS_ACCESS_KEY_ID`
- `OSS_ACCESS_KEY_SECRET`
- `OSS_BUCKET`

## Manual Validation Checklist

Before considering the implementation complete, verify the following:

1.  **Project Creation**: Can create a new project and see it in the list.
2.  **Outline Approval**: Can draft an outline, edit it, and approve it.
3.  **Style Brief**: Can define style fields and approve the brief.
4.  **Generation**: Can start a generation job and see real-time progress via SignalR.
5.  **Preview**: Can view the compiled HTML preview of the generated slides.
6.  **Assets**: Can upload an image and insert it into a slide.
7.  **Export**: Can trigger an export and download the self-contained presentation.
8.  **Persistence**: Refreshing the page or restarting the server preserves all project data.
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
