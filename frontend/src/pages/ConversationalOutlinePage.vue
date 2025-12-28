<template>
  <div class="conversational-outline-page">
    <div v-if="projectsStore.currentProject" class="container">
      <header class="page-header">
        <div>
          <h1>{{ projectsStore.currentProject.name }}</h1>
          <p class="subtitle">Create and refine your presentation outline through conversation</p>
        </div>
        <div class="nav-links">
          <router-link :to="{ name: 'style-brief', params: { projectId } }" class="nav-link">
            Continue to Style Brief →
          </router-link>
        </div>
      </header>

      <div class="content-grid">
        <!-- Left: Conversation Panel -->
        <div class="conversation-section">
          <ConversationPanel :project-id="projectId" />
        </div>

        <!-- Right: Outline Display -->
        <div class="outline-section">
          <div class="outline-header">
            <h2>Current Outline</h2>
            <span
              v-if="outlineStore.outline"
              :class="['status-badge', `status-${outlineStore.outline.status.toLowerCase()}`]"
            >
              {{ outlineStore.outline.status }}
            </span>
          </div>

          <div v-if="outlineStore.loading" class="loading-state">
            <div class="spinner"></div>
            <p>Loading outline...</p>
          </div>

          <div v-else-if="outlineStore.error" class="error-state">
            <p>{{ outlineStore.error }}</p>
            <button @click="loadOutline" class="retry-button">Retry</button>
          </div>

          <div v-else-if="outlineStore.outline" class="outline-display">
            <div
              v-for="(slide, index) in outlineStore.outline.slides"
              :key="index"
              :class="[
                'slide-card',
                { 'slide-changed': isSlideChanged(index) }
              ]"
            >
              <div class="slide-number">{{ index + 1 }}</div>
              <h3 class="slide-title">{{ slide.title }}</h3>
              <ul class="key-points">
                <li v-for="(point, pIndex) in slide.keyPoints" :key="pIndex">
                  {{ point }}
                </li>
              </ul>
            </div>

            <div v-if="outlineStore.outline.slides.length === 0" class="empty-outline">
              <p>No slides yet. Start a conversation to create your outline!</p>
            </div>
          </div>

          <div v-else class="empty-outline">
            <p>Start a conversation to create your outline</p>
            <p class="hint">Try: "Create a presentation about [your topic]"</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import { useProjectsStore } from '../stores/projectsStore';
import { useOutlineStore } from '../stores/outlineStore';
import { useConversationStore } from '../stores/conversationStore';
import { initializeConversationHub, disconnectConversationHub } from '../app/realtime/conversationHub';
import ConversationPanel from '../components/ConversationPanel.vue';

const route = useRoute();
const projectsStore = useProjectsStore();
const outlineStore = useOutlineStore();
const conversationStore = useConversationStore();
const projectId = route.params.projectId as string;

const isSlideChanged = (index: number) => {
  return outlineStore.changedSlideIndices.includes(index);
};

async function loadOutline() {
  await outlineStore.fetchOutline(projectId);
}

onMounted(async () => {
  // Load project data
  await projectsStore.fetchProject(projectId);

  // Load outline
  await loadOutline();

  // Load conversation history
  await conversationStore.loadMessages(projectId);

  // Initialize SignalR hub
  await initializeConversationHub(projectId);
});

onUnmounted(async () => {
  // Disconnect SignalR hub
  await disconnectConversationHub();

  // Clear conversation state
  conversationStore.clearMessages();
});
</script>

<style scoped>
.conversational-outline-page {
  height: 100vh;
  background: #f9fafb;
}

.container {
  max-width: 1600px;
  margin: 0 auto;
  padding: 24px;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 24px;
}

.page-header h1 {
  margin: 0 0 8px 0;
  font-size: 28px;
  font-weight: 700;
  color: #111827;
}

.subtitle {
  margin: 0;
  color: #6b7280;
  font-size: 16px;
}

.nav-links {
  display: flex;
  gap: 12px;
}

.nav-link {
  padding: 10px 20px;
  background: white;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  text-decoration: none;
  color: #374151;
  font-weight: 500;
  transition: all 0.2s;
}

.nav-link:hover {
  background: #f9fafb;
  border-color: #9ca3af;
}

.content-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  flex: 1;
  min-height: 0;
}

.conversation-section,
.outline-section {
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.outline-section {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  padding: 16px;
  overflow: hidden;
}

.outline-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e5e7eb;
}

.outline-header h2 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #111827;
}

.status-badge {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
}

.status-draft {
  background: #fef3c7;
  color: #92400e;
}

.status-approved {
  background: #d1fae5;
  color: #065f46;
}

.outline-display {
  flex: 1;
  overflow-y: auto;
  padding-right: 8px;
}

.slide-card {
  background: #f9fafb;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 12px;
  transition: all 0.3s ease;
}

.slide-card.slide-changed {
  border-color: #3b82f6;
  background: #eff6ff;
  animation: highlight-pulse 1s ease-in-out;
}

@keyframes highlight-pulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.02); }
}

.slide-number {
  display: inline-block;
  width: 32px;
  height: 32px;
  line-height: 32px;
  text-align: center;
  background: #3b82f6;
  color: white;
  border-radius: 50%;
  font-weight: 600;
  font-size: 14px;
  margin-bottom: 12px;
}

.slide-title {
  margin: 0 0 12px 0;
  font-size: 16px;
  font-weight: 600;
  color: #111827;
}

.key-points {
  margin: 0;
  padding-left: 20px;
  list-style-type: disc;
}

.key-points li {
  margin-bottom: 6px;
  color: #374151;
  font-size: 14px;
  line-height: 1.5;
}

.loading-state,
.error-state,
.empty-outline {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #6b7280;
  text-align: center;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f4f6;
  border-top-color: #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-state {
  color: #dc2626;
}

.retry-button {
  margin-top: 12px;
  padding: 8px 16px;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
}

.retry-button:hover {
  background: #2563eb;
}

.empty-outline .hint {
  font-size: 13px;
  font-style: italic;
  color: #9ca3af;
  margin-top: 8px;
}

/* Scrollbar styling */
.outline-display::-webkit-scrollbar {
  width: 6px;
}

.outline-display::-webkit-scrollbar-track {
  background: #f1f1f1;
}

.outline-display::-webkit-scrollbar-thumb {
  background: #888;
  border-radius: 3px;
}

.outline-display::-webkit-scrollbar-thumb:hover {
  background: #555;
}
</style>
