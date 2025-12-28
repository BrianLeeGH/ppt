# Tasks: Conversational Outline Generation

**Input**: Design documents from `/specs/002-conversational-outline-generation/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Tests**: Tests are NOT explicitly requested in the feature specification, so test tasks are excluded from this implementation plan.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

This is a web application with separate backend and frontend:
- Backend: `backend/src/`
- Frontend: `frontend/src/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and database structure for conversational outline generation

- [ ] T001 Add Microsoft.EntityFrameworkCore.Sqlite package to backend/src/SlideBuilder.Infrastructure/SlideBuilder.Infrastructure.csproj
- [ ] T002 [P] Add Polly package for retry policies to backend/src/SlideBuilder.Infrastructure/SlideBuilder.Infrastructure.csproj
- [ ] T003 [P] Configure SQLite connection string in backend/src/SlideBuilder.Api/appsettings.Development.json
- [ ] T004 Create OutlineRevision entity in backend/src/SlideBuilder.Core/Domain/Entities.cs
- [ ] T005 [P] Create ConversationSummary entity in backend/src/SlideBuilder.Core/Domain/Entities.cs
- [ ] T006 Add OutlineRevisions and ConversationSummaries DbSets to backend/src/SlideBuilder.Infrastructure/Persistence/SlideBuilderDbContext.cs
- [ ] T007 Configure entity relationships and indexes in backend/src/SlideBuilder.Infrastructure/Persistence/SlideBuilderDbContext.cs OnModelCreating method
- [ ] T008 Create EF Core migration AddConversationalOutlineEntities in backend/src/SlideBuilder.Api
- [ ] T009 Apply database migration to create new tables

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core AI integration and conversation infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T010 Create OutlineProposal DTO in backend/src/SlideBuilder.Core/AI/Models/OutlineProposal.cs
- [ ] T011 [P] Create SlideProposal DTO in backend/src/SlideBuilder.Core/AI/Models/SlideProposal.cs
- [ ] T012 [P] Create OutlineEditCommand DTO in backend/src/SlideBuilder.Core/AI/Models/OutlineEditCommand.cs
- [ ] T013 [P] Create EditOperation enum in backend/src/SlideBuilder.Core/AI/Models/OutlineEditCommand.cs
- [ ] T014 Create IConversationRepository interface in backend/src/SlideBuilder.Core/Persistence/IConversationRepository.cs
- [ ] T015 Implement ConversationRepository with EF Core in backend/src/SlideBuilder.Infrastructure/Persistence/ConversationRepository.cs
- [ ] T016 Register ConversationRepository in dependency injection in backend/src/SlideBuilder.Api/Program.cs
- [ ] T017 Create ContextWindowManager class in backend/src/SlideBuilder.Core/AI/Prompts/ContextWindowManager.cs
- [ ] T018 Implement BuildContextForAI method with sliding window logic in ContextWindowManager
- [ ] T019 Implement conversation summarization logic in ContextWindowManager
- [ ] T020 Create OutlinePromptBuilder class in backend/src/SlideBuilder.Core/AI/Prompts/OutlinePromptBuilder.cs
- [ ] T021 Implement BuildInitialGenerationPrompt method in OutlinePromptBuilder
- [ ] T022 [P] Implement BuildEditRequestPrompt method in OutlinePromptBuilder
- [ ] T023 [P] Implement BuildApprovalPrompt method in OutlinePromptBuilder
- [ ] T024 Configure Polly retry policy for IModelClient in backend/src/SlideBuilder.Api/Program.cs (3 retries, exponential backoff)
- [ ] T025 Add AiSettings configuration properties: MaxRetries, TimeoutSeconds, ContextWindowSize, SummaryThreshold in backend/src/SlideBuilder.Core/Configuration/AiSettings.cs
- [ ] T026 Create SendMessageRequest DTO in backend/src/SlideBuilder.Api/Contracts/ConversationDtos.cs
- [ ] T027 [P] Create ConversationMessageDto DTO in backend/src/SlideBuilder.Api/Contracts/ConversationDtos.cs
- [ ] T028 [P] Create OutlineUpdatedEvent DTO in backend/src/SlideBuilder.Api/Contracts/ConversationDtos.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - AI-generated initial outline from user description (Priority: P1) 🎯 MVP

**Goal**: Enable users to create a complete presentation outline by describing their topic in natural language, receiving AI-generated slide titles and key points

