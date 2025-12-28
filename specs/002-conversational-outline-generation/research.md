# Research: Conversational Outline Generation

**Feature**: 002-conversational-outline-generation
**Date**: 2025-12-28
**Phase**: 0 - Research & Technology Decisions

## Overview

This document resolves all "NEEDS CLARIFICATION" items from the Technical Context and establishes technology choices for the conversational outline generation feature.

## Research Tasks

### 1. Database Selection for MVP

**Question**: Which Entity Framework-compatible database should be used?

**Decision**: SQLite for MVP, PostgreSQL for production

**Rationale**:
- SQLite requires zero configuration, perfect for MVP development and local testing
- File-based storage aligns with MVP-first simplicity (no separate DB server)
- Entity Framework Core has first-class SQLite support
- Easy migration path to PostgreSQL when scaling needs arise (same EF Core abstractions)
- The existing codebase likely already has EF Core configured; SQLite is the minimal-setup choice

**Alternatives Considered**:
- PostgreSQL immediately: Rejected because it requires Docker/server setup, violating MVP-first simplicity
- In-memory provider: Rejected because data must persist across sessions (requirement FR-012)
- SQL Server: Rejected due to licensing complexity and Windows-only limitations

**Implementation Notes**:
- Connection string: `Data Source=slidebuilder.db` in `appsettings.Development.json`
- Add NuGet package: `Microsoft.EntityFrameworkCore.Sqlite` to Infrastructure project
- Migration path: When moving to production, swap connection string to PostgreSQL (no code changes due to EF abstraction)

---

### 2. Testing Framework Selection

**Question**: What testing frameworks should be used for backend and frontend?

**Decision**:
- Backend: xUnit + FluentAssertions + Moq
- Frontend: Vitest + Vue Test Utils

**Rationale**:

**Backend (xUnit)**:
- xUnit is the modern, recommended testing framework for .NET
- Excellent async/await support (critical for AI service testing)
- Extensible via attributes and fixtures
- FluentAssertions provides readable assertions: `result.Should().NotBeNull()`
- Moq allows mocking `IModelClient`, `IUnitOfWork` for isolated unit tests

**Frontend (Vitest)**:
- Vitest is the official Vite-native test runner (faster than Jest)
- Built-in Vue 3 support via `@vue/test-utils`
- Same config as Vite build tool (no separate setup)
- Fast HMR-style test execution
- Modern ESM support aligns with Vue 3 ecosystem

**Alternatives Considered**:
- Backend NUnit: Rejected because xUnit is more idiomatic in modern .NET
- Frontend Jest: Rejected because Vitest has better Vite integration and performance
- E2E testing (Playwright): Deferred to post-MVP; unit/integration tests sufficient for Phase 1

**Implementation Notes**:
- Backend: Add packages to `SlideBuilder.Tests.csproj`: `xunit`, `xunit.runner.visualstudio`, `FluentAssertions`, `Moq`
- Frontend: Add packages to `frontend/package.json`: `vitest`, `@vitest/ui`, `@vue/test-utils`
- Frontend: Create `vitest.config.ts` extending Vite config

---

### 3. Conversational AI Prompt Engineering Best Practices

**Question**: How should prompts be structured for reliable outline generation and editing?

**Decision**: Structured prompt templates with JSON schema enforcement

**Rationale**:
- OpenAI-compatible APIs work best with clear instructions + JSON schema output format
- Separate prompt templates for different operations: initial generation, add slide, remove slide, reorder, edit content
- Use system messages for role definition, user messages for context + current outline state
- Include few-shot examples in prompts to improve accuracy (e.g., "User says 'add slide about X after slide 3' → JSON: {operation: 'insert', position: 4, ...}")
- Pin current outline state at the top of context to ensure AI always sees latest structure

**Best Practices for Outline Operations**:

1. **Initial Generation Prompt**:
   ```
   System: You are an expert presentation outline creator. Generate structured outlines with slide titles and 2-5 key points per slide.
   User: Create a [N]-slide presentation about [topic] for [audience].
   Current outline: null
   Output format: JSON array of {title: string, keyPoints: string[]}
   ```

