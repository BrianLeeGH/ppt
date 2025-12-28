
# Feature Specification: AI Slide HTML Presentation Builder

**Feature Branch**: `001-ai-slide-builder`  
**Created**: 2025-12-28  
**Status**: Draft  
**Input**: User description: "我想建立一个 ppt 生成系统，不过并非生成 ppt 的，而是生成 html 网页的，通过和 ai 对话的方式，创建一个可用于展示的 slide 形式的 html 网页，我希望 ai 生成 css,js,pug，然后再编译成 html 网页，项目分前后端，以 project 或者文件夹形式进行管理。当然也可以放置一些 图片，让 ai 帮忙加插到 ppt 中，我希望通过 oss 或者 s3 用来存储这些文档。"

## Clarifications

### Session 2025-12-28

- Q: Frontend/backend tech direction? → A: Frontend uses Vue (with Pinia). Backend uses .NET (targeting .NET 10) and focuses on managing generation state.
- Q: “Stop/continue” semantics? → A: Primary goal is cross-session continuity: user can close browser/computer and later resume from the latest persisted progress; MVP does not require fine-grained cancellation mid-token.
- Q: Model endpoint choice? → A: Use an OpenAI-compatible API surface so models can be swapped by configuration.
- Q: Agent framework direction? → A: Start with Microsoft.Agent.Framework as the orchestration layer; if it proves unsuitable, fall back to a simpler workflow/state-machine approach.
- Q: MVP generation workflow granularity? → A: Use 4–6 coarse stages with persistence at stage boundaries (DraftOutline → OutlineApproved → GenerateSlides → CompilePreview → Export/UploadAssets optional).
- Q: Preview updates + compilation choice? → A: Use push-style real-time progress updates and compile for preview so the preview matches the final output.
- Q: New requirement: style definition/confirmation? → A: Introduce a structured Style Brief with explicit confirmation and conflict-resolution; it must guide CSS/JS generation and be editable independently of slide content.
- Q: Storage provider preference? → A: Implement OSS first (not S3), but keep storage abstracted so switching to S3 later is low-friction.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Align on outline before generation (Priority: P1)

As a creator, I want the assistant to propose an outline (slide list) and confirm it with me before generating full slides so that the result matches my intent with minimal rework.

**Why this priority**: Slide decks require strong structure; confirming an outline first is the fastest way to converge on the right content.

**Independent Test**: Can be fully tested by creating a project, reaching an approved outline, and verifying the outline drives what slides exist.

**Acceptance Scenarios**:

1. **Given** a new project, **When** I describe a topic and goal, **Then** the system proposes an outline that includes slide titles and 2–5 key points per slide.
2. **Given** a proposed outline, **When** I request outline edits (add/remove/reorder/rename slides or change key points), **Then** the outline updates and the system re-summarizes the updated outline for confirmation.
3. **Given** a proposed outline, **When** I confirm the outline, **Then** the outline is marked as approved and becomes the basis for generating or updating slides.
4. **Given** I explicitly choose to skip outlining, **When** I ask the system to generate immediately, **Then** the system proceeds but still presents a derived outline for quick verification.

---

### User Story 2 - Define and confirm presentation style (Priority: P2)

As a creator, I want to define and confirm the presentation style (theme) before or during generation so that the produced CSS/JS yields a coherent, intentional visual and motion style.

**Why this priority**: Slide decks are highly sensitive to style; without an agreed style, CSS/JS generation becomes inconsistent and harder to iterate.

**Independent Test**: Can be tested by selecting/confirming a style brief, generating slides, and verifying that appearance and transitions are consistent with that style.

**Acceptance Scenarios**:

