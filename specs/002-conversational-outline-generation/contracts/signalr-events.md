# SignalR Events: Conversational Outline Generation

**Feature**: 002-conversational-outline-generation
**Date**: 2025-12-28

## Overview

This document defines the SignalR hub events for real-time communication between the backend and frontend during conversational outline generation.

## Hub Connection

**Hub URL**: `/hubs/jobs` (existing JobsHub, extended)

**Authentication**: Bearer token (if applicable)

**Connection Groups**: Clients automatically joined to project-specific groups upon connection

## Client → Server Methods

### JoinProjectGroup

**Purpose**: Subscribe to real-time updates for a specific project

**Method Signature**:
```csharp
Task JoinProjectGroup(Guid projectId)
```

**Client Invocation** (TypeScript):
```typescript
await connection.invoke('JoinProjectGroup', projectId);
```

**Response**: None (silent success)

**Error Cases**:
- Invalid `projectId` → throws `HubException`
- User lacks access to project → throws `HubException` with "Unauthorized"

---

### LeaveProjectGroup

**Purpose**: Unsubscribe from updates for a specific project

**Method Signature**:
```csharp
Task LeaveProjectGroup(Guid projectId)
```

**Client Invocation** (TypeScript):
```typescript
await connection.invoke('LeaveProjectGroup', projectId);
```

**Response**: None (silent success)

---

## Server → Client Events

### OutlineUpdated

**Purpose**: Notifies clients when the outline has been modified (new revision created)

**Event Signature**:
```csharp
Task OutlineUpdated(OutlineUpdatedEvent payload)
```

**Payload Schema**:
```typescript
interface OutlineUpdatedEvent {
  projectId: string;          // UUID
  outlineId: string;          // UUID
  outline: OutlineDto;        // Full outline data
  revisionNumber: number;     // New revision number
  changedSlideIndices: number[]; // Zero-based indices of changed slides (for UI highlighting)
  triggeredByMessageId?: string; // UUID of message that caused this update
}
```

**Example Payload** (JSON):
```json
{
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "outlineId": "8b3a7c12-9f45-4d8e-bc34-1e7a9f3c8b21",
  "outline": {
    "id": "8b3a7c12-9f45-4d8e-bc34-1e7a9f3c8b21",
    "deckId": "2f8d6a11-3c45-4f7b-a123-9e8c7b6a5d43",
    "status": "Draft",
    "slides": [
      {
        "title": "Introduction",
        "keyPoints": ["Overview of topic", "Goals of presentation"]
      },
      {
        "title": "Renewable Energy Sources",
        "keyPoints": ["Solar power", "Wind energy", "Hydroelectric systems"]
      }
    ],
    "createdAt": "2025-12-28T10:30:00Z",
    "currentRevisionNumber": 5
  },
  "revisionNumber": 5,
  "changedSlideIndices": [1],
  "triggeredByMessageId": "7c4b2a91-8e34-4f5c-ab12-6d9e8c7f5a34"
}
```

**Client Handler** (TypeScript):
```typescript
connection.on('OutlineUpdated', (event: OutlineUpdatedEvent) => {
  console.log(`Outline updated: revision ${event.revisionNumber}`);
  outlineStore.setOutline(event.outline);
  highlightChangedSlides(event.changedSlideIndices);
});
```

**When Triggered**:
- After AI processes a user message and modifies the outline
- After user approves outline (status changes to `Approved`)
- After system creates a new draft from an approved outline

---

### MessageReceived

**Purpose**: Notifies clients when a new conversation message is created (typically an AI response)

**Event Signature**:
```csharp
Task MessageReceived(ConversationMessageDto message)
```

**Payload Schema**:
```typescript
interface ConversationMessageDto {
  id: string;           // UUID
  role: 'User' | 'Assistant' | 'System';
  content: string;
  createdAt: string;    // ISO 8601 datetime
  metadata?: Record<string, any>;
}
```

**Example Payload** (JSON):
```json
{
  "id": "9e7c3b81-2f45-4a8d-bc12-5d8e7f6a9c34",
  "role": "Assistant",
  "content": "I've added a new slide about renewable energy after slide 3. The outline now has 11 slides total. Would you like me to make any adjustments?",
  "createdAt": "2025-12-28T10:30:05Z",
  "metadata": {
    "triggeredRevisionId": "8b3a7c12-9f45-4d8e-bc34-1e7a9f3c8b21"
  }
}
```

**Client Handler** (TypeScript):
```typescript
connection.on('MessageReceived', (message: ConversationMessageDto) => {
  conversationStore.addMessage(message);
  scrollToLatestMessage();
});
```

**When Triggered**:
- After AI generates a response to a user message
- After system generates informational messages (e.g., "Outline approved")

---

