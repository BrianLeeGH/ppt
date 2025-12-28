<template>
  <div class="outline-page">
    <div v-if="projectsStore.currentProject">
      <h1>Outline: {{ projectsStore.currentProject.name }}</h1>
      <div class="nav-links">
        <router-link :to="{ name: 'style-brief', params: { projectId } }">Go to Style Brief</router-link>
      </div>
      <div class="status">Status: {{ outlineStore.outline?.status }}</div>

      <div v-if="outlineStore.loading">Loading...</div>
      <div v-else-if="outlineStore.error" class="error">{{ outlineStore.error }}</div>

      <div v-else-if="outlineStore.outline" class="editor">
        <div v-for="(slide, index) in editableSlides" :key="index" class="slide-edit">
          <input v-model="slide.title" placeholder="Slide Title" />
          <textarea v-model="slide.keyPointsString" placeholder="Key Points (one per line)"></textarea>
          <button @click="removeSlide(index)">Remove</button>
        </div>

        <button @click="addSlide">Add Slide</button>
        <button @click="saveOutline" :disabled="outlineStore.loading">Save Changes</button>
        <button @click="approveOutline" :disabled="outlineStore.loading || outlineStore.outline.status === 'Approved'" class="approve-btn">
          Approve Outline
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useProjectsStore } from '../stores/projectsStore';
import { useOutlineStore, type OutlineSlide } from '../stores/outlineStore';

const route = useRoute();
const projectsStore = useProjectsStore();
const outlineStore = useOutlineStore();
const projectId = route.params.projectId as string;

const editableSlides = ref<{ title: string; keyPointsString: string }[]>([]);

onMounted(async () => {
  await projectsStore.fetchProject(projectId);
  await outlineStore.fetchOutline(projectId);
});

watch(() => outlineStore.outline, (newOutline) => {
  if (newOutline) {
    editableSlides.value = newOutline.slides.map(s => ({
      title: s.title,
      keyPointsString: s.keyPoints.join('\n')
    }));
  }
}, { immediate: true });

const addSlide = () => {
  editableSlides.value.push({ title: '', keyPointsString: '' });
};

const removeSlide = (index: number) => {
  editableSlides.value.splice(index, 1);
};

const saveOutline = async () => {
  const slides: OutlineSlide[] = editableSlides.value.map(s => ({
    title: s.title,
    keyPoints: s.keyPointsString.split('\n').filter(p => p.trim() !== '')
  }));
  await outlineStore.updateOutline(projectId, slides);
};

const approveOutline = async () => {
  if (confirm('Are you sure you want to approve this outline? This will enable slide generation.')) {
    await outlineStore.approveOutline(projectId);
  }
};
</script>

<style scoped>
.outline-page {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
}

.slide-edit {
  border: 1px solid #ccc;
  padding: 15px;
  margin-bottom: 15px;
  border-radius: 4px;
}

.slide-edit input {
  display: block;
  width: 100%;
  margin-bottom: 10px;
  padding: 8px;
}

.slide-edit textarea {
  display: block;
  width: 100%;
  height: 80px;
  margin-bottom: 10px;
  padding: 8px;
}

.approve-btn {
  background-color: #42b983;
  color: white;
  margin-left: 10px;
}

.error {
  color: red;
}
</style>
