<template>
  <div class="conversation-panel">
    <div class="conversation-header">
      <h3>Outline Conversation</h3>
      <span v-if="isSending" class="status-indicator">Processing...</span>
    </div>

    <MessageList
      :messages="messages"
      :changed-slide-indices="changedSlideIndices"
    />

    <MessageInput
      :disabled="isSending"
      :error="error"
      @send="handleSendMessage"
    />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useConversationStore } from '../stores/conversationStore';
import { useOutlineStore } from '../stores/outlineStore';
import MessageList from './MessageList.vue';
import MessageInput from './MessageInput.vue';

const props = defineProps<{
  projectId: string;
}>();

const conversationStore = useConversationStore();
const outlineStore = useOutlineStore();

const messages = computed(() => conversationStore.messages);
const isSending = computed(() => conversationStore.isSending);
const error = computed(() => conversationStore.error);
const changedSlideIndices = computed(() => outlineStore.changedSlideIndices);

async function handleSendMessage(content: string) {
  await conversationStore.sendMessage(props.projectId, content);
}
</script>

<style scoped>
.conversation-panel {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.conversation-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  border-bottom: 1px solid #e5e7eb;
}

.conversation-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #111827;
}

.status-indicator {
  font-size: 14px;
  color: #6b7280;
  font-weight: 500;
}
</style>