### JobProgressUpdated (Existing Event - Reused)

**Purpose**: Notifies clients of job execution progress (used for long-running AI operations)

**Event Signature**:
```csharp
Task JobProgressUpdated(JobProgressEvent payload)
```

**Payload Schema**:
```typescript
interface JobProgressEvent {
  jobId: string;
  projectId: string;
  status: 'Queued' | 'Running' | 'Succeeded' | 'Failed' | 'Stopped';
  stage: string;
  error?: string;
  updatedAt: string;
}
```

**Example Payload** (JSON):
```json
{
  "jobId": "5f3a8c12-4b67-4d9e-ab34-2e8c7f6a9d21",
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Running",
  "stage": "DraftOutline",
  "updatedAt": "2025-12-28T10:30:03Z"
}
```

**Client Handler** (TypeScript):
```typescript
connection.on('JobProgressUpdated', (progress: JobProgressEvent) => {
  if (progress.stage === 'DraftOutline') {
    showLoadingSpinner('Generating outline...');
  }
  if (progress.status === 'Failed') {
    showError(progress.error || 'Job failed');
  }
});
```

**When Triggered**:
- During outline generation (stage: `DraftOutline`)
- On job completion or failure

---

## Event Flow Diagram

```text
User Action (Frontend):
┌──────────────────┐
│ User sends msg   │
│ via POST /api/   │
│ conversation     │
└────────┬─────────┘
         │
         ↓
Backend Processing:
┌──────────────────┐
│ Create user msg  │
│ Trigger job      │
└────────┬─────────┘
         │
         ↓
┌──────────────────────────┐
│ SignalR: JobProgressUpdated │ ────→ Frontend shows "Generating..."
│ status=Running           │
└────────┬─────────────────┘
         │
         ↓
┌──────────────────┐
│ AI processes     │
│ DraftOutlineStage│
└────────┬─────────┘
         │
         ↓
┌──────────────────────────┐
│ Create OutlineRevision   │
│ Create Assistant message │
└────────┬─────────────────┘
         │
         ├────→ SignalR: OutlineUpdated ────→ Frontend updates outline display
         │
         └────→ SignalR: MessageReceived ───→ Frontend adds AI response to chat
```

## Client Connection Example (TypeScript)

```typescript
import * as signalR from '@microsoft/signalr';

// Initialize connection
const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/jobs')
  .withAutomaticReconnect()
  .build();

// Register event handlers
connection.on('OutlineUpdated', (event: OutlineUpdatedEvent) => {
  outlineStore.setOutline(event.outline);
  highlightChangedSlides(event.changedSlideIndices);
});

connection.on('MessageReceived', (message: ConversationMessageDto) => {
  conversationStore.addMessage(message);
});

connection.on('JobProgressUpdated', (progress: JobProgressEvent) => {
  if (progress.stage === 'DraftOutline') {
    if (progress.status === 'Running') {
      showLoadingSpinner('AI is generating your outline...');
    } else if (progress.status === 'Succeeded') {
      hideLoadingSpinner();
    } else if (progress.status === 'Failed') {
      showError('Failed to generate outline. Please try again.');
    }
  }
});

// Start connection
await connection.start();

// Join project group
const projectId = 'your-project-id';
await connection.invoke('JoinProjectGroup', projectId);

// Cleanup on component unmount
onUnmounted(() => {
  connection.invoke('LeaveProjectGroup', projectId);
  connection.stop();
});
```

## Error Handling

### Connection Failures
- Frontend should use `withAutomaticReconnect()` for resilience
- On reconnect, re-join project groups automatically

### Event Delivery Guarantees
- SignalR provides at-least-once delivery
- Frontend should handle duplicate events gracefully (idempotent state updates)

### Missed Events
- If client disconnects and reconnects, it may miss events
- On reconnect, frontend should:
  1. Re-fetch current outline via GET `/api/projects/{id}/outline/current`
  2. Re-fetch recent messages via GET `/api/projects/{id}/conversation/messages?limit=50`

## Security Considerations

- All SignalR events are scoped to project groups
- Clients can only join groups for projects they have access to (enforced by `JoinProjectGroup` authorization)
- No sensitive data (API keys, internal IDs) should be included in event payloads

## Testing

### Manual Testing (Browser Console)
```javascript
// Connect to hub
const conn = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/jobs')
  .build();

conn.on('OutlineUpdated', console.log);
await conn.start();
await conn.invoke('JoinProjectGroup', '<project-id>');

// Send a message via API, observe events in console
```

### Unit Testing (Backend)
- Mock `IHubContext<JobsHub>` in tests
- Verify `Clients.Group(projectId).SendAsync("OutlineUpdated", ...)` is called

### Integration Testing
- Use SignalR test client to connect to hub
- Send API requests, assert events received
