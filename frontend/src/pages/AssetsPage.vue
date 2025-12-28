<script setup lang="ts">
import { useRoute } from 'vue-router';
import AssetUploader from '../components/AssetUploader.vue';

const route = useRoute();
const projectId = route.params.projectId as string;

const onAssetSelected = (asset: any) => {
  console.log('Asset selected:', asset);
};
</script>

<template>
  <div class="assets-page">
    <header class="page-header">
      <router-link :to="`/projects/${projectId}/outline`" class="back-link">← Back to Project</router-link>
      <h1>Project Assets</h1>
    </header>

    <section class="assets-section">
      <AssetUploader :projectId="projectId" @asset-selected="onAssetSelected" />
    </section>
  </div>
</template>

<style scoped>
.assets-page {
  max-width: 1000px;
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

.assets-section {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
</style>
