# Implementation Plan: Conversational Outline Generation

**Branch**: `002-conversational-outline-generation` | **Date**: 2025-12-28 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from [specs/002-conversational-outline-generation/spec.md](spec.md)

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Transform the outline generation workflow from form-based editing to a fully conversational AI-driven experience. Users will create, refine, and approve presentation outlines through natural language dialogue with an AI assistant, with all outline modifications interpreted and applied conversationally. The system will enforce sequential processing via UI controls, preserve conversation history permanently, implement retry strategies for AI service failures, and use a sliding window context management approach for long conversations while always pinning the current outline state.

## Technical Context

**Language/Version**:
- Backend: C# / .NET 10.0
- Frontend: TypeScript / Vue 3.5.24

**Primary Dependencies**:
- Backend: ASP.NET Core, SignalR (real-time communication), Entity Framework Core (implied by UnitOfWork pattern)
- Frontend: Vue 3, Pinia 3.0.4 (state management), Vue Router 4.6.4, SignalR Client (@microsoft/signalr 10.0.0), Axios 1.13.2

**Storage**:
- Database: NEEDS CLARIFICATION (Entity Framework-compatible DB - likely SQLite for MVP or PostgreSQL for production)
- Object Storage: Abstracted via provider interface (targeting OSS initially, S3-compatible later)

**AI Integration**:
- OpenAI-compatible API via `IModelClient` abstraction
- Supports structured JSON output parsing from markdown code blocks
- Model/provider configured via `AiSettings` (BaseUrl, ApiKey, Model)

**Testing**: NEEDS CLARIFICATION (likely xUnit for backend, Vitest for frontend based on stack)

**Target Platform**: Web application (browser + server)

**Project Type**: Web (separate backend/frontend with real-time communication)

**Performance Goals**:
- AI response time: <30 seconds for outline generation
- Outline display update: <2 seconds post-AI response
- Real-time communication: immediate SignalR hub notifications

**Constraints**:
- UI must enforce sequential message processing (no concurrent outline edits)
- Permanent retention for all conversation messages and revisions (no automatic deletion)
- Context window management required for 100+ message conversations

**Scale/Scope**:
- MVP feature supporting single-user outline authoring
- ~5-8 new/modified backend files (stages, controllers, DTOs)
- ~3-5 new frontend components (chat UI, outline display integration)
- Expected: 50 conversation turns per outline lifecycle

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Principle I: Confirm Before Generate (Outline + Style Brief)
**Status**: ✅ PASS

- Feature explicitly implements the outline confirmation gate via conversational approval (FR-008, FR-009)
- System prevents slide generation until outline status is Approved (FR-023)
- When users modify approved outlines, system creates new draft version and warns about regeneration impact (FR-024)

### Principle II: Durable Work Products (Resumable, Versioned)
**Status**: ✅ PASS

- Permanent retention of all conversation messages (FR-010)
- Conversation history and outline state restored on project reopen (FR-012)
- Resumable outline generation across sessions (FR-022)
- Outline revisions tracked via `OutlineRevision` entity linked to conversation messages (from spec dependencies)

### Principle III: MVP-First Simplicity
**Status**: ✅ PASS

- Leverages existing `DraftOutlineStage` job stage rather than introducing new orchestration
- Reuses existing `ConversationMessage` entity and SignalR infrastructure
- No new services; enhancement of existing job-based workflow
- Complexity justified: conversational interface is the core feature requirement, not added complexity

### Principle IV: Provider-Agnostic AI Integration
**Status**: ✅ PASS

- Uses existing `IModelClient` abstraction (OpenAI-compatible)
- Prompts will request structured JSON output for outline proposals (aligns with existing `GenerateStructuredAsync<T>`)
- No model-specific logic in domain layer

### Principle V: Security and Data Handling
**Status**: ✅ PASS

- Relies on existing `AiSettings` configuration (BaseUrl, ApiKey via User Secrets or environment variables)
- No new secrets introduced
- Conversation messages stored in database (existing persistence layer)
- Object storage not directly involved in this feature

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── SlideBuilder.Api/
│   │   ├── Controllers/
│   │   │   └── ConversationController.cs      # NEW: API endpoints for conversation
│   │   ├── Contracts/
│   │   │   ├── ConversationDtos.cs            # NEW: DTOs for messages, requests
│   │   │   └── OutlineDtos.cs                 # MODIFIED: Add conversational context
│   │   └── Hubs/
│   │       └── JobsHub.cs                     # MODIFIED: Add outline update events
│   ├── SlideBuilder.Core/
│   │   ├── Domain/
│   │   │   └── Entities.cs                    # MODIFIED: Add OutlineRevision entity
│   │   ├── Jobs/Stages/
│   │   │   └── DraftOutlineStage.cs           # MODIFIED: Implement conversational logic
│   │   ├── AI/
│   │   │   ├── Prompts/
│   │   │   │   ├── OutlinePromptBuilder.cs    # NEW: Builds prompts for outline ops
│   │   │   │   └── ContextWindowManager.cs    # NEW: Sliding window + pinning
│   │   │   └── Models/
│   │   │       ├── OutlineProposal.cs         # NEW: Structured AI output
│   │   │       └── OutlineEditCommand.cs      # NEW: Parsed edit instructions
│   │   └── Persistence/
│   │       └── IConversationRepository.cs     # NEW: Conversation queries
│   └── SlideBuilder.Infrastructure/
│       └── Persistence/
│           └── ConversationRepository.cs      # NEW: EF Core implementation
└── tests/
    └── SlideBuilder.Tests/
        ├── Jobs/
        │   └── DraftOutlineStageTests.cs      # NEW: Unit tests for stage
        └── AI/
            └── ContextWindowManagerTests.cs   # NEW: Context management tests