**Independent Test**: Create a new project, send a message like "Create a 10-slide presentation about climate change impacts for high school students", verify AI returns a structured outline with 10 slides containing titles and key points

### Implementation for User Story 1

- [ ] T029 [US1] Create ConversationController in backend/src/SlideBuilder.Api/Controllers/ConversationController.cs
- [ ] T030 [US1] Implement GET /api/projects/{projectId}/conversation/messages endpoint with pagination in ConversationController
- [ ] T031 [US1] Implement POST /api/projects/{projectId}/conversation/messages endpoint in ConversationController
- [ ] T032 [US1] Add message validation logic (max 10,000 chars, non-empty) in ConversationController
- [ ] T033 [US1] Implement job creation and queueing for DraftOutlineStage in ConversationController.SendMessage
- [ ] T034 [US1] Enhance DraftOutlineStage in backend/src/SlideBuilder.Core/Jobs/Stages/DraftOutlineStage.cs to load conversation messages
- [ ] T035 [US1] Implement AI context building using ContextWindowManager in DraftOutlineStage
- [ ] T036 [US1] Implement AI model call using OutlinePromptBuilder.BuildInitialGenerationPrompt in DraftOutlineStage
- [ ] T037 [US1] Implement OutlineProposal parsing and validation in DraftOutlineStage
- [ ] T038 [US1] Implement outline creation from OutlineProposal in DraftOutlineStage
- [ ] T039 [US1] Create OutlineRevision with RevisionNumber=1 when outline is created in DraftOutlineStage
- [ ] T040 [US1] Create AI assistant message with outline proposal in DraftOutlineStage
- [ ] T041 [US1] Extend JobsHub in backend/src/SlideBuilder.Api/Hubs/JobsHub.cs with OutlineUpdated event method
- [ ] T042 [US1] Implement SignalR OutlineUpdated event broadcast in DraftOutlineStage
- [ ] T043 [US1] Implement SignalR MessageReceived event broadcast in DraftOutlineStage
- [ ] T044 [P] [US1] Install @microsoft/signalr package in frontend (verify version 10.0.0)
- [ ] T045 [P] [US1] Create conversationService.ts API client in frontend/src/services/conversationService.ts
- [ ] T046 [US1] Implement getMessages API call in conversationService
- [ ] T047 [US1] Implement sendMessage API call in conversationService
- [ ] T048 [US1] Create conversationStore.ts Pinia store in frontend/src/stores/conversationStore.ts
- [ ] T049 [US1] Implement messages state and isSending state in conversationStore
- [ ] T050 [US1] Implement loadMessages action in conversationStore
- [ ] T051 [US1] Implement sendMessage action with optimistic update in conversationStore
- [ ] T052 [US1] Enhance signalrService.ts in frontend/src/services/signalrService.ts to subscribe to OutlineUpdated event
- [ ] T053 [US1] Implement OutlineUpdated handler to update outlineStore in signalrService
- [ ] T054 [US1] Implement MessageReceived handler to add message to conversationStore in signalrService
- [ ] T055 [P] [US1] Create ConversationPanel.vue component in frontend/src/components/ConversationPanel.vue
- [ ] T056 [P] [US1] Create MessageList.vue component in frontend/src/components/MessageList.vue
- [ ] T057 [P] [US1] Create MessageInput.vue component in frontend/src/components/MessageInput.vue
- [ ] T058 [US1] Implement message rendering (user/assistant/system roles) in MessageList.vue
- [ ] T059 [US1] Implement send button with disabled state during processing in MessageInput.vue
- [ ] T060 [US1] Integrate ConversationPanel into ProjectView.vue in frontend/src/views/ProjectView.vue
- [ ] T061 [US1] Connect ConversationPanel to conversationStore in ConversationPanel.vue
- [ ] T062 [US1] Implement automatic scrolling to latest message in MessageList.vue

**Checkpoint**: At this point, User Story 1 should be fully functional - users can describe a presentation topic and receive an AI-generated outline with slides and key points

---

## Phase 4: User Story 2 - Conversational outline refinement (Priority: P2)

**Goal**: Enable users to modify outlines conversationally (add, remove, reorder, modify slides) through natural language requests

**Independent Test**: Generate an initial outline, send conversational edit requests like "add a slide about solar energy after slide 3" or "remove slide 7", verify outline updates correctly while preserving unaffected slides

### Implementation for User Story 2

