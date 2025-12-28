# Quickstart Guide: Conversational Outline Generation

**Feature**: 002-conversational-outline-generation
**Branch**: `002-conversational-outline-generation`
**Date**: 2025-12-28

## Overview

This guide walks through setting up and implementing the conversational outline generation feature. Follow these steps to get the feature running locally.

## Prerequisites

- .NET 10 SDK installed
- Node.js 18+ installed
- AI API credentials (OpenAI-compatible endpoint)
- Git (for branch management)

## Quick Start (5 minutes)

### 1. Checkout Feature Branch

```bash
git checkout 002-conversational-outline-generation
```

### 2. Configure AI Settings

**Option A: User Secrets (Recommended for Development)**

```bash
cd backend/src/SlideBuilder.Api
dotnet user-secrets set "Ai:BaseUrl" "https://api.openai.com/v1"
dotnet user-secrets set "Ai:ApiKey" "your-api-key-here"
dotnet user-secrets set "Ai:Model" "gpt-4"
```

**Option B: appsettings.Development.json**

```json
{
  "Ai": {
    "BaseUrl": "https://api.openai.com/v1",
    "ApiKey": "your-api-key-here",
    "Model": "gpt-4",
    "MaxRetries": 3,
    "TimeoutSeconds": 30
  }
}
```

**⚠️ Important**: Do NOT commit API keys to source control!

### 3. Apply Database Migrations

```bash
cd backend/src/SlideBuilder.Api
dotnet ef database update
```

This creates:
- `OutlineRevisions` table
- `ConversationSummaries` table
- Required indexes

### 4. Run Backend

```bash
cd backend/src/SlideBuilder.Api
dotnet run
```

Backend should start at `http://localhost:5000`

### 5. Install Frontend Dependencies

```bash
cd frontend
npm install
```

### 6. Run Frontend

```bash
npm run dev
```

Frontend should start at `http://localhost:5173`

### 7. Test the Feature

1. Navigate to `http://localhost:5173`
2. Create or open a project
3. Open the conversation panel
4. Send a message: *"Create a 5-slide presentation about artificial intelligence"*
5. Observe:
   - AI generates an outline
   - Outline displays in the panel
   - Conversation history updates

## Development Workflow

### Backend Development

**Project Structure**:
```text
backend/src/SlideBuilder.Api/
├── Controllers/ConversationController.cs     # NEW
├── Contracts/ConversationDtos.cs             # NEW
├── Hubs/JobsHub.cs                           # MODIFIED

backend/src/SlideBuilder.Core/
├── Domain/Entities.cs                        # MODIFIED (add OutlineRevision, ConversationSummary)
├── Jobs/Stages/DraftOutlineStage.cs          # MODIFIED (add AI logic)
├── AI/Prompts/OutlinePromptBuilder.cs        # NEW
├── AI/Prompts/ContextWindowManager.cs        # NEW
├── Persistence/IConversationRepository.cs    # NEW

backend/src/SlideBuilder.Infrastructure/
├── Persistence/ConversationRepository.cs     # NEW
└── Persistence/SlideBuilderDbContext.cs      # MODIFIED (add DbSets)
```

**Key Files to Implement**:

1. **ConversationController.cs**:
   ```csharp
   [ApiController]
   [Route("api/projects/{projectId}/conversation")]
   public class ConversationController : ControllerBase
   {
       [HttpGet("messages")]
       public async Task<IActionResult> GetMessages(Guid projectId, [FromQuery] int limit = 50);

       [HttpPost("messages")]
       public async Task<IActionResult> SendMessage(Guid projectId, [FromBody] SendMessageRequest request);
   }
   ```

2. **OutlinePromptBuilder.cs**:
   ```csharp
   public class OutlinePromptBuilder
   {
       public string BuildInitialGenerationPrompt(string userDescription, int? targetSlideCount = null);
       public string BuildEditRequestPrompt(string userRequest, Outline currentOutline);
       public string BuildApprovalPrompt(Outline outline);
   }
   ```