frontend/
├── src/
│   ├── components/
│   │   ├── ConversationPanel.vue              # NEW: Chat UI component
│   │   ├── MessageList.vue                    # NEW: Message display
│   │   ├── MessageInput.vue                   # NEW: Input with send control
│   │   └── OutlineDisplay.vue                 # MODIFIED: Real-time updates
│   ├── stores/
│   │   ├── conversationStore.ts               # NEW: Pinia store for messages
│   │   └── outlineStore.ts                    # MODIFIED: Handle conversational updates
│   ├── services/
│   │   ├── conversationService.ts             # NEW: API client for conversation
│   │   └── signalrService.ts                  # MODIFIED: Subscribe to outline events
│   └── views/
│       └── ProjectView.vue                    # MODIFIED: Integrate conversation panel
└── tests/
    └── unit/
        └── conversationStore.spec.ts          # NEW: Store unit tests
```

**Structure Decision**: Web application with separate backend (.NET 10) and frontend (Vue 3). This feature adds conversational capabilities to the existing outline workflow by:

- Enhancing the `DraftOutlineStage` job stage with AI-driven outline generation
- Adding a new conversation API and repository layer
- Building frontend chat UI components integrated with existing outline display
- Leveraging existing SignalR infrastructure for real-time outline updates

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

**No violations**: All constitution principles passed. No additional complexity beyond feature requirements.

---

## Post-Design Constitution Re-Check

*Phase 1 design complete. Re-evaluating all principles.*

### Principle I: Confirm Before Generate (Outline + Style Brief)
**Status**: ✅ PASS (reconfirmed)

**Design Validation**:
- API contract includes conversational approval endpoint (POST `/conversation/messages` with approval intent)
- Data model enforces `OutlineStatus` transitions: Draft → Approved via conversation only
- `OutlineRevision` entity tracks approval events via `TriggeredByMessageId`
- When editing approved outline, system creates new Draft + preserves approved version (documented in data-model.md)

### Principle II: Durable Work Products (Resumable, Versioned)
**Status**: ✅ PASS (reconfirmed)

**Design Validation**:
- `OutlineRevision` entity provides full version history with sequential `RevisionNumber`
- `ConversationMessage` persisted permanently (no TTL, no deletion policy)
- `ConversationSummary` enables long-term conversation continuity beyond 100+ messages
- SignalR events allow real-time resumption when clients reconnect (`JoinProjectGroup` re-subscribes)
- Quickstart.md documents reconnection strategy: re-fetch current state on reconnect

### Principle III: MVP-First Simplicity
**Status**: ✅ PASS (reconfirmed)

**Design Validation**:
- Zero new microservices; all logic in existing API + job stages
- Reused entities: `ConversationMessage` (no schema changes), `Outline` (behavior only)
- New entities minimal: `OutlineRevision` (3 fields), `ConversationSummary` (4 fields)
- SignalR events extend existing `JobsHub` (2 new events: `OutlineUpdated`, `MessageReceived`)
- No external dependencies beyond existing stack (EF Core, SignalR, Pinia)
- Research.md confirms SQLite for MVP (zero-config database)

### Principle IV: Provider-Agnostic AI Integration
**Status**: ✅ PASS (reconfirmed)

**Design Validation**:
- `OutlinePromptBuilder` outputs plain strings consumed by `IModelClient` interface
- Structured output parsed from JSON code blocks (existing `GenerateStructuredAsync<T>` logic)
- No OpenAI-specific libraries; uses generic HTTP client in `OpenAiCompatibleModelClient`
- Prompt templates stored as code constants (no provider lock-in)
- Research.md documents retry strategy via Polly (provider-agnostic resilience)

### Principle V: Security and Data Handling
**Status**: ✅ PASS (reconfirmed)

**Design Validation**:
- Quickstart.md documents User Secrets for `Ai:ApiKey` (NOT appsettings.json)
- API contracts include standard error responses (no secret leakage in error details)
- SignalR authorization via `JoinProjectGroup` enforces project-level access control
- `ConversationMessage.Content` max length: 10,000 chars (prevents abuse)
- No PII/secrets in SignalR event payloads (contracts/signalr-events.md verified)

**Final Verdict**: ✅ **ALL PRINCIPLES PASS** - Design adheres to constitution. Ready for implementation (Phase 2: Tasks generation).