1. **Given** a new project, **When** I describe my audience and tone, **Then** the system proposes a concise style brief for confirmation (e.g., visual tone, emphasis level, and transition feel).
2. **Given** a proposed style brief, **When** I adjust it (e.g., “more minimal”, “more playful”, “increase contrast”, “bigger headings”), **Then** the system updates and re-summarizes the brief for confirmation.
3. **Given** a confirmed style brief, **When** I generate slides or regenerate slides, **Then** the generated output follows the confirmed style consistently across slides.
4. **Given** an existing deck with a confirmed style, **When** I request a style change, **Then** the system summarizes the impact (what will change) and asks for confirmation before applying it.

---

### User Story 3 - Generate slides from an approved outline (Priority: P3)

As a creator, once the outline is approved, I want the assistant to generate slides in a controlled way so that I can review, refine, and keep the deck coherent.

**Why this priority**: The deck must be coherent across slides; generation should preserve structure and allow targeted edits without unintended changes.

**Independent Test**: Can be tested by approving an outline, generating a deck, applying a targeted edit to a single slide, and confirming only intended parts change.

**Acceptance Scenarios**:

1. **Given** an approved outline, **When** I request slide generation, **Then** the system produces slides that match the outline order and topics.
2. **Given** a generated deck, **When** I ask to regenerate or rewrite a specific slide, **Then** only that slide changes unless I explicitly request wider changes.
3. **Given** a generated deck, **When** I ask for a global adjustment (tone, detail level, audience), **Then** the system explains which slides will change and applies the update consistently.
4. **Given** a deck preview, **When** I navigate slides (next/previous), **Then** the presentation behaves like a slide show (one slide visible at a time) and navigation is consistent.

---

### User Story 4 - Export a self-contained presentation (Priority: P4)

As a creator, I want to export a presentation that can be hosted or opened for display so that I can share it or present it without the authoring tool.

**Why this priority**: A deck is only useful if it can be displayed outside the tool.

**Independent Test**: Can be tested by exporting a deck and opening the exported output in a standard browser environment.

**Acceptance Scenarios**:

1. **Given** a deck with multiple slides, **When** I export for presentation, **Then** the export opens as a slide-style web page without requiring the authoring UI.
2. **Given** an export, **When** I open it on a different machine/environment, **Then** all slides render correctly and required assets resolve.

---

### User Story 5 - Manage and insert images/assets (Priority: P5)

As a creator, I want to provide images (upload or link) and ask the assistant to place them into slides so that I can build visual presentations.

**Why this priority**: Visual assets are essential for slide presentations; asset support is the next most valuable slice after basic deck creation.

**Independent Test**: Can be tested by adding an image asset, inserting it into a slide via chat instruction, and verifying it appears in preview and export.

**Acceptance Scenarios**:

1. **Given** an image asset is available to the project, **When** I ask to insert it into a specific slide, **Then** the slide includes the image in the specified location/role (e.g., background, illustration, or figure).
2. **Given** an image URL is invalid or unreachable, **When** I attempt to add it, **Then** the system reports the failure and does not break the deck preview.

### Edge Cases

