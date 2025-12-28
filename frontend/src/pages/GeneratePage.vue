<script setup lang="ts">
import { onMounted, onUnmounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useJobsStore } from '../stores/jobsStore';
import { useProjectsStore } from '../stores/projectsStore';

const route = useRoute();
const router = useRouter();
const jobsStore = useJobsStore();
const projectsStore = useProjectsStore();

const projectId = route.params.projectId as string;

onMounted(async () => {
  await projectsStore.fetchProject(projectId);
  await jobsStore.fetchProjectJobs(projectId);
  jobsStore.subscribeToProject(projectId);
});

onUnmounted(() => {
  jobsStore.unsubscribe();
});

const latestJob = computed(() => jobsStore.projectJobs[0]);

const canStartJob = computed(() => {
  const project = projectsStore.currentProject;
  if (!project || !project.deck) return false;
  return project.deck.draftOutline?.status === 'Approved' &&
         project.deck.draftStyleBrief?.status === 'Approved';
});

const startGeneration = async () => {
  try {
    await jobsStore.startJob(projectId);
  } catch (err) {
    console.error('Failed to start generation', err);
  }
};

const viewPreview = () => {
  router.push(`/projects/${projectId}/preview`);
};
</script>

<template>
  <div class="generate-page">
    <header class="page-header">
      <router-link :to="`/projects/${projectId}/style-brief`" class="back-link">← Back to Style Brief</router-link>
      <h1>Generate Presentation</h1>
    </header>

    <div v-if="projectsStore.loading" class="loading">Loading project...</div>

    <div v-else-if="projectsStore.currentProject" class="content">
      <section class="status-card">
        <h2>Generation Status</h2>

        <div v-if="!latestJob" class="no-job">
          <p v-if="canStartJob">Ready to generate your presentation!</p>
          <p v-else class="warning">Outline and Style Brief must be approved before generating.</p>
          <button
            @click="startGeneration"
            :disabled="!canStartJob || jobsStore.loading"
            class="primary-button"
          >
            Start Generation
          </button>
        </div>

        <div v-else class="job-details">
          <div class="status-badge" :class="latestJob.status.toLowerCase()">
            {{ latestJob.status }}
          </div>

          <div class="progress-info">
            <p><strong>Current Stage:</strong> {{ latestJob.stage || 'Initializing...' }}</p>
            <div v-if="latestJob.status === 'Running'" class="spinner"></div>
          </div>

          <div v-if="latestJob.error" class="error-box">
            <strong>Error:</strong> {{ latestJob.error }}
          </div>

          <div class="actions">
            <button
              v-if="latestJob.status === 'Succeeded'"
              @click="viewPreview"
              class="primary-button"
            >
              View Preview
            </button>

            <button
              v-if="latestJob.status === 'Failed' || latestJob.status === 'Succeeded' || latestJob.status === 'Stopped'"
              @click="startGeneration"
              :disabled="!canStartJob || jobsStore.loading"
              class="secondary-button"
            >
              Restart Generation
            </button>
          </div>
        </div>
      </section>

      <section class="history">
        <h3>Job History</h3>
        <table v-if="jobsStore.projectJobs.length > 0">
          <thead>
            <tr>
              <th>Date</th>
              <th>Status</th>
              <th>Stage</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="job in jobsStore.projectJobs" :key="job.id">
              <td>{{ new Date(job.createdAt).toLocaleString() }}</td>
              <td>{{ job.status }}</td>
              <td>{{ job.stage }}</td>
            </tr>
          </tbody>
        </table>
        <p v-else>No previous jobs.</p>
      </section>
    </div>
  </div>
</template>

<style scoped>
.generate-page {
  max-width: 800px;
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

.status-card {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  margin-bottom: 2rem;
  text-align: center;
}

.status-badge {
  display: inline-block;
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-weight: bold;
  margin-bottom: 1rem;
  text-transform: uppercase;
}

.status-badge.queued { background: #eee; color: #666; }
.status-badge.running { background: #e3f2fd; color: #1976d2; }
.status-badge.succeeded { background: #e8f5e9; color: #2e7d32; }
.status-badge.failed { background: #ffebee; color: #c62828; }

.progress-info {
  margin: 1.5rem 0;
}

.error-box {
  background: #ffebee;
  color: #c62828;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
  text-align: left;
}

.actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
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

.primary-button:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.secondary-button {
  background: white;
  color: #42b883;
  border: 1px solid #42b883;
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  cursor: pointer;
}

.history {
  margin-top: 3rem;
}

table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 1rem;
}

th, td {
  text-align: left;
  padding: 0.75rem;
  border-bottom: 1px solid #eee;
}

.warning {
  color: #f57c00;
  margin-bottom: 1rem;
}

.spinner {
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3498db;
  border-radius: 50%;
  width: 30px;
  height: 30px;
  animation: spin 2s linear infinite;
  margin: 1rem auto;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>
