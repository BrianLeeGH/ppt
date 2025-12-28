---

description: "Task list for feature implementation"

---

# Tasks: AI Slide HTML Presentation Builder

**Input**: Design documents from `specs/001-ai-slide-builder/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Not adding automated test tasks (not explicitly requested in spec). Each story includes independent test criteria.

## Format: `[TaskID] [P?] [Story] Description with file path`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., [US1])
- All tasks include concrete file paths

---

## Phase 1: Setup (Shared Infrastructure)

- [ ] T001 Create backend solution skeleton in backend/src/SlideBuilder.sln
- [ ] T002 Create ASP.NET Core Web API project in backend/src/SlideBuilder.Api/SlideBuilder.Api.csproj
- [ ] T003 Create Core class library in backend/src/SlideBuilder.Core/SlideBuilder.Core.csproj
- [ ] T004 Create Infrastructure class library in backend/src/SlideBuilder.Infrastructure/SlideBuilder.Infrastructure.csproj
- [ ] T005 Create backend test project in backend/tests/SlideBuilder.Tests/SlideBuilder.Tests.csproj
- [ ] T006 Create Vue 3 + Vite + TypeScript app in frontend/package.json
- [ ] T007 Add Pinia store setup in frontend/src/app/pinia.ts
- [ ] T008 Add frontend routing shell in frontend/src/app/router.ts
- [ ] T009 Create minimal page layout scaffolding in frontend/src/app/App.vue
- [ ] T010 Create internal Pug compiler tool package in tools/pug-compiler/package.json
- [ ] T011 Add repo-level .editorconfig in .editorconfig
- [ ] T012 Add local dev documentation pointers in specs/001-ai-slide-builder/quickstart.md

---

## Phase 2: Foundational (Blocking Prerequisites)

**⚠️ CRITICAL**: No user story work should begin until this phase is complete.

- [ ] T013 Implement backend configuration binding for AI + OSS settings in backend/src/SlideBuilder.Api/Configuration/AppSettings.cs
- [ ] T014 [P] Implement consistent API error responses middleware in backend/src/SlideBuilder.Api/Middleware/ProblemDetailsMiddleware.cs
- [ ] T015 [P] Implement request logging setup in backend/src/SlideBuilder.Api/Logging/LoggingExtensions.cs
- [ ] T016 Implement EF Core DbContext + SQLite wiring in backend/src/SlideBuilder.Infrastructure/Persistence/SlideBuilderDbContext.cs
- [ ] T017 Add initial EF Core migrations configuration in backend/src/SlideBuilder.Infrastructure/Persistence/Migrations/README.md
- [ ] T018 Create domain entities (Project/Deck/Outline/StyleBrief/Slide/Asset/Job/Checkpoint/Revision/ConversationMessage) in backend/src/SlideBuilder.Core/Domain/
- [ ] T019 Implement repository/unit-of-work primitives in backend/src/SlideBuilder.Infrastructure/Persistence/Repositories/
- [ ] T020 Implement storage provider abstraction interface in backend/src/SlideBuilder.Core/Storage/IObjectStorage.cs
- [ ] T021 Implement OSS storage provider (first provider) in backend/src/SlideBuilder.Infrastructure/Storage/OssObjectStorage.cs
- [ ] T022 Implement AI model client abstraction in backend/src/SlideBuilder.Core/AI/IModelClient.cs
- [ ] T023 Implement OpenAI-compatible model client in backend/src/SlideBuilder.Infrastructure/AI/OpenAiCompatibleModelClient.cs
- [ ] T024 Implement durable job engine primitives (job runner + stage checkpoints) in backend/src/SlideBuilder.Core/Jobs/
- [ ] T025 Implement SignalR hub for job updates in backend/src/SlideBuilder.Api/Hubs/JobsHub.cs
- [ ] T026 Wire DI registrations (DbContext, storage, model client, job engine, SignalR) in backend/src/SlideBuilder.Api/Program.cs
- [ ] T027 Add frontend API client wrapper with base URL config in frontend/src/app/api/client.ts
- [ ] T028 Add frontend realtime client wrapper (SignalR) in frontend/src/app/realtime/jobsHub.ts

**Checkpoint**: Foundation ready — can implement user stories.

---

## Phase 3: User Story 1 - Align on outline before generation (Priority: P1) 🎯 MVP

**Goal**: Create projects, draft/iterate outline, and approve outline as a gated step.

**Independent Test**: Create a project → draft outline → edit outline → approve outline → reopen project and verify approved outline persists.

- [ ] T029 [P] [US1] Add outline models/DTOs for API in backend/src/SlideBuilder.Api/Contracts/OutlineDtos.cs
- [ ] T030 [P] [US1] Implement Project service (create/list/get) in backend/src/SlideBuilder.Core/Services/Projects/ProjectService.cs
- [ ] T031 [US1] Implement Project endpoints in backend/src/SlideBuilder.Api/Controllers/ProjectsController.cs
- [ ] T032 [P] [US1] Implement Outline service (get/update/approve) in backend/src/SlideBuilder.Core/Services/Outlines/OutlineService.cs
- [ ] T033 [US1] Implement Outline endpoints (/outline, /outline/approve) in backend/src/SlideBuilder.Api/Controllers/OutlineController.cs
- [ ] T034 [US1] Enforce outline-approval gate check helper in backend/src/SlideBuilder.Core/Guards/OutlineApprovalGuard.cs
- [ ] T035 [P] [US1] Create Pinia store for projects in frontend/src/stores/projectsStore.ts
- [ ] T036 [P] [US1] Create Pinia store for outline in frontend/src/stores/outlineStore.ts
- [ ] T037 [US1] Implement project list + create page in frontend/src/pages/ProjectsPage.vue
- [ ] T038 [US1] Implement outline editor + approve flow in frontend/src/pages/OutlinePage.vue
- [ ] T039 [US1] Add route wiring for projects + outline pages in frontend/src/app/router.ts

---

## Phase 4: User Story 2 - Define and confirm presentation style (Priority: P2)

**Goal**: Draft/iterate a structured Style Brief and approve it; style changes require impact summary + confirmation.

**Independent Test**: Draft style brief → edit style brief → approve → attempt change and confirm impact gate is triggered.

- [ ] T040 [P] [US2] Add style brief models/DTOs for API in backend/src/SlideBuilder.Api/Contracts/StyleBriefDtos.cs
- [ ] T041 [P] [US2] Implement StyleBrief service (get/update/approve) in backend/src/SlideBuilder.Core/Services/Styles/StyleBriefService.cs
- [ ] T042 [US2] Implement StyleBrief endpoints (/style-brief, /style-brief/approve) in backend/src/SlideBuilder.Api/Controllers/StyleBriefController.cs
- [ ] T043 [US2] Implement style impact-summary generator (diff summary) in backend/src/SlideBuilder.Core/Services/Styles/StyleImpactService.cs
- [ ] T044 [P] [US2] Create Pinia store for style brief in frontend/src/stores/styleBriefStore.ts
- [ ] T045 [US2] Implement style brief editor + approve flow in frontend/src/pages/StyleBriefPage.vue
- [ ] T046 [US2] Add routing for style brief page in frontend/src/app/router.ts

---

## Phase 5: User Story 3 - Generate slides from an approved outline (Priority: P3)

**Goal**: Start a durable job that generates slides from approved outline + style brief, and compile preview.

**Independent Test**: With approved outline + style brief → start job → observe SignalR progress → preview becomes available → targeted slide regeneration affects only one slide.

- [ ] T047 [P] [US3] Define job stage enum + persistence mapping in backend/src/SlideBuilder.Core/Jobs/JobStage.cs
- [ ] T048 [US3] Implement Job API contracts in backend/src/SlideBuilder.Api/Contracts/JobDtos.cs
- [ ] T049 [US3] Implement Job endpoints (/projects/{id}/jobs, /jobs/{id}, /jobs/{id}/stop) in backend/src/SlideBuilder.Api/Controllers/JobsController.cs
- [ ] T050 [US3] Implement job runner stages (DraftOutline/GenerateSlides/CompilePreview) in backend/src/SlideBuilder.Core/Jobs/Stages/
- [ ] T051 [US3] Implement checkpoint persistence at stage boundaries in backend/src/SlideBuilder.Infrastructure/Persistence/JobCheckpointStore.cs
- [ ] T052 [US3] Implement slide generation prompt builder (outline+intent+style) in backend/src/SlideBuilder.Core/AI/Prompts/SlideGenerationPromptBuilder.cs
- [ ] T053 [US3] Implement structured response parsing/validation for model outputs in backend/src/SlideBuilder.Core/AI/Parsing/ModelOutputParser.cs
- [ ] T054 [US3] Implement Pug/CSS/JS artifact schema + persistence in backend/src/SlideBuilder.Core/Artifacts/
- [ ] T055 [US3] Implement preview compilation invocation (calls internal tool) in backend/src/SlideBuilder.Infrastructure/Compilation/PugCompilationService.cs
- [ ] T056 [US3] Implement preview endpoint (/projects/{id}/preview) in backend/src/SlideBuilder.Api/Controllers/PreviewController.cs
- [ ] T057 [P] [US3] Add frontend job status store + subscriptions in frontend/src/stores/jobsStore.ts
- [ ] T058 [US3] Implement generation controls + progress UI in frontend/src/pages/GeneratePage.vue
- [ ] T059 [US3] Implement preview viewer page that loads compiled HTML in frontend/src/pages/PreviewPage.vue
- [ ] T060 [US3] Add routing for generate + preview pages in frontend/src/app/router.ts
- [ ] T061 [US3] Implement targeted slide rewrite endpoint scaffold in backend/src/SlideBuilder.Api/Controllers/SlidesController.cs
- [ ] T062 [US3] Implement targeted slide update workflow in backend/src/SlideBuilder.Core/Services/Slides/SlideEditService.cs

---

## Phase 6: User Story 4 - Export a self-contained presentation (Priority: P4)

**Goal**: Export a presentation that can be opened without the authoring UI.

**Independent Test**: Export latest revision → open export URL (or downloaded file) → slides render and navigate correctly.

- [ ] T063 [US4] Define export artifact format and metadata in backend/src/SlideBuilder.Core/Exports/ExportManifest.cs
- [ ] T064 [US4] Implement export pipeline (compile + package) in backend/src/SlideBuilder.Core/Exports/ExportService.cs
- [ ] T065 [US4] Upload export output to storage provider in backend/src/SlideBuilder.Infrastructure/Exports/ExportStorageWriter.cs
- [ ] T066 [US4] Implement export endpoint (/projects/{id}/export) in backend/src/SlideBuilder.Api/Controllers/ExportController.cs
- [ ] T067 [US4] Implement frontend export trigger + export link UI in frontend/src/pages/ExportPage.vue
- [ ] T068 [US4] Add routing for export page in frontend/src/app/router.ts

---

## Phase 7: User Story 5 - Manage and insert images/assets (Priority: P5)

**Goal**: Add assets via upload/URL and allow insertion into slides; failures are handled gracefully.

**Independent Test**: Upload image → insert into slide → preview shows image → export resolves asset; invalid URL surfaces actionable error.

- [ ] T069 [P] [US5] Define asset API contracts in backend/src/SlideBuilder.Api/Contracts/AssetDtos.cs
- [ ] T070 [US5] Implement asset upload endpoint in backend/src/SlideBuilder.Api/Controllers/AssetsController.cs
- [ ] T071 [US5] Implement asset URL-import endpoint in backend/src/SlideBuilder.Api/Controllers/AssetsController.cs
- [ ] T072 [US5] Implement asset storage key strategy and metadata persistence in backend/src/SlideBuilder.Infrastructure/Storage/AssetStore.cs
- [ ] T073 [P] [US5] Implement frontend asset picker/uploader component in frontend/src/components/AssetUploader.vue
- [ ] T074 [US5] Implement assets page (list + add) in frontend/src/pages/AssetsPage.vue
- [ ] T075 [US5] Implement slide content block support for assets in backend/src/SlideBuilder.Core/Domain/Slides/ContentBlocks.cs
- [ ] T076 [US5] Implement “insert asset into slide” flow in backend/src/SlideBuilder.Core/Services/Slides/SlideAssetInsertService.cs
- [ ] T077 [US5] Update preview compilation to include asset URLs in backend/src/SlideBuilder.Infrastructure/Compilation/PreviewAssetResolver.cs
- [ ] T078 [US5] Add routing for assets page in frontend/src/app/router.ts

---

## Phase 8: Polish & Cross-Cutting Concerns

- [ ] T079 [P] Add end-to-end manual validation checklist in specs/001-ai-slide-builder/quickstart.md
- [ ] T080 Harden configuration validation and startup checks in backend/src/SlideBuilder.Api/Configuration/ValidationExtensions.cs
- [ ] T081 Add storage failure user-facing messages mapping in backend/src/SlideBuilder.Api/Errors/StorageErrorMapper.cs
- [ ] T082 Add "reopen project" status summary endpoint in backend/src/SlideBuilder.Api/Controllers/ProjectStatusController.cs
- [ ] T083 Ensure job resume behavior is implemented (resume from last checkpoint) in backend/src/SlideBuilder.Core/Jobs/JobResumeService.cs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies
- **Foundational (Phase 2)**: Depends on Setup; blocks all user stories
- **User Stories (Phase 3+)**: Depend on Foundational
- **Polish (Phase 8)**: Depends on all desired user stories

### User Story Dependency Graph (recommended MVP order)

- Setup → Foundational → **US1 (Outline)** → **US2 (Style Brief)** → **US3 (Generate + Preview)** → **US4 (Export)** → **US5 (Assets)**

### Parallel Opportunities (examples)

- Within Setup: T002–T010 can be split across different folders (backend/frontend/tools)
- Within Foundational: T014, T015, T020–T023 can be done in parallel (middleware, logging, storage, AI)
- Within US1: backend contracts/services (T029–T033) can proceed alongside frontend stores/pages (T035–T038)
- Within US3: frontend job UI (T057–T060) can proceed alongside backend job stages (T047–T056)

---

## Parallel Example: User Story 1

Example parallelizable work items (not tasks):

1) Backend contracts: backend/src/SlideBuilder.Api/Contracts/OutlineDtos.cs
2) Backend outline service: backend/src/SlideBuilder.Core/Services/Outlines/OutlineService.cs
3) Frontend stores: frontend/src/stores/projectsStore.ts + frontend/src/stores/outlineStore.ts

---

## Implementation Strategy

### MVP First (US1 Only)

- Complete Phase 1–2
- Complete Phase 3 (US1)
- Validate US1 independently via the US1 test criteria above

### Incremental Delivery

- Add US2, validate independently
- Add US3, validate independently
- Add US4, validate independently
- Add US5, validate independently