- [ ] T063 [P] [US2] Implement BuildEditRequestPrompt with edit operation detection in OutlinePromptBuilder
- [ ] T064 [US2] Enhance DraftOutlineStage to detect edit requests vs initial generation in ExecuteAsync
- [ ] T065 [US2] Implement OutlineEditCommand parsing from AI response in DraftOutlineStage
- [ ] T066 [US2] Implement Insert operation logic (add slide at position) in DraftOutlineStage
- [ ] T067 [P] [US2] Implement Delete operation logic (remove slide, renumber) in DraftOutlineStage
- [ ] T068 [P] [US2] Implement Reorder operation logic (swap/move slides) in DraftOutlineStage
- [ ] T069 [P] [US2] Implement Modify operation logic (edit slide title/keyPoints) in DraftOutlineStage
- [ ] T070 [US2] Implement OutlineRevision creation with incremented RevisionNumber for edits in DraftOutlineStage
- [ ] T071 [US2] Link OutlineRevision.TriggeredByMessageId to the user message that caused the edit in DraftOutlineStage
- [ ] T072 [US2] Calculate changedSlideIndices for OutlineUpdated event in DraftOutlineStage
- [ ] T073 [US2] Implement ambiguous request detection (NeedsClarification flag) in DraftOutlineStage
- [ ] T074 [US2] Handle clarification questions from AI in DraftOutlineStage (store as assistant message, don't modify outline)
- [ ] T075 [US2] Enhance OutlineDisplay.vue in frontend/src/components/OutlineDisplay.vue to highlight changed slides
- [ ] T076 [US2] Implement visual diff animation using changedSlideIndices in OutlineDisplay.vue
- [ ] T077 [US2] Add CSS classes for highlighting changed slides in OutlineDisplay.vue

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - users can create outlines and refine them conversationally

---

## Phase 5: User Story 3 - Conversational outline confirmation and approval (Priority: P3)

**Goal**: Enable users to explicitly approve outlines through conversation, changing status to Approved and enabling progression to slide generation

**Independent Test**: Create and refine an outline, send approval message like "approve this outline" or "looks good", verify outline status changes to Approved and system confirms next steps

### Implementation for User Story 3

- [ ] T078 [US3] Implement BuildApprovalPrompt in OutlinePromptBuilder
- [ ] T079 [US3] Implement approval intent detection in DraftOutlineStage
- [ ] T080 [US3] Implement Approve operation in OutlineEditCommand enum handling in DraftOutlineStage
- [ ] T081 [US3] Implement outline status transition from Draft to Approved in DraftOutlineStage
- [ ] T082 [US3] Create system message confirming approval in DraftOutlineStage
- [ ] T083 [US3] Implement enforcement that GenerateSlidesStage cannot proceed unless outline status is Approved in backend/src/SlideBuilder.Core/Jobs/Stages/GenerateSlidesStage.cs
- [ ] T084 [US3] Implement approved outline editing warning logic in DraftOutlineStage
- [ ] T085 [US3] When editing approved outline, create new Draft outline and preserve approved version in DraftOutlineStage
- [ ] T086 [US3] Display approval status in frontend OutlineDisplay.vue
- [ ] T087 [US3] Show warning message when attempting to edit approved outline in ConversationPanel.vue
- [ ] T088 [US3] Display confirmation message from AI about next steps after approval in MessageList.vue

**Checkpoint**: All core conversational outline features (create, refine, approve) are now independently functional

---

## Phase 6: User Story 4 - Conversation history and context preservation (Priority: P4)

**Goal**: Ensure all outline-related conversations are permanently saved and visible, with full restoration on project reopen

**Independent Test**: Have a multi-turn conversation to create and refine an outline, close browser, reopen project, verify full conversation history and outline state are restored

### Implementation for User Story 4

- [ ] T089 [US4] Implement conversation history retrieval on project open in conversationStore.loadMessages
- [ ] T090 [US4] Implement SignalR reconnection with automatic JoinProjectGroup call in signalrService
- [ ] T091 [US4] Implement re-fetch of current outline on reconnect in frontend/src/services/signalrService.ts
- [ ] T092 [US4] Add conversation history panel with scrollable message list in ConversationPanel.vue
- [ ] T093 [US4] Display message timestamps in MessageList.vue
- [ ] T094 [US4] Implement visual linking between messages and outline revisions in MessageList.vue
- [ ] T095 [US4] Add revision history view showing which messages triggered which outline changes in OutlineDisplay.vue

**Checkpoint**: Conversation history and context preservation complete - users can review full dialogue and resume work after browser restart

---

## Phase 7: User Story 5 - Real-time outline updates during conversation (Priority: P5)

**Goal**: Provide real-time visual feedback when AI modifies the outline, with immediate display updates and change highlighting

**Independent Test**: Request an outline change conversationally, verify outline display updates automatically without refresh, with visual indication of changed slides

### Implementation for User Story 5

- [ ] T096 [US5] Implement real-time outline state update in outlineStore when OutlineUpdated event received
- [ ] T097 [US5] Implement changed slide highlighting animation in OutlineDisplay.vue
- [ ] T098 [US5] Add loading state display during AI processing in ConversationPanel.vue
- [ ] T099 [US5] Implement JobProgressUpdated event handler for DraftOutlineStage in signalrService
- [ ] T100 [US5] Display "Generating outline..." message during AI processing in ConversationPanel.vue
- [ ] T101 [US5] Handle connection state (connecting, connected, reconnecting, disconnected) in signalrService
- [ ] T102 [US5] Display connection status indicator in ConversationPanel.vue
- [ ] T103 [US5] Implement error state display for failed AI requests in MessageList.vue
- [ ] T104 [US5] Add manual retry button for failed requests in MessageInput.vue
- [ ] T105 [US5] Implement optimistic UI update rollback on error in conversationStore

**Checkpoint**: All user stories complete with real-time updates - full conversational outline generation experience is functional

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and production readiness

- [ ] T106 [P] Add comprehensive error messages for AI service failures with retry guidance in DraftOutlineStage
- [ ] T107 [P] Implement logging for all conversation operations in ConversationController
- [ ] T108 [P] Implement logging for all AI operations in DraftOutlineStage
- [ ] T109 [P] Add request/response validation with detailed error messages in ConversationController
- [ ] T110 Update CLAUDE.md with conversational outline generation commands and patterns
- [ ] T111 Add User Secrets configuration documentation to quickstart.md
- [ ] T112 [P] Implement UI polish: loading spinners, animations, transitions in ConversationPanel.vue
- [ ] T113 [P] Implement responsive design for conversation panel in ConversationPanel.vue
- [ ] T114 Add keyboard shortcuts (Enter to send, Shift+Enter for newline) in MessageInput.vue
- [ ] T115 Implement message character counter (max 10,000) in MessageInput.vue
- [ ] T116 Add accessibility attributes (ARIA labels, roles) to conversation UI components
- [ ] T117 Validate quickstart.md instructions by following setup steps
- [ ] T118 Code review and refactoring for clarity and maintainability
- [ ] T119 Performance testing: verify <30s AI response time and <2s outline display update

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - User stories can proceed in parallel (if staffed) or sequentially by priority
  - US2 depends on US1 (needs outline to edit)
  - US3 depends on US1 (needs outline to approve)
  - US4 is independent (conversation history infrastructure)
  - US5 is independent (real-time updates infrastructure)
- **Polish (Phase 8)**: Depends on desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Depends on User Story 1 (needs outline creation to work before editing)
- **User Story 3 (P3)**: Depends on User Story 1 (needs outline to approve) - Can be developed in parallel with US2
- **User Story 4 (P4)**: Can start after Foundational (Phase 2) - Independent of other stories
- **User Story 5 (P5)**: Can start after Foundational (Phase 2) - Independent of other stories, but enhanced by US1

### Within Each User Story

- Backend API endpoints before frontend services
- Pinia stores before Vue components
- Core components (ConversationPanel structure) before child components (MessageList, MessageInput)
- SignalR event handlers before UI components that consume events
- Data flow: Backend → API → Service → Store → Component

### Parallel Opportunities

- **Phase 1 (Setup)**: T001, T002, T003 can run in parallel; T005 parallel with T004; After T006, T007 can run
- **Phase 2 (Foundational)**: T010-T013 (DTOs) all parallel; T021-T023 (OutlinePromptBuilder methods) all parallel; T026-T028 (Event DTOs) all parallel
- **User Story 1**: T044, T045, T055-T057 (frontend components) can run in parallel; T052-T054 (SignalR handlers) can run in parallel
- **User Story 2**: T067-T069 (edit operations) all parallel
- **Phase 8 (Polish)**: T106-T109, T112-T113, T116 can all run in parallel

---

## Parallel Example: User Story 1 Backend

```bash
# After foundational infrastructure is ready, these backend tasks can proceed:
# First wave (DTOs - all parallel):
Task T029: Create ConversationController
Task T044: Install SignalR package
Task T045: Create conversationService.ts

# Second wave (components - all parallel after T055):
Task T055: Create ConversationPanel.vue
Task T056: Create MessageList.vue
Task T057: Create MessageInput.vue
```

---

## Parallel Example: Phase 2 Foundational

```bash
# AI Models (all parallel):
Task T010: Create OutlineProposal DTO
Task T011: Create SlideProposal DTO
Task T012: Create OutlineEditCommand DTO
Task T013: Create EditOperation enum

# OutlinePromptBuilder methods (all parallel after T020):
Task T021: Implement BuildInitialGenerationPrompt
Task T022: Implement BuildEditRequestPrompt
Task T023: Implement BuildApprovalPrompt

# Event DTOs (all parallel):
Task T026: Create SendMessageRequest DTO
Task T027: Create ConversationMessageDto DTO
Task T028: Create OutlineUpdatedEvent DTO
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T009)
2. Complete Phase 2: Foundational (T010-T028) - CRITICAL blocker
3. Complete Phase 3: User Story 1 (T029-T062)
4. **STOP and VALIDATE**: Test conversation-based outline generation independently
5. Deploy/demo MVP with just initial outline generation capability

**MVP Delivers**: Users can create presentation outlines by describing their topic in natural language and receiving AI-generated structured outlines.

### Incremental Delivery

1. **Foundation** (Phases 1-2): Setup + AI infrastructure → Foundation ready
2. **MVP** (Phase 3: US1): Initial outline generation → Test independently → Deploy/Demo
3. **Enhancement 1** (Phase 4: US2): Conversational refinement → Test independently → Deploy/Demo
4. **Enhancement 2** (Phase 5: US3): Approval workflow → Test independently → Deploy/Demo
5. **Enhancement 3** (Phases 6-7: US4-US5): History persistence + Real-time updates → Deploy/Demo
6. **Polish** (Phase 8): Production hardening → Final release

Each phase adds incremental value without breaking previous functionality.

### Parallel Team Strategy

With multiple developers:

1. **Team completes Setup + Foundational together** (Phases 1-2)
2. **Once Foundational is done**:
   - Developer A: User Story 1 (Phase 3) - Core conversation + outline generation
   - Developer B: User Story 4 (Phase 6) - History persistence (independent)
   - Developer C: User Story 5 (Phase 7) - Real-time updates (independent)
3. **After US1 complete**:
   - Developer A: User Story 2 (Phase 4) - Outline editing (depends on US1)
   - Developer B: User Story 3 (Phase 5) - Approval workflow (depends on US1)
4. **Final integration** (Phase 8): Team collaboration on polish

---

## Task Summary

**Total Tasks**: 119

**Tasks by Phase**:
- Phase 1 (Setup): 9 tasks
- Phase 2 (Foundational): 19 tasks
- Phase 3 (User Story 1 - P1): 34 tasks
- Phase 4 (User Story 2 - P2): 15 tasks
- Phase 5 (User Story 3 - P3): 11 tasks
- Phase 6 (User Story 4 - P4): 7 tasks
- Phase 7 (User Story 5 - P5): 10 tasks
- Phase 8 (Polish): 14 tasks

**Parallel Tasks**: 43 tasks marked with [P] flag

**MVP Scope** (Recommended):
- Phase 1 (Setup): 9 tasks
- Phase 2 (Foundational): 19 tasks
- Phase 3 (User Story 1): 34 tasks
- **Total MVP**: 62 tasks

**Independent Test Criteria**:
- **US1**: Create project, describe topic, receive AI outline with titles and key points
- **US2**: Generate outline, send edit request, verify correct modification preserving other slides
- **US3**: Refine outline, send approval, verify status changes to Approved
- **US4**: Create conversation, close browser, reopen, verify full history restored
- **US5**: Request outline change, verify real-time display update with highlighting

---

## Notes

- [P] tasks = different files, no dependencies - can run in parallel
- [Story] label maps task to specific user story for traceability (US1-US5)
- Each user story should be independently completable and testable
- Tests are NOT included per feature specification (no TDD requirement)
- Commit after each task or logical group of parallel tasks
- Stop at any checkpoint to validate story independently
- Suggested MVP: Phases 1-3 only (62 tasks) delivers core conversational outline generation
- Full feature: All phases (119 tasks) delivers complete conversational outline workflow with refinement, approval, history, and real-time updates
