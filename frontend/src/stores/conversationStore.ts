import { defineStore } from 'pinia';
import { ref } from 'vue';
import { conversationApi, type ConversationMessage } from '../app/api/conversationApi';

export const useConversationStore = defineStore('conversation', () => {
  const messages = ref<ConversationMessage[]>([]);
  const isSending = ref(false);
  const error = ref<string | null>(null);

  async function loadMessages(projectId: string, limit: number = 50) {
    try {
      error.value = null;
      const response = await conversationApi.getMessages(projectId, limit);
      messages.value = response.messages;
    } catch (err) {
      error.value = 'Failed to load messages';
      console.error('Error loading messages:', err);
    }
  }

  async function sendMessage(projectId: string, content: string) {
    if (!content.trim()) {
      error.value = 'Message cannot be empty';
      return;
    }

    if (content.length > 10000) {
      error.value = 'Message exceeds maximum length of 10,000 characters';
      return;
    }

    isSending.value = true;
    error.value = null;

    try {
      const response = await conversationApi.sendMessage(projectId, content);

      // Optimistic update - add user message immediately
      messages.value.push(response.message);
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to send message';
      console.error('Error sending message:', err);
    } finally {
      isSending.value = false;
    }
  }

  function addMessage(message: ConversationMessage) {
    // Check if message already exists (avoid duplicates)
    const exists = messages.value.some(m => m.id === message.id);
    if (!exists) {
      messages.value.push(message);
    }
  }

  function clearMessages() {
    messages.value = [];
    error.value = null;
  }

  return {
    messages,
    isSending,
    error,
    loadMessages,
    sendMessage,
    addMessage,
    clearMessages
  };
});
