# Data Model: AI Slide HTML Presentation Builder

This describes entities and relationships for the MVP. It is implementation-agnostic, but designed to support: outline/style confirmation, versioning, and resumable jobs.

## Entities

### Project
- Fields: id, name, createdAt, updatedAt
- Relationships: 1 Project → 1 Deck; 1 Project → many Assets; 1 Project → many ConversationMessages; 1 Project → many Jobs; 1 Project → many Revisions

### Deck
- Fields: id, projectId, title (optional), intentSummary (optional)
- Relationships: 1 Deck → 1 Outline (draft); 1 Deck → 1 Outline (approved reference via revision); 1 Deck → 1 StyleBrief (draft); 1 Deck → 1 StyleBrief (approved reference via revision); 1 Deck → many Slides

### Outline
- Fields: id, deckId, status (Draft/Approved), slides[] (ordered list: title + keyPoints), createdAt
- Notes: Outline is explicitly confirmed and can be revised.

### StyleBrief
- Fields: id, deckId, status (Draft/Approved), fields (VisualTone, AudienceContext, Density, Emphasis, Contrast, TypographyHierarchy, LayoutRhythm, MotionFeel, MediaTreatment, BrandConstraints), createdAt

### Slide
- Fields: id, deckId, orderIndex, title, contentBlocks (text/media references), speakerNotes (optional)

### Asset
- Fields: id, projectId, kind (Image/Other), originalName, contentType, sizeBytes, storageKey, source (Upload/URL), createdAt
- Notes: Stored in OSS; referenced by slides.

### ConversationMessage
- Fields: id, projectId, role (User/Assistant/System), content, createdAt

### Revision
- Fields: id, projectId, createdAt, summary (optional)
- Notes: Points to a snapshot of deck state (outline/style/slide content) used for preview/export.

### Job
- Fields: id, projectId, status (Queued/Running/Succeeded/Failed/Stopped), stage (DraftOutline/OutlineApproved/GenerateSlides/CompilePreview/Export/UploadAssets), createdAt, updatedAt, error (optional), inputSummary (optional)

### Checkpoint
- Fields: id, jobId, stageCompleted, revisionId (optional), createdAt, details (optional)

## Key Relationships

- Project 1→N Jobs; Job 1→N Checkpoints
- Project 1→N Revisions
- Deck owns the working (draft) Outline + StyleBrief; approved versions are linked via Revision
- Slides reference Assets by id/storageKey

## State Transitions (MVP)

- Outline: Draft → Approved (via explicit confirmation)
- StyleBrief: Draft → Approved (via explicit confirmation)
- Job: Queued → Running → (Succeeded | Failed | Stopped)
- Job stage advances monotonically; resume continues from last completed stage
