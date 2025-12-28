<script setup lang="ts">
import { ref } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();
const projectId = route.params.projectId as string;

const exporting = ref(false);
const exportError = ref<string | null>(null);
const downloadUrl = ref<string | null>(null);

const startExport = async () => {
  exporting.value = true;
  exportError.value = null;
  downloadUrl.value = null;

  try {
    // We can either trigger a server-side save or just use the direct download link
    // For MVP, let's provide a direct download link
    const baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
    downloadUrl.value = `${baseUrl}/projects/${projectId}/export/download`;
  } catch (err: any) {
    exportError.value = err.message;
  } finally {
    exporting.value = false;
  }
};
</script>

<template>
  <div class="export-page">
    <header class="page-header">
      <router-link :to="`/projects/${projectId}/preview`" class="back-link">← Back to Preview</router-link>
      <h1>Export Presentation</h1>
    </header>

    <div class="export-card">
      <p>Export your presentation as a self-contained ZIP file containing HTML, CSS, and JS.</p>

      <div v-if="!downloadUrl" class="actions">
        <button @click="startExport" :disabled="exporting" class="primary-button">
          {{ exporting ? 'Preparing Export...' : 'Prepare Export' }}
        </button>
      </div>

      <div v-else class="success-box">
        <p>Your export is ready!</p>
        <a :href="downloadUrl" class="download-link" download>Download ZIP File</a>
        <button @click="downloadUrl = null" class="text-button">Start Over</button>
      </div>

      <div v-if="exportError" class="error-box">
        {{ exportError }}
      </div>
    </div>
  </div>
</template>

<style scoped>
.export-page {
  max-width: 600px;
  margin: 0 auto;
  padding: 2rem;
}

.page-header {
  margin-bottom: 2rem;
}

.back-link {
  display: block;
  margin-bottom: 0.5rem;
  color: #666;
  text-decoration: none;
}

.export-card {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  text-align: center;
}

.actions {
  margin-top: 2rem;
}

.primary-button {
  background: #42b883;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  cursor: pointer;
  font-weight: bold;
}

.success-box {
  margin-top: 2rem;
  padding: 1.5rem;
  background: #e8f5e9;
  border-radius: 4px;
}

.download-link {
  display: inline-block;
  background: #2e7d32;
  color: white;
  text-decoration: none;
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  font-weight: bold;
  margin: 1rem 0;
}

.text-button {
  display: block;
  margin: 0 auto;
  background: none;
  border: none;
  color: #666;
  cursor: pointer;
  text-decoration: underline;
}

.error-box {
  margin-top: 1rem;
  color: #c62828;
}
</style>