2. **Edit Request Prompt**:
   ```
   System: You are an outline editor. Parse natural language edit requests into structured commands.
   User: [edit request text]
   Current outline: [JSON of current outline]
   Output format: {operation: "insert"|"delete"|"reorder"|"modify", targetSlideIndex: number, ...}
   ```

3. **Clarification Detection**:
   - If AI output includes `{needsClarification: true, questions: [...]}`, trigger follow-up conversation
   - Use structured output to distinguish between outline updates and clarification requests

**Alternatives Considered**:
- Free-form text parsing: Rejected due to unreliability; structured JSON reduces parsing errors
- Single mega-prompt for all operations: Rejected because specialized prompts perform better per operation type
- Chain-of-thought prompting: Deferred; JSON schema is sufficient for MVP

**Implementation Notes**:
- Create `OutlinePromptBuilder` class with methods: `BuildInitialGenerationPrompt()`, `BuildEditRequestPrompt()`, `BuildApprovalPrompt()`
- Store prompt templates as constants or embedded resources
- Include version field in prompts for A/B testing improvements later

---

### 4. Context Window Management Strategy

**Question**: How to handle 100+ message conversations within AI model token limits?

**Decision**: Sliding window (last 20 messages) + summarized history + pinned outline state

**Rationale**:
- Most AI models have 4K-32K token context windows; 100+ full messages exceed this
- Recent messages (last 20) contain most relevant conversational context
- Earlier messages can be summarized: "User requested 10 slides about climate change; outline was generated and refined through 15 edits"
- Current outline state MUST always be pinned at top of context (non-negotiable for correctness)
- This approach balances context richness with token efficiency

**Implementation Strategy**:

1. **Context Structure** (sent to AI):
   ```
   [System message: role definition]
   [Pinned: Current outline JSON]
   [Summarized: Messages 1-80 → "User created outline about X, made Y edits"]
   [Full: Messages 81-100 in chronological order]
   [User: Current request]
   ```

2. **Sliding Window Parameters**:
   - Window size: 20 messages (configurable via `AppSettings`)
   - Summary trigger: When message count > 30
   - Summary generation: Use AI to create 2-3 sentence summary of older messages
   - Re-summarize every 50 messages to keep summary current

3. **Pinned Content**:
   - Always include: Current outline JSON, outline status (Draft/Approved), slide count
   - Format: `CURRENT OUTLINE (v{revisionNumber}, {status}): {outlineJson}`

**Alternatives Considered**:
- Full message history: Rejected due to token limits and cost
- Fixed message truncation: Rejected because it loses important context arbitrarily
- Vector embeddings + semantic search: Rejected as over-engineering for MVP; sliding window is simpler and sufficient

**Implementation Notes**:
- Create `ContextWindowManager` class with method: `BuildContextForAI(IEnumerable<ConversationMessage> allMessages, Outline currentOutline)`
- Store summaries in a new `ConversationSummary` entity (or as special system messages)
- Add configuration: `AiSettings.ContextWindowSize` (default: 20), `AiSettings.SummaryThreshold` (default: 30)

---

### 5. Retry Strategy for AI Service Failures

**Question**: How should the system handle AI timeouts and service unavailability?

**Decision**: Exponential backoff with configurable retry count (default: 3)

**Rationale**:
- Transient failures (network hiccups, model overload) are common with external APIs
- Exponential backoff prevents overwhelming the service during outages
- User-facing manual retry button provides escape hatch when all auto-retries fail
- Configurable retry count allows environment-specific tuning (prod vs dev)

**Retry Logic**:

1. **Automatic Retries**:
   - Retry count: 3 (configurable via `AiSettings.MaxRetries`)
   - Backoff: 2^n seconds (1s, 2s, 4s)
   - Retry conditions: HTTP 429 (rate limit), 500 (server error), timeout, network exceptions
   - No retry: HTTP 400 (bad request - likely prompt issue), 401 (auth failure)

2. **Timeout Configuration**:
   - AI request timeout: 30 seconds (aligned with SC-001: <30s response time)
   - HttpClient timeout: 35 seconds (5s buffer for connection overhead)

3. **User Experience**:
   - Show loading spinner during retries with message: "Generating outline (attempt {n}/{maxRetries})..."
   - On final failure: Display error + "Retry" button
   - Error message: "AI service unavailable. Please check your connection and try again."

