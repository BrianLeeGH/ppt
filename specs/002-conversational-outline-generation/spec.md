# Feature Specification: Conversational Outline Generation

**Feature Branch**: `002-conversational-outline-generation`
**Created**: 2025-12-28
**Status**: Draft
**Input**: User description: "目前的功能不太符合我要求,我希望做到的是 outline 风格 ppt 最后都是使用对话的形式产生,但目前并非这样"

## Clarifications

### Session 2025-12-28

- Q: What happens when two conflicting outline edit requests are sent in rapid succession? → A: UI enforces physical queuing - users must cancel the first request before submitting a second; no simultaneous submissions allowed at interaction level.
- Q: What is the retention policy for conversation messages and outline revisions? → A: Permanent retention - all conversation messages and outline revision history are kept indefinitely for full auditability.
- Q: What happens when a user tries to edit an already-approved outline? → A: Create a new draft outline version (preserving the approved version) and warn the user that changes may require regenerating slides.
- Q: What is the retry strategy when AI service fails or times out? → A: System-level configurable automatic retry count; after all retries exhausted, UI displays error message with a "Retry" button allowing user to manually retry after resolving AI service issues.
- Q: How does the system handle very long conversations (100+ messages) for a single outline? → A: Use sliding window strategy - keep recent N messages in full context, summarize earlier messages; always pin task-critical content (current outline state) at top of context to ensure AI has latest outline structure.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - AI-generated initial outline from user description (Priority: P1)

As a creator, when I start a new presentation project, I want to describe my topic and goals in natural language and have the AI propose a complete outline (with slide titles and key points) so that I don't have to manually build the structure from scratch.

**Why this priority**: This is the foundation of conversational outline generation. Without AI-generated initial outlines, users must manually create every slide in a form, which defeats the purpose of AI-assisted authoring.

**Independent Test**: Can be fully tested by creating a project, sending a conversational message describing a presentation topic, and verifying the AI returns a structured outline proposal with slide titles and key points.

**Acceptance Scenarios**:

1. **Given** a new project with no outline, **When** I send a message like "Create a 10-slide presentation about climate change impacts for high school students", **Then** the AI proposes an outline with 10 slides, each with a title and 2-5 key points.
2. **Given** I provide minimal information (e.g., just "presentation about Python"), **When** the AI generates an outline, **Then** it asks clarifying questions about audience, goal, and depth before proposing the outline.
3. **Given** I provide a detailed brief with specific requirements, **When** the AI generates an outline, **Then** the outline reflects those requirements (e.g., specific sections, emphasis areas, target slide count).
4. **Given** the AI proposes an outline, **When** I review it, **Then** the outline is displayed in a clear, structured format showing slide order, titles, and key points for each slide.

---

### User Story 2 - Conversational outline refinement (Priority: P2)

As a creator, after reviewing the AI-proposed outline, I want to request changes conversationally (e.g., "add a slide about renewable energy between slides 3 and 4", "remove the conclusion slide", "make slide 5 more technical") and have the AI update the outline accordingly.

**Why this priority**: Manual form editing contradicts the conversational paradigm. Users should be able to refine outlines through natural dialogue, not by editing text fields.

**Independent Test**: Can be tested by generating an initial outline, sending conversational edit requests, and verifying the outline updates correctly while preserving unaffected slides.

**Acceptance Scenarios**:

1. **Given** an AI-proposed outline, **When** I say "add a slide about solar energy after slide 3", **Then** the AI inserts a new slide with an appropriate title and key points at position 4, renumbering subsequent slides.
2. **Given** an existing outline, **When** I say "remove slide 7", **Then** the AI removes that slide and renumbers the remaining slides.
3. **Given** an existing outline, **When** I say "swap slides 2 and 5", **Then** the AI reorders those slides and updates the outline display.
4. **Given** an existing outline, **When** I say "make slide 4 more detailed", **Then** the AI adds more key points or expands existing key points for that specific slide.
5. **Given** an existing outline, **When** I request a change that affects multiple slides (e.g., "split slide 3 into two slides"), **Then** the AI explains the change and asks for confirmation before applying it.
6. **Given** an ambiguous request (e.g., "improve the middle section"), **When** the AI processes it, **Then** it asks clarifying questions to understand which slides and what type of improvement.

---

### User Story 3 - Conversational outline confirmation and approval (Priority: P3)

As a creator, once I'm satisfied with the outline, I want to explicitly approve it through conversation (e.g., "this looks good, let's proceed" or "approve outline") so that the system knows the structure is finalized and ready for full slide generation.

