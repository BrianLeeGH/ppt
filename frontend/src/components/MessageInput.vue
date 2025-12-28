<template>
  <div class="message-input-container">
    <div v-if="error" class="error-message">
      {{ error }}
    </div>

    <div class="input-wrapper">
      <textarea
        v-model="messageContent"
        :disabled="disabled"
        placeholder="Type your message... (Shift+Enter for new line, Enter to send)"
        class="message-textarea"
        rows="3"
        @keydown="handleKeydown"
        ref="textareaRef"
      />

      <div class="input-footer">
        <span class="character-count" :class="{ 'over-limit': characterCount > 10000 }">
          {{ characterCount }} / 10,000
        </span>

        <button
          @click="handleSend"
          :disabled="disabled || !canSend"
          class="send-button"
        >
          {{ disabled ? 'Sending...' : 'Send' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';

const props = defineProps<{
  disabled?: boolean;
  error?: string | null;
}>();

const emit = defineEmits<{
  send: [content: string];
}>();

const messageContent = ref('');
const textareaRef = ref<HTMLTextAreaElement | null>(null);

const characterCount = computed(() => messageContent.value.length);
const canSend = computed(() => {
  return messageContent.value.trim().length > 0 && characterCount.value <= 10000;
});

function handleSend() {
  if (!canSend.value || props.disabled) return;

  emit('send', messageContent.value.trim());
  messageContent.value = '';

  // Reset textarea height
  if (textareaRef.value) {
    textareaRef.value.style.height = 'auto';
  }
}

function handleKeydown(event: KeyboardEvent) {
  // Enter without Shift sends the message
  if (event.key === 'Enter' && !event.shiftKey) {
    event.preventDefault();
    handleSend();
  }
}
</script>

<style scoped>
.message-input-container {
  border-top: 1px solid #e5e7eb;
  background: white;
}

.error-message {
  padding: 12px 16px;
  background: #fef2f2;
  color: #dc2626;
  font-size: 14px;
  border-bottom: 1px solid #fecaca;
}

.input-wrapper {
  padding: 16px;
}

.message-textarea {
  width: 100%;
  padding: 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
  font-family: inherit;
  resize: vertical;
  min-height: 60px;
  max-height: 200px;
  transition: border-color 0.2s;
}

.message-textarea:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.message-textarea:disabled {
  background: #f9fafb;
  cursor: not-allowed;
}

.input-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 8px;
}

.character-count {
  font-size: 12px;
  color: #6b7280;
}

.character-count.over-limit {
  color: #dc2626;
  font-weight: 600;
}

.send-button {
  padding: 8px 24px;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.send-button:hover:not(:disabled) {
  background: #2563eb;
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.send-button:disabled {
  background: #9ca3af;
  cursor: not-allowed;
  transform: none;
}

.send-button:active:not(:disabled) {
  transform: translateY(0);
}
</style>
