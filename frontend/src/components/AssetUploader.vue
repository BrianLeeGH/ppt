<script setup lang="ts">
import { ref, onMounted } from 'vue';
import apiClient from '../app/api/client';

const props = defineProps<{
  projectId: string;
}>();

const emit = defineEmits(['asset-selected']);

const assets = ref<any[]>([]);
const uploading = ref(false);
const importUrl = ref('');
const importing = ref(false);

const fetchAssets = async () => {
  try {
    const response = await apiClient.get(`/projects/${props.projectId}/assets`);
    assets.value = response.data;
  } catch (err) {
    console.error('Failed to fetch assets', err);
  }
};

const handleFileUpload = async (event: Event) => {
  const target = event.target as HTMLInputElement;
  if (!target.files?.length) return;

  uploading.value = true;
  const file = target.files[0];
  if (!file) return;

  const formData = new FormData();
  formData.append('file', file);

  try {
    const response = await apiClient.post(`/projects/${props.projectId}/assets/upload`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    assets.value.unshift(response.data);
    emit('asset-selected', response.data);
  } catch (err) {
    console.error('Upload failed', err);
  } finally {
    uploading.value = false;
  }
};

const handleImport = async () => {
  if (!importUrl.value) return;

  importing.value = true;
  try {
    const response = await apiClient.post(`/projects/${props.projectId}/assets/import`, {
      url: importUrl.value
    });
    assets.value.unshift(response.data);
    emit('asset-selected', response.data);
    importUrl.value = '';
  } catch (err) {
    console.error('Import failed', err);
  } finally {
    importing.value = false;
  }
};

onMounted(fetchAssets);
</script>

<template>
  <div class="asset-uploader">
    <div class="upload-controls">
      <div class="file-upload">
        <label class="upload-button">
          <input type="file" @change="handleFileUpload" :disabled="uploading" accept="image/*" />
          {{ uploading ? 'Uploading...' : 'Upload Image' }}
        </label>
      </div>

      <div class="url-import">
        <input v-model="importUrl" placeholder="Paste image URL..." :disabled="importing" />
        <button @click="handleImport" :disabled="importing || !importUrl">Import</button>
      </div>
    </div>

    <div class="asset-grid">
      <div v-for="asset in assets" :key="asset.id" class="asset-item" @click="emit('asset-selected', asset)">
        <img v-if="asset.kind === 'Image'" :src="asset.storageKey" :alt="asset.originalName" />
        <div v-else class="other-asset">{{ asset.originalName }}</div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.asset-uploader {
  border: 1px solid #eee;
  padding: 1rem;
  border-radius: 8px;
}

.upload-controls {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
}

.upload-button {
  display: inline-block;
  background: #42b883;
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
}

.upload-button input {
  display: none;
}

.url-import {
  display: flex;
  flex: 1;
  gap: 0.5rem;
}

.url-import input {
  flex: 1;
  padding: 0.5rem;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.asset-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
  gap: 0.5rem;
  max-height: 300px;
  overflow-y: auto;
}

.asset-item {
  border: 1px solid #eee;
  border-radius: 4px;
  overflow: hidden;
  cursor: pointer;
  aspect-ratio: 1;
}

.asset-item:hover {
  border-color: #42b883;
}

.asset-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.other-asset {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  font-size: 0.8rem;
  padding: 0.5rem;
  text-align: center;
}
</style>