**Why this priority**: Approval is a critical gate between outlining and generation. Conversational approval is more natural than clicking a button and allows the AI to summarize what will happen next.

**Independent Test**: Can be tested by creating and refining an outline, sending an approval message, and verifying the outline status changes to Approved and the system moves to the next workflow stage.

**Acceptance Scenarios**:

1. **Given** a refined outline that I'm satisfied with, **When** I say "approve this outline" or "looks good, proceed", **Then** the outline status changes to Approved and the AI confirms what happens next (e.g., "Great! I'll now move to style definition").
2. **Given** an outline that needs minor tweaks, **When** I try to approve it but mention a concern (e.g., "approve, but I'm not sure about slide 6"), **Then** the AI offers to refine slide 6 before final approval.
3. **Given** an approved outline, **When** I later request a change, **Then** the AI warns that changing an approved outline may require regenerating slides and asks for confirmation.
4. **Given** I want to skip outline review, **When** I explicitly say "skip outline, generate immediately", **Then** the AI proceeds to generate an outline internally but still shows it briefly for quick verification before continuing.

---

### User Story 4 - Conversation history and context preservation (Priority: P4)

As a creator, I want all my outline-related conversations to be saved and visible so that I can review the history of changes and understand how the outline evolved.

**Why this priority**: Transparency and auditability are important for complex creative work. Users should see the full dialogue that shaped their outline.

**Independent Test**: Can be tested by having a multi-turn conversation to create and refine an outline, then reviewing the conversation history to verify all messages and outline states are preserved.

**Acceptance Scenarios**:

1. **Given** I've had a conversation to create an outline, **When** I view the project's conversation history, **Then** I see all my messages and the AI's responses in chronological order.
2. **Given** I close my browser and reopen the project, **When** I view the conversation, **Then** the full conversation history is restored.
3. **Given** multiple outline revisions through conversation, **When** I review the history, **Then** I can see which messages triggered which outline changes.

---

### User Story 5 - Real-time outline updates during conversation (Priority: P5)

