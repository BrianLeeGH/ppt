<template>
  <div class="message-list" ref="messageContainer">
    <div
      v-for="message in messages"
      :key="message.id"
      :class="['message', `message-${message.role.toLowerCase()}`]"
    >
      <div class="message-header">
        <span class="message-role">{{ message.role }}</span>
        <span class="message-time">{{ formatTime(message.createdAt) }}</span>
      </div>
      <div class="message-content">
        {{ message.content }}
      </div>
    </div>

    <div v-if="messages.length === 0" class="empty-state">
      <p>Start a conversation to create your outline</p>
      <p class="hint">Try: "Create a 10-slide presentation about climate change"</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue';
import type { ConversationMessage } from '../app/api/conversationApi';

const props = defineProps<{
  messages: ConversationMessage[];
  changedSlideIndices?: number[];
}>();

const messageContainer = ref<HTMLElement | null>(null);

// Auto-scroll to bottom when new messages arrive
watch(() => props.messages.length, async () => {
  await nextTick();
  if (messageContainer.value) {
    messageContainer.value.scrollTop = messageContainer.value.scrollHeight;
  }
});

function formatTime(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}
</script>

<style scoped>
.message-list {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.message {
  padding: 12px;
  border-radius: 8px;
  max-width: 80%;
}

.message-user {
  align-self: flex-end;
  background: #3b82f6;
  color: white;
}

.message-assistant {
  align-self: flex-start;
  background: #f3f4f6;
  color: #111827;
}

.message-system {
  align-self: center;
  background: #ecfdf5;
  color: #065f46;
  font-style: italic;
  max-width: 90%;
  text-align: center;
}

.message-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 6px;
  font-size: 12px;
  opacity: 0.8;
}

.message-role {
  font-weight: 600;
}

.message-time {
  font-size: 11px;
}

.message-content {
  font-size: 14px;
  line-height: 1.5;
  white-space: pre-wrap;
  word-wrap: break-word;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #6b7280;
  text-align: center;
}

.empty-state p {
  margin: 8px 0;
}

.empty-state .hint {
  font-size: 13px;
  font-style: italic;
  color: #9ca3af;
}

/* Scrollbar styling */
.message-list::-webkit-scrollbar {
  width: 6px;
}

.message-list::-webkit-scrollbar-track {
  background: #f1f1f1;
}

.message-list::-webkit-scrollbar-thumb {
  background: #888;
  border-radius: 3px;
}

.message-list::-webkit-scrollbar-thumb:hover {
  background: #555;
}
</style>
