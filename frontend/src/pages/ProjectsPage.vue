<template>
  <div class="projects-page">
    <h1>Projects</h1>

    <div class="create-project">
      <input v-model="newProjectName" placeholder="Project Name" @keyup.enter="createProject" />
      <button @click="createProject" :disabled="!newProjectName || projectsStore.loading">Create Project</button>
    </div>

    <div v-if="projectsStore.loading">Loading...</div>
    <div v-else-if="projectsStore.error" class="error">{{ projectsStore.error }}</div>

    <ul v-else class="project-list">
      <li v-for="project in projectsStore.projects" :key="project.id">
        <router-link :to="{ name: 'outline', params: { projectId: project.id } }">
          {{ project.name }}
        </router-link>
        <span class="date">{{ new Date(project.createdAt).toLocaleString() }}</span>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useProjectsStore } from '../stores/projectsStore';
import { useRouter } from 'vue-router';

const projectsStore = useProjectsStore();
const router = useRouter();
const newProjectName = ref('');

onMounted(() => {
  projectsStore.fetchProjects();
});

const createProject = async () => {
  if (!newProjectName.value) return;
  try {
    const project = await projectsStore.createProject(newProjectName.value);
    newProjectName.value = '';
    router.push({ name: 'outline', params: { projectId: project.id } });
  } catch (err) {
    // Error handled by store
  }
};
</script>

<style scoped>
.projects-page {
  max-width: 800px;
  margin: 0 auto;
  padding: 20px;
}

.create-project {
  margin-bottom: 20px;
}

.create-project input {
  padding: 8px;
  margin-right: 10px;
  width: 200px;
}

.project-list {
  list-style: none;
  padding: 0;
}

.project-list li {
  padding: 10px;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
}

.date {
  color: #888;
  font-size: 0.9em;
}

.error {
  color: red;
}
</style>