- What happens when the assistant produces malformed presentation content (e.g., missing slides, unreadable markup)?
- What happens when the outline is approved but later changes conflict with already-refined slides?
- What happens when a user request is ambiguous (e.g., “make it more professional”) and could apply to structure, tone, or visuals?
- What happens when user style requests conflict (e.g., “minimal” but also “lots of decorative elements”)?
- What happens when a style change makes text hard to read (e.g., low contrast) in preview?
- What happens when exported output references missing assets?
- How does the system behave when an image is extremely large or an unsupported format?
- What happens when storage is temporarily unavailable during upload/download?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow users to create, open, rename, and delete presentation projects.
- **FR-002**: System MUST maintain a slide deck within a project, including slide creation, deletion, reordering, and duplication.
- **FR-003**: System MUST provide a conversational authoring workflow where user messages can request slide changes (e.g., add slides, rewrite slide content, adjust structure).
- **FR-004**: System MUST provide an interactive preview that displays the deck in a slide-show format with next/previous navigation.
- **FR-005**: System MUST allow exporting a presentation as a displayable web format that can be opened without the authoring interface.
- **FR-006**: System MUST preserve a project’s source assets (presentation content, styling, and behavior definitions) in a structured, inspectable form so creators can review and iterate.
- **FR-007**: System MUST support adding image assets to a project via file upload and via remote URL import.
- **FR-008**: System MUST allow the assistant (or user) to reference existing project assets when composing slides.
- **FR-009**: System MUST store exported outputs and referenced assets in OSS object storage for MVP.
- **FR-030**: Storage access MUST be abstracted behind a provider interface so OSS can later be replaced with S3 without changing domain logic.
- **FR-010**: System MUST allow users to retrieve and re-open previously saved projects and their assets.
- **FR-011**: System MUST handle storage failures gracefully by surfacing actionable error messages and keeping existing project data intact.
- **FR-012**: System MUST keep an auditable history of the conversation and resulting deck revisions for a project (at minimum: timestamps, user requests, and resulting deck version identifier).
- **FR-013**: System MUST support an explicit outline stage that represents the planned slide list (ordered slides with titles and key points).
- **FR-014**: System MUST request user confirmation of the outline before generating full slide content by default.
- **FR-015**: System MUST allow users to edit the outline via conversation (add/remove/reorder/rename slides and adjust key points) and re-confirm.
- **FR-016**: System MUST support targeted edits (single slide rewrite/regenerate) without changing other slides unless the user explicitly requests a broader change.
- **FR-017**: When a request would change multiple slides or restructure the deck, the system MUST summarize the expected impact and request confirmation before applying the change.
- **FR-018**: System MUST capture and display the deck’s declared intent (e.g., audience, duration, tone, and goal) and allow users to update it; updates MUST influence future generations and edits.
- **FR-019**: If critical intent information is missing, the system MUST either propose reasonable defaults and ask for confirmation or ask focused questions before continuing.
- **FR-020**: System MUST support a style brief (theme) that captures presentation style choices in a structured, editable form.
- **FR-021**: System MUST request user confirmation of the style brief by default before generating full slide styling/behavior.
- **FR-022**: System MUST apply the confirmed style brief consistently across slide generation, preview, and export.
- **FR-023**: When a style change would modify multiple slides or global presentation behavior, the system MUST summarize expected impact and request confirmation before applying it.
- **FR-024**: System MUST allow updating the style brief and regenerating styling/behavior without requiring users to rewrite slide content unless explicitly requested.
- **FR-025**: System MUST represent generation as a durable, resumable job with coarse-grained stages (at minimum: DraftOutline, OutlineApproved, GenerateSlides, CompilePreview; optional: Export, UploadAssets).
- **FR-026**: System MUST persist progress at stage boundaries so that users can close their browser/computer and later continue from the latest completed stage.
- **FR-027**: System MUST display the latest known job stage, last update time, and any errors when a project is reopened.
- **FR-028**: System MUST allow users to stop a job in MVP terms by preventing further stages from running; the system MAY allow finer-grained interruption but it is not required for MVP.
- **FR-029**: System MUST allow users to adjust prompts/inputs (e.g., outline, style brief, or intent) and start a new job while retaining prior job outputs for comparison.

### Assumptions

- The initial release targets a single user workspace per installation (multi-user collaboration and complex permissions are out of scope for MVP).
- Object storage is available via an OSS provider for MVP.
- A storage abstraction layer exists so the implementation can later switch providers (e.g., to S3) with minimal changes.
- Presentations are intended for modern browsers; legacy browser support is out of scope unless explicitly added later.

### Dependencies

- Access to an AI assistant capability to interpret user intent and produce structured slide/deck changes.
- Access to object storage credentials/policies that allow upload, download, and listing of project artifacts and assets.
- A hosting or distribution mechanism for exported presentations (e.g., local file distribution or a web host), managed outside this feature unless explicitly included.

### Key Entities *(include if feature involves data)*

