<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();
const projectId = route.params.projectId as string;

const previewUrl = computed(() => {
  const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
  return `${baseUrl}/projects/${projectId}/preview`;
});
</script>

<template>
  <div class="preview-page">
    <header class="page-header">
      <router-link :to="`/projects/${projectId}/generate`" class="back-link">← Back to Generation</router-link>
      <h1>Presentation Preview</h1>
    </header>

    <div class="preview-container">
      <iframe :src="previewUrl" class="preview-iframe"></iframe>
    </div>
  </div>
</template>

<style scoped>
.preview-page {
  display: flex;
  flex-direction: column;
  height: 100vh;
  padding: 1rem;
  box-sizing: border-box;
}

.page-header {
  margin-bottom: 1rem;
}

.back-link {
  color: #666;
  text-decoration: none;
  font-size: 0.9rem;
}

.preview-container {
  flex: 1;
  border: 1px solid #ccc;
  border-radius: 8px;
  overflow: hidden;
  background: #eee;
}

.preview-iframe {
  width: 100%;
  height: 100%;
  border: none;
}
</style>
