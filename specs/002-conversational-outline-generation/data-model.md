# Data Model: Conversational Outline Generation

**Feature**: 002-conversational-outline-generation
**Date**: 2025-12-28
**Phase**: 1 - Data Model Design

## Overview

This document defines the entity model, relationships, validation rules, and state transitions for the conversational outline generation feature.

## Entity Diagram

```text
Project (existing)
  ├── 1:N ConversationMessage (existing, enhanced)
  ├── 1:N Revision (existing)
  └── 1:1 Deck (existing)
              └── 1:1 Outline (existing, enhanced)
                      └── 1:N OutlineRevision (NEW)

ConversationSummary (NEW)
  └── N:1 Project
```

## Entities

### ConversationMessage (Existing - No Schema Changes)

**Purpose**: Stores individual messages in the outline creation/editing conversation.

**Schema** (already exists in `Entities.cs`):
```csharp
public class ConversationMessage
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Role { get; set; } = string.Empty; // User/Assistant/System
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

**Validation Rules**:
- `Role` MUST be one of: "User", "Assistant", "System"
- `Content` MUST NOT be empty for User/Assistant roles
- `Content` MAX length: 10,000 characters (prevents abuse)
- `ProjectId` MUST reference an existing Project

**State Transitions**: N/A (immutable once created)

**Notes**:
- Existing entity supports the feature without changes
- Frontend will set `Role = "User"` for user messages, backend sets `Role = "Assistant"` for AI responses
- System messages (e.g., "Outline approved") use `Role = "System"`

---

### OutlineRevision (NEW)

**Purpose**: Tracks outline snapshots at specific points in the conversation, enabling version history and auditability.

**Schema**:
```csharp
public class OutlineRevision
{
    public Guid Id { get; set; }
    public Guid OutlineId { get; set; }
    public Guid? TriggeredByMessageId { get; set; }  // Which conversation message caused this revision
    public string SlidesJson { get; set; } = "[]";   // JSON array of {title, keyPoints}
    public int RevisionNumber { get; set; }          // Sequential: 1, 2, 3...
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Outline Outline { get; set; } = null!;
    public ConversationMessage? TriggeredByMessage { get; set; }
}
```

**Validation Rules**:
- `OutlineId` MUST reference an existing Outline
- `TriggeredByMessageId` MAY be null (for initial system-generated outlines)
- `SlidesJson` MUST be valid JSON array
- `SlidesJson` parsed content MUST match schema: `Array<{title: string, keyPoints: string[]}>`
- `RevisionNumber` MUST be sequential within an Outline (enforced by application logic)
- `RevisionNumber` MUST be > 0

**Relationships**:
- N:1 with Outline (many revisions per outline)
- N:1 with ConversationMessage (optional; tracks which message triggered this revision)

**State Transitions**: N/A (immutable once created)

**Notes**:
- New revision created whenever outline changes via conversation
- Enables "diff" view between revisions
- `TriggeredByMessageId` links revision history to conversation flow

---

### ConversationSummary (NEW)

**Purpose**: Stores AI-generated summaries of old conversation messages to enable sliding window context management.

**Schema**:
```csharp
public class ConversationSummary
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public int CoveringMessagesFrom { get; set; }  // First message index included in summary
    public int CoveringMessagesTo { get; set; }    // Last message index included in summary
    public string SummaryText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}
```

**Validation Rules**:
- `ProjectId` MUST reference an existing Project
- `CoveringMessagesFrom` MUST be >= 0
- `CoveringMessagesTo` MUST be >= `CoveringMessagesFrom`
- `SummaryText` MUST NOT be empty
- `SummaryText` MAX length: 2,000 characters
- No overlapping summaries within same Project (enforced by application logic)

**Relationships**:
- N:1 with Project (many summaries per project as conversation grows)

**State Transitions**: N/A (immutable once created)

**Notes**:
- Created automatically when conversation exceeds 30 messages
- Used by `ContextWindowManager` to build AI prompts
- Multiple summaries may exist if conversation is very long (e.g., messages 1-50, 51-100)

---

### Outline (Existing - Enhanced Behavior)

**Purpose**: Represents the current draft or approved outline for a deck.

**Schema** (already exists, no changes):
```csharp
public class Outline
{
    public Guid Id { get; set; }
    public Guid DeckId { get; set; }
    public OutlineStatus Status { get; set; }
    public string SlidesJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
}

public enum OutlineStatus
{
    Draft,
    Approved
}
```

**Enhanced Behavior for This Feature**:
- When outline is Approved and user requests edits, system MUST:
  1. Create a new `Outline` with `Status = Draft`
  2. Copy `SlidesJson` from approved outline as starting point
  3. Preserve old approved `Outline` (mark as archived or keep in Revision history)
  4. Warn user: "This will create a new draft version. Approved outline will be preserved."

**Validation Rules** (existing + new):
- `Status` MUST be one of: Draft, Approved
- `SlidesJson` MUST be valid JSON array
- Only ONE Draft outline allowed per Deck at a time (enforced by application logic)
- Approval transition MUST happen via conversation (FR-008)

**State Transitions**:
```text
Draft → Approved (when user says "approve outline")
Approved → [Archive] + new Draft (when user requests edits to approved outline)
```

**Notes**:
- Existing entity supports feature with behavior enhancements only
- `SlidesJson` format: `[{title: "...", keyPoints: ["...", "..."]}]`

---

## Supporting Models (Non-Persisted)

### OutlineProposal (In-Memory DTO)

**Purpose**: Structured representation of AI-generated outline proposals.

**Schema**:
```csharp
public class OutlineProposal
{
    public List<SlideProposal> Slides { get; set; } = new();
    public bool NeedsClarification { get; set; }
    public List<string>? ClarificationQuestions { get; set; }
}

