<template>
  <div class="style-brief-page">
    <div v-if="projectsStore.currentProject">
      <h1>Style Brief: {{ projectsStore.currentProject.name }}</h1>
      <div class="status">Status: {{ styleBriefStore.styleBrief?.status }}</div>

      <div v-if="styleBriefStore.loading">Loading...</div>
      <div v-else-if="styleBriefStore.error" class="error">{{ styleBriefStore.error }}</div>

      <div v-else-if="styleBriefStore.styleBrief" class="editor">
        <div class="field">
          <label>Visual Tone</label>
          <input v-model="editableFields.visualTone" />
        </div>
        <div class="field">
          <label>Audience Context</label>
          <input v-model="editableFields.audienceContext" />
        </div>
        <div class="field">
          <label>Density</label>
          <input v-model="editableFields.density" />
        </div>
        <div class="field">
          <label>Emphasis</label>
          <input v-model="editableFields.emphasis" />
        </div>
        <div class="field">
          <label>Contrast</label>
          <input v-model="editableFields.contrast" />
        </div>
        <div class="field">
          <label>Typography Hierarchy</label>
          <input v-model="editableFields.typographyHierarchy" />
        </div>
        <div class="field">
          <label>Layout Rhythm</label>
          <input v-model="editableFields.layoutRhythm" />
        </div>
        <div class="field">
          <label>Motion Feel</label>
          <input v-model="editableFields.motionFeel" />
        </div>
        <div class="field">
          <label>Media Treatment</label>
          <input v-model="editableFields.mediaTreatment" />
        </div>
        <div class="field">
          <label>Brand Constraints</label>
          <input v-model="editableFields.brandConstraints" />
        </div>

        <button @click="saveStyleBrief" :disabled="styleBriefStore.loading">Save Changes</button>
        <button @click="approveStyleBrief" :disabled="styleBriefStore.loading || styleBriefStore.styleBrief.status === 'Approved'" class="approve-btn">
          Approve Style Brief
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useProjectsStore } from '../stores/projectsStore';
import { useStyleBriefStore, type StyleBriefFields } from '../stores/styleBriefStore';

const route = useRoute();
const projectsStore = useProjectsStore();
const styleBriefStore = useStyleBriefStore();
const projectId = route.params.projectId as string;

const editableFields = ref<StyleBriefFields>({
  visualTone: '',
  audienceContext: '',
  density: '',
  emphasis: '',
  contrast: '',
  typographyHierarchy: '',
  layoutRhythm: '',
  motionFeel: '',
  mediaTreatment: '',
  brandConstraints: '',
});

onMounted(async () => {
  await projectsStore.fetchProject(projectId);
  await styleBriefStore.fetchStyleBrief(projectId);
});

watch(() => styleBriefStore.styleBrief, (newBrief) => {
  if (newBrief) {
    editableFields.value = { ...newBrief.fields };
  }
}, { immediate: true });

const saveStyleBrief = async () => {
  await styleBriefStore.updateStyleBrief(projectId, editableFields.value);
};

const approveStyleBrief = async () => {
  if (confirm('Are you sure you want to approve this style brief? This will be used for slide generation.')) {
    await styleBriefStore.approveStyleBrief(projectId);
  }
};
</script>

<style scoped>
.style-brief-page {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
}

.field {
  margin-bottom: 15px;
}

.field label {
  display: block;
  font-weight: bold;
  margin-bottom: 5px;
}

.field input {
  width: 100%;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
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