**Alternatives Considered**:
- Infinite retries: Rejected because users need to know when to give up
- Circuit breaker pattern: Deferred to post-MVP; adds complexity without clear MVP benefit
- Queue-based async processing: Rejected; real-time feedback is a core UX requirement

**Implementation Notes**:
- Use `Polly` library for retry policies: `builder.Services.AddHttpClient<IModelClient, OpenAiCompatibleModelClient>().AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(...))`
- Add to `AiSettings`: `MaxRetries` (default: 3), `TimeoutSeconds` (default: 30)
- Frontend: Disable "Send" button during retries, show attempt counter

---

### 6. Real-time Outline Update Mechanism

**Question**: How should outline updates be pushed to the frontend in real-time?

**Decision**: SignalR hub events with optimistic UI updates

**Rationale**:
- SignalR is already integrated in the codebase (seen in `@microsoft/signalr` dependency and `JobsHub`)
- Reusing existing hub infrastructure aligns with MVP-first simplicity
- Optimistic updates (UI shows change immediately, then confirms via SignalR) provide instant feedback

**SignalR Event Flow**:

1. **User sends message**:
   - Frontend: POST `/api/conversation/messages` with message content
   - Optimistic: Add user message to local store immediately
   - Backend: Save message, trigger `DraftOutlineStage` job

2. **AI processes request**:
   - Backend: `DraftOutlineStage` calls `IModelClient`, updates outline
   - Backend: Saves new outline revision
   - Backend: SignalR hub sends `OutlineUpdated` event with outline JSON

3. **Frontend receives update**:
   - SignalR handler: `hub.on('OutlineUpdated', (outline) => outlineStore.setOutline(outline))`
   - UI re-renders outline display automatically (Vue reactivity)
   - Highlight changed slides (visual diff)

**Event Schema**:
```typescript
// SignalR event
interface OutlineUpdatedEvent {
  projectId: string;
  outlineId: string;
  outline: OutlineDto;
  revisionNumber: number;
  changedSlideIndices: number[]; // For highlighting
}
```

**Alternatives Considered**:
- Polling: Rejected due to latency and server load
- WebSockets (raw): Rejected because SignalR is already integrated
- Server-Sent Events: Rejected; SignalR provides bidirectional communication if needed later

**Implementation Notes**:
- Extend `JobsHub` with `OutlineUpdated(Guid projectId, OutlineDto outline)` method
- Frontend: Subscribe to hub in `conversationStore` initialization
- Add visual diff: Compare `changedSlideIndices` array, apply CSS class `outline-slide--changed`

---

## Summary of Decisions

| Topic | Decision | Key Rationale |
|-------|----------|---------------|
| Database | SQLite (MVP) → PostgreSQL (prod) | Zero-config, EF Core compatible, easy migration |
| Backend Testing | xUnit + FluentAssertions + Moq | Modern .NET standard, async-friendly |
| Frontend Testing | Vitest + Vue Test Utils | Vite-native, fast, official Vue support |
| Prompt Engineering | Structured templates + JSON schema | Reliability, parseability, operation-specific |
| Context Management | Sliding window (20 msgs) + summary + pinned outline | Token efficiency, preserves relevance |
| Retry Strategy | Exponential backoff (3 retries, 2^n sec) | Handles transients, user escape hatch |
| Real-time Updates | SignalR `OutlineUpdated` event + optimistic UI | Reuses existing infra, instant feedback |

## Technology Stack Finalized

**Backend**:
- .NET 10.0 (C#)
- ASP.NET Core Web API
- Entity Framework Core 10.x + SQLite (MVP)
- SignalR for real-time communication
- Polly for resilience (retry policies)
- xUnit + Moq + FluentAssertions for testing

**Frontend**:
- Vue 3.5.24 (TypeScript)
- Pinia 3.0.4 (state management)
- Vue Router 4.6.4
- Axios 1.13.2 (HTTP client)
- @microsoft/signalr 10.0.0
- Vitest + @vue/test-utils for testing

**AI Integration**:
- OpenAI-compatible API (via `IModelClient`)
- Configurable provider (BaseUrl, ApiKey, Model)
- Structured JSON output parsing

## Next Steps

All NEEDS CLARIFICATION items resolved. Proceeding to **Phase 1: Design & Contracts**.
