import { defineStore } from 'pinia';
import { ref } from 'vue';
import apiClient from '../app/api/client';
import { createJobsHubConnection } from '../app/realtime/jobsHub';

export interface Job {
  id: string;
  projectId: string;
  status: 'Queued' | 'Running' | 'Succeeded' | 'Failed' | 'Stopped';
  stage: string;
  createdAt: string;
  updatedAt: string;
  error?: string;
  inputSummary?: string;
}

export const useJobsStore = defineStore('jobs', () => {
  const currentJob = ref<Job | null>(null);
  const projectJobs = ref<Job[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);
  let hubConnection: any = null;

  const fetchProjectJobs = async (projectId: string) => {
    loading.value = true;
    try {
      const response = await apiClient.get(`/jobs/project/${projectId}`);
      projectJobs.value = response.data;
    } catch (err: any) {
      error.value = err.message;
    } finally {
      loading.value = false;
    }
  };

  const startJob = async (projectId: string) => {
    loading.value = true;
    try {
      const response = await apiClient.post('/jobs', { projectId });
      currentJob.value = response.data;
      projectJobs.value.unshift(response.data);
      return response.data;
    } catch (err: any) {
      error.value = err.message;
      throw err;
    } finally {
      loading.value = false;
    }
  };

  const subscribeToProject = (projectId: string) => {
    if (hubConnection) {
      hubConnection.stop();
    }

    hubConnection = createJobsHubConnection(projectId);

    hubConnection.connection.on('JobStatusUpdated', (job: Job) => {
      if (currentJob.value?.id === job.id) {
        currentJob.value = job;
      }
      const index = projectJobs.value.findIndex(j => j.id === job.id);
      if (index !== -1) {
        projectJobs.value[index] = job;
      } else {
        projectJobs.value.unshift(job);
      }
    });

    hubConnection.start();
  };

  const unsubscribe = () => {
    if (hubConnection) {
      hubConnection.stop();
      hubConnection = null;
    }
  };

  return {
    currentJob,
    projectJobs,
    loading,
    error,
    fetchProjectJobs,
    startJob,
    subscribeToProject,
    unsubscribe
  };
});