- **Project**: A user-managed container for a single presentation; includes name, timestamps, and ownership scope.
- **Deck**: The ordered collection of slides belonging to a project; includes theme/style intent and export settings.
- **Outline**: The planned structure of the deck (slide order, titles, and key points) that is reviewed and approved before full generation.
- **Style Brief**: The confirmed presentation style definition (e.g., visual tone, typography emphasis, spacing feel, contrast preference, and transition/motion feel) used to guide CSS/JS generation.
- **Slide**: A single unit in the deck; includes layout intent, content blocks (text/media), and notes (optional).
- **Asset**: A binary or external resource (e.g., image) associated with a project; includes type, source (upload/URL), and usage references.
- **Conversation**: The sequence of user/assistant messages tied to a project.
- **Revision**: A saved state of the deck at a point in time; linked to the conversation event(s) that produced it.
- **Job**: A durable record of an in-progress or completed generation run for a project; includes current stage, status, timestamps, and links to produced outputs.
- **Checkpoint**: The last completed stage/output snapshot for a job, used to resume work across sessions.
- **Export**: A generated, viewable snapshot of a deck intended for presentation or sharing; references a specific revision.

## Style Brief Definition (guides CSS/JS generation)

The Style Brief is a structured, confirmable set of choices. The system MUST present it in a concise summary and require explicit confirmation by default.

### Style Brief Fields (MVP)

- **Visual Tone**: One of: Minimal / Corporate / Editorial / Playful / Bold.
- **Audience Context**: One short phrase (e.g., “product managers”, “investors”, “engineers”).
- **Information Density**: Low / Medium / High (controls how much text per slide is acceptable).
- **Emphasis Level**: Calm / Balanced / Punchy (how strongly headings and key numbers are highlighted).
- **Contrast Preference**: Normal / High (readability-first option).
- **Typography Hierarchy**: H1-dominant / Balanced / Body-dominant (relative emphasis of headings vs body).
- **Layout Rhythm**: Spacious / Compact (whitespace preference).
- **Motion Feel**: None / Subtle / Dynamic (guides transitions and micro-interactions).
- **Media Treatment**: Sparse / Balanced / Visual-heavy (frequency and role of images).
- **Brand Constraints (optional)**: Short text constraints such as “avoid bright colors”, “use monochrome”, or “keep it formal”.

### Confirmation Rules

- By default, the system MUST not finalize CSS/JS generation until the Style Brief is confirmed.
- If the user provides only vague style direction (e.g., “more professional”), the system MUST map it into Style Brief fields and ask for confirmation.
- If user requirements conflict (e.g., Visual Tone=Minimal but Media Treatment=Visual-heavy), the system MUST surface the conflict and ask which dimension wins.

### How Style Brief Constrains Generation

- The system MUST treat the Style Brief as global guidance for all slides, including newly generated slides and regenerated slides.
- The system MUST allow changing Style Brief independently from slide content, and applying it should primarily affect styling/behavior rather than rewriting content.
- Any request that would change global look/feel (e.g., contrast, motion, rhythm) MUST be treated as a style change and follow the impact-summary + confirmation rule.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can reach an approved outline for a 10-slide deck via chat in under 3 minutes.
- **SC-002**: 95% of exports open and render correctly in a standard modern browser environment on first attempt.
- **SC-003**: From opening a project, the first slide becomes viewable in under 3 seconds for 90% of sessions with typical deck sizes (≤ 20 slides, ≤ 30 images).
- **SC-004**: A user can create a new project, generate a 10-slide deck from an approved outline, and reach a presentable preview in under 10 minutes.
- **SC-005**: 90% of users can complete the primary workflow (outline → generate → preview → export) without external help.
- **SC-006**: Storage-related failures present an actionable message (what failed + suggested next step) in 100% of observed failure cases.
- **SC-007**: For targeted slide edits, 90% of the time only the requested slide changes (no unintended changes to other slides), as verified by revision comparisons.
- **SC-008**: After confirming a style brief, 90% of slides in a generated deck are judged visually consistent with that style by a simple reviewer checklist (e.g., consistent headings, spacing feel, and transition feel).