As a creator, when the AI modifies the outline during our conversation, I want to see the outline update in real-time (or immediately after the AI's response) so that I have visual confirmation of changes.

**Why this priority**: Visual feedback is essential for understanding what changed. Users shouldn't have to refresh or navigate away to see outline updates.

**Independent Test**: Can be tested by requesting an outline change conversationally and verifying the outline display updates automatically after the AI responds.

**Acceptance Scenarios**:

1. **Given** an active conversation, **When** the AI proposes or updates an outline, **Then** the outline display panel updates automatically to show the new structure.
2. **Given** I'm viewing the outline and conversation simultaneously, **When** the AI makes a change, **Then** the changed slides are highlighted or animated to draw attention.
3. **Given** a slow or unreliable connection, **When** an outline update is in progress, **Then** the UI shows a loading state and doesn't display partial/corrupted outline data.

---

### Edge Cases

- What happens when the AI proposes an outline with inconsistent slide counts (e.g., user asked for 10 slides but AI generated 8)?
- What happens when a user's conversational request is ambiguous or contradictory (e.g., "add more detail but keep it shorter")?
- What happens when the AI fails to parse a complex outline modification request?
- What happens when outline generation times out or the AI service is unavailable? (Resolved: System auto-retries with configurable count, then shows error with manual retry button)
- What happens when a user approves an outline but the approval fails to save to the backend?
- What happens when two conflicting outline edit requests are sent in rapid succession? (Resolved: UI enforces physical queuing via disabled send button while processing)
- What happens when a user tries to edit an already-approved outline? (Resolved: Create new draft version, preserve approved version, warn about regeneration impact)
- How does the system handle very long conversations (100+ messages) for a single outline? (Resolved: Sliding window with recent messages + summarized history; current outline always pinned in context)
- What happens when a user tries to generate slides before approving an outline?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a conversational interface (chat UI) for outline creation and editing; UI MUST disable message submission while a request is in-flight to enforce sequential processing.
- **FR-002**: System MUST accept natural language descriptions of presentation topics and goals.
- **FR-003**: System MUST use AI to generate an initial outline proposal from user descriptions, including slide titles and 2-5 key points per slide.
- **FR-004**: System MUST display AI-generated outline proposals in a structured, readable format.
- **FR-005**: System MUST accept conversational requests to modify outlines (add slides, remove slides, reorder slides, edit slide titles, edit key points).
- **FR-006**: System MUST apply outline modifications via AI interpretation of natural language requests rather than direct form input.
- **FR-007**: System MUST preserve the outline structure (slide order, titles, key points) across conversation turns.
- **FR-008**: System MUST allow users to approve outlines conversationally (e.g., "approve outline", "this looks good").
- **FR-009**: System MUST change outline status from Draft to Approved upon conversational approval.
- **FR-010**: System MUST persist all conversation messages (user and AI) related to outline creation and editing with permanent retention (no automatic deletion or archival).
- **FR-011**: System MUST display conversation history in chronological order.
- **FR-012**: System MUST restore conversation history and outline state when a project is reopened.
- **FR-013**: System MUST update the outline display in real-time or immediately after AI responses that modify the outline.
- **FR-014**: System MUST handle ambiguous or unclear outline requests by asking clarifying questions before applying changes.
- **FR-015**: System MUST summarize expected changes and request confirmation for modifications that affect multiple slides or significantly alter structure.
- **FR-016**: System MUST provide error messages when outline generation or modification fails, with guidance on how to retry or rephrase; UI MUST include a "Retry" button for manual retry after AI service failures.
- **FR-025**: System MUST implement configurable automatic retry logic for AI service failures (timeout, unavailability) with system-level retry count setting; after exhausting retries, display user-friendly error with manual retry option.
- **FR-017**: System MUST prevent direct form-based editing of outlines in favor of conversational editing.
- **FR-018**: System MUST implement the `DraftOutlineStage` job stage to orchestrate AI-driven outline generation from user input.
- **FR-019**: System MUST use a conversational AI model client (e.g., OpenAI-compatible API) for outline generation and modification.
- **FR-020**: System MUST include outline-specific prompt templates that guide the AI to produce well-structured outline proposals.
- **FR-026**: System MUST implement sliding window context management for long conversations (100+ messages): keep recent N messages in full context, generate summaries of earlier messages, and always pin the current outline state at the top of the AI context to ensure awareness of latest structure.
- **FR-021**: System MUST track all outline revision history linked to conversation messages with permanent retention.
- **FR-022**: System MUST support resumable outline generation—if a user closes the browser during outline creation, they should be able to continue the conversation when they return.
- **FR-023**: System MUST enforce that slide generation (GenerateSlidesStage) cannot proceed until an outline is in Approved status.
- **FR-024**: When an approved outline is modified, system MUST create a new draft outline version (preserving the original approved version as a separate revision), warn the user that changes may require regenerating slides, and request confirmation before proceeding.

### Assumptions

- Users prefer conversational interfaces over form-based CRUD for creative tasks like outline generation.
- The AI model is capable of understanding natural language outline edit requests (add, remove, reorder, modify).
- A chat UI component exists or will be built as part of this feature.
- The existing `ConversationMessage` entity and backend infrastructure can support outline-related conversations.
- SignalR or similar real-time communication is available for pushing outline updates to the UI.

### Dependencies

- Access to an AI model via `IModelClient` interface for outline generation and conversational editing.
- A prompt builder or template system for crafting outline-specific AI prompts.
- A frontend chat UI component (Vue-based) for displaying conversations and outline updates.
- Backend support for storing and retrieving conversation messages.
- Real-time communication infrastructure (SignalR/WebSockets) for pushing outline updates to clients.

### Key Entities *(include if feature involves data)*

- **ConversationMessage**: Represents a single message in the outline creation/editing conversation; includes sender (user or AI), content, timestamp, and associated project ID.
- **OutlineRevision**: A snapshot of the outline at a specific point in the conversation; includes slide list (titles and key points), creation timestamp, and reference to the triggering conversation message.
- **OutlinePrompt**: A structured AI prompt template for outline generation, including user description, target slide count, audience, and tone guidance.
- **OutlineEditRequest**: An internal representation of a user's conversational edit request, parsed into actionable commands (add/remove/reorder/modify).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can generate an initial 10-slide outline from a conversational description in under 30 seconds (AI response time).
- **SC-002**: 90% of outline edit requests result in correct modifications on the first attempt without requiring clarification.
- **SC-003**: Users can complete the full outline workflow (describe → generate → refine → approve) entirely through conversation without touching form inputs.
- **SC-004**: Conversation history is preserved and fully restored 100% of the time when projects are reopened.
- **SC-005**: Outline display updates within 2 seconds of AI response completion.
- **SC-006**: 95% of users find the conversational outline workflow more intuitive than manual form-based editing (measured via user feedback or usability testing).
- **SC-007**: Ambiguous outline requests trigger clarifying questions in 100% of cases, preventing incorrect modifications.
- **SC-008**: The system handles at least 50 conversation turns for a single outline without performance degradation or data loss.