3. **DraftOutlineStage.cs** (enhanced):
   ```csharp
   public async Task ExecuteAsync(Job job, CancellationToken ct)
   {
       // 1. Load conversation messages
       var messages = await _conversationRepo.GetMessagesAsync(job.ProjectId, ct);

       // 2. Build AI context (sliding window)
       var context = _contextManager.BuildContextForAI(messages, currentOutline);

       // 3. Call AI model
       var proposal = await _modelClient.GenerateStructuredAsync<OutlineProposal>(context, ct);

       // 4. Update outline
       var revision = await _outlineService.ApplyProposal(job.ProjectId, proposal, ct);

       // 5. Notify clients via SignalR
       await _hub.Clients.Group(job.ProjectId.ToString()).SendAsync("OutlineUpdated", ...);
   }
   ```

### Frontend Development

**Project Structure**:
```text
frontend/src/
├── components/
│   ├── ConversationPanel.vue       # NEW: Main chat UI
│   ├── MessageList.vue             # NEW: Message display
│   ├── MessageInput.vue            # NEW: Input field + send button
│   └── OutlineDisplay.vue          # MODIFIED: Real-time updates
├── stores/
│   ├── conversationStore.ts        # NEW: Pinia store
│   └── outlineStore.ts             # MODIFIED: Handle SignalR events
└── services/
    ├── conversationService.ts      # NEW: API client
    └── signalrService.ts           # MODIFIED: Subscribe to events
```

**Key Components to Implement**:

1. **ConversationPanel.vue**:
   ```vue
   <template>
     <div class="conversation-panel">
       <MessageList :messages="messages" />
       <MessageInput
         :disabled="isSending"
         @send="handleSendMessage"
       />
     </div>
   </template>

   <script setup lang="ts">
   import { useConversationStore } from '@/stores/conversationStore';
   const store = useConversationStore();
   const { messages, sendMessage } = store;
   </script>
   ```

2. **conversationStore.ts**:
   ```typescript
   import { defineStore } from 'pinia';
   import { ref } from 'vue';
   import conversationService from '@/services/conversationService';

   export const useConversationStore = defineStore('conversation', () => {
     const messages = ref<ConversationMessage[]>([]);
     const isSending = ref(false);

     async function loadMessages(projectId: string) {
       const data = await conversationService.getMessages(projectId);
       messages.value = data.messages;
     }

     async function sendMessage(projectId: string, content: string) {
       isSending.value = true;
       try {
         const result = await conversationService.sendMessage(projectId, content);
         // Optimistic update
         messages.value.push({ role: 'User', content, createdAt: new Date().toISOString() });
       } finally {
         isSending.value = false;
       }
     }

     return { messages, isSending, loadMessages, sendMessage };
   });
   ```

3. **signalrService.ts** (enhanced):
   ```typescript
   import * as signalR from '@microsoft/signalr';
   import { useOutlineStore } from '@/stores/outlineStore';
   import { useConversationStore } from '@/stores/conversationStore';

   let connection: signalR.HubConnection | null = null;

   export async function connectToHub() {
     connection = new signalR.HubConnectionBuilder()
       .withUrl('/hubs/jobs')
       .withAutomaticReconnect()
       .build();

     connection.on('OutlineUpdated', (event) => {
       const outlineStore = useOutlineStore();
       outlineStore.setOutline(event.outline);
       outlineStore.highlightChanges(event.changedSlideIndices);
     });

     connection.on('MessageReceived', (message) => {
       const conversationStore = useConversationStore();
       conversationStore.addMessage(message);
     });

     await connection.start();
   }

   export async function joinProjectGroup(projectId: string) {
     await connection?.invoke('JoinProjectGroup', projectId);
   }
   ```

## Testing

### Unit Tests

**Backend (xUnit)**:

```bash
cd backend/tests/SlideBuilder.Tests
dotnet test
```