public class SlideProposal
{
    public string Title { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = new();
}
```

**Usage**: Deserialized from AI JSON output, then converted to `SlidesJson` for storage.

---

### OutlineEditCommand (In-Memory DTO)

**Purpose**: Parsed representation of a conversational edit request.

**Schema**:
```csharp
public class OutlineEditCommand
{
    public EditOperation Operation { get; set; }
    public int? TargetSlideIndex { get; set; }  // Zero-based
    public SlideProposal? NewSlide { get; set; }
    public string? ModificationInstructions { get; set; }
    public bool RequiresConfirmation { get; set; }
    public string? ConfirmationMessage { get; set; }
}

public enum EditOperation
{
    Insert,      // Add new slide
    Delete,      // Remove slide
    Reorder,     // Move slide(s)
    Modify,      // Edit slide title/keyPoints
    Approve      // Change status to Approved
}
```

**Usage**: AI parses user message into this structure, backend applies edit to outline.

---

## Validation Summary

| Entity | Key Validations |
|--------|-----------------|
| ConversationMessage | Role ∈ {User, Assistant, System}, Content non-empty, max 10K chars |
| OutlineRevision | Valid JSON, sequential RevisionNumber, OutlineId exists |
| ConversationSummary | Non-overlapping ranges, max 2K chars, From <= To |
| Outline | One Draft per Deck, conversational approval only |

## Indexing Strategy

**Performance-Critical Queries**:

1. **Get recent messages for a project** (for context window):
   ```sql
   SELECT * FROM ConversationMessages
   WHERE ProjectId = @id
   ORDER BY CreatedAt DESC
   LIMIT 20;
   ```
   **Index**: `CREATE INDEX IX_ConversationMessages_ProjectId_CreatedAt ON ConversationMessages(ProjectId, CreatedAt DESC);`

2. **Get latest outline revision**:
   ```sql
   SELECT * FROM OutlineRevisions
   WHERE OutlineId = @id
   ORDER BY RevisionNumber DESC
   LIMIT 1;
   ```
   **Index**: `CREATE INDEX IX_OutlineRevisions_OutlineId_RevisionNumber ON OutlineRevisions(OutlineId, RevisionNumber DESC);`

3. **Get summaries for a project** (for context building):
   ```sql
   SELECT * FROM ConversationSummaries
   WHERE ProjectId = @id
   ORDER BY CoveringMessagesFrom ASC;
   ```
   **Index**: `CREATE INDEX IX_ConversationSummaries_ProjectId_Range ON ConversationSummaries(ProjectId, CoveringMessagesFrom);`

## Migration Notes

**New Tables**:
- `OutlineRevisions` (new table)
- `ConversationSummaries` (new table)

**Modified Tables**:
- None (existing entities support the feature)

**Data Migration**:
- Existing `Outline` records: No migration needed
- Existing `ConversationMessage` records: No migration needed
- For existing outlines, create initial `OutlineRevision` with `RevisionNumber = 1` if needed for historical tracking (optional, not required for MVP)

**EF Core Migration Command**:
```bash
cd backend/src/SlideBuilder.Api
dotnet ef migrations add AddConversationalOutlineEntities
dotnet ef database update
```

## Database Schema (SQL)

```sql
-- OutlineRevisions table
CREATE TABLE OutlineRevisions (
    Id TEXT PRIMARY KEY,
    OutlineId TEXT NOT NULL,
    TriggeredByMessageId TEXT,
    SlidesJson TEXT NOT NULL DEFAULT '[]',
    RevisionNumber INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (OutlineId) REFERENCES Outlines(Id),
    FOREIGN KEY (TriggeredByMessageId) REFERENCES ConversationMessages(Id)
);

-- ConversationSummaries table
CREATE TABLE ConversationSummaries (
    Id TEXT PRIMARY KEY,
    ProjectId TEXT NOT NULL,
    CoveringMessagesFrom INTEGER NOT NULL,
    CoveringMessagesTo INTEGER NOT NULL,
    SummaryText TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
);

-- Indexes
CREATE INDEX IX_OutlineRevisions_OutlineId_RevisionNumber
    ON OutlineRevisions(OutlineId, RevisionNumber DESC);

CREATE INDEX IX_ConversationMessages_ProjectId_CreatedAt
    ON ConversationMessages(ProjectId, CreatedAt DESC);

CREATE INDEX IX_ConversationSummaries_ProjectId_Range
    ON ConversationSummaries(ProjectId, CoveringMessagesFrom);
```

## State Transition Diagram

```text
Outline Status Flow:
┌─────┐  User: "approve outline"   ┌──────────┐
│Draft│ ───────────────────────────>│ Approved │
└─────┘                             └──────────┘
   ↑                                      │
   │                                      │ User requests edit
   │                                      ↓
   │                             ┌────────────────┐
   └─────────────────────────────┤ New Draft + AI │
                                 │ copies content │
                                 └────────────────┘
                            (Approved outline preserved)

OutlineRevision Creation Flow:
┌──────────────┐
│ User Message │
└──────┬───────┘
       ↓
┌──────────────┐    AI generates    ┌───────────────┐
│ AI Processes │ ─────────────────> │ New Outline   │
│ via Stage    │                    │ SlidesJson    │
└──────────────┘                    └───────┬───────┘
                                            ↓
                                   ┌─────────────────┐
                                   │ OutlineRevision │
                                   │ RevisionNumber++│
                                   │ TriggeredByMsg  │
                                   └─────────────────┘
```

## Next Steps

Data model complete. Proceeding to **Contracts** generation (API contracts, DTOs, OpenAPI schemas).