**Example Test**:
```csharp
public class DraftOutlineStageTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidUserMessage_CreatesOutlineRevision()
    {
        // Arrange
        var mockModelClient = new Mock<IModelClient>();
        mockModelClient.Setup(m => m.GenerateStructuredAsync<OutlineProposal>(It.IsAny<string>(), default, null))
            .ReturnsAsync(new OutlineProposal { Slides = [...] });

        // Act
        await stage.ExecuteAsync(job, CancellationToken.None);

        // Assert
        mockModelClient.Verify(m => m.GenerateStructuredAsync<OutlineProposal>(...), Times.Once);
    }
}
```

**Frontend (Vitest)**:

```bash
cd frontend
npm run test
```

**Example Test**:
```typescript
import { describe, it, expect } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useConversationStore } from '@/stores/conversationStore';

describe('conversationStore', () => {
  it('adds message when sendMessage succeeds', async () => {
    setActivePinia(createPinia());
    const store = useConversationStore();

    await store.sendMessage('project-id', 'Hello');

    expect(store.messages).toHaveLength(1);
    expect(store.messages[0].content).toBe('Hello');
  });
});
```

### Integration Testing

**Test Conversation Flow**:

1. Send POST to `/api/projects/{id}/conversation/messages`
2. Verify job created and status = `Running`
3. Wait for SignalR `OutlineUpdated` event
4. Verify outline has expected slides
5. Send follow-up message: "Add a slide about X"
6. Verify `OutlineUpdated` event with new slide

## Debugging

### Backend Debugging

**Enable Verbose Logging**:

```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "SlideBuilder.Core.Jobs": "Trace",
      "SlideBuilder.Core.AI": "Trace"
    }
  }
}
```

**Breakpoint Locations**:
- `ConversationController.SendMessage()` - Entry point
- `DraftOutlineStage.ExecuteAsync()` - AI processing
- `ContextWindowManager.BuildContextForAI()` - Context assembly

### Frontend Debugging

**Enable SignalR Logging**:

```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/jobs')
  .configureLogging(signalR.LogLevel.Debug) // Add this line
  .build();
```

**Vue DevTools**:
- Inspect Pinia stores: `conversationStore`, `outlineStore`
- Monitor state changes in real-time
- Check SignalR connection status

### Common Issues

**Issue**: AI responses timing out

**Solution**:
- Increase `AiSettings.TimeoutSeconds` to 60
- Check AI API rate limits
- Verify network connectivity

---

**Issue**: SignalR events not received

**Solution**:
- Check browser console for connection errors
- Verify `JoinProjectGroup` was called
- Confirm backend is sending events to correct group ID

---

**Issue**: Outline display not updating

**Solution**:
- Ensure `OutlineUpdated` handler calls `outlineStore.setOutline()`
- Check Vue reactivity (use `ref()` or `reactive()`)
- Verify SignalR connection is active

## Performance Tips

1. **Context Window Optimization**:
   - Tune `AiSettings.ContextWindowSize` (default: 20)
   - Generate summaries only when needed (threshold: 30 messages)

2. **Database Indexing**:
   - Ensure indexes exist on `ConversationMessages(ProjectId, CreatedAt)`
   - Monitor query performance with EF Core logging

3. **Frontend Optimization**:
   - Use virtual scrolling for long message lists (100+ messages)
   - Debounce "Send" button to prevent accidental double-sends

## Next Steps

After implementing the feature:

1. Run `/speckit.tasks` to generate task breakdown
2. Implement tasks in order (backend → frontend → tests)
3. Test user stories from spec.md
4. Create pull request for review

## Resources

- [Spec Document](spec.md)
- [Data Model](data-model.md)
- [API Contracts](contracts/conversation-api.yaml)
- [SignalR Events](contracts/signalr-events.md)
- [Research Decisions](research.md)

## Support

For questions or issues:
- Check the [plan.md](plan.md) for architecture decisions
- Review constitution.md for project principles
- Consult the feature spec for requirements clarity
