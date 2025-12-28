import { defineStore } from 'pinia';
import apiClient from '../app/api/client';

export interface Project {
  id: string;
  name: string;
  createdAt: string;
  updatedAt: string;
  deck?: {
    draftOutline?: { status: string };
    draftStyleBrief?: { status: string };
  };
}

export const useProjectsStore = defineStore('projects', {
  state: () => ({
    projects: [] as Project[],
    currentProject: null as Project | null,
    loading: false,
    error: null as string | null,
  }),
  actions: {
    async fetchProjects() {
      this.loading = true;
      try {
        const response = await apiClient.get<Project[]>('/projects');
        this.projects = response.data;
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
    async fetchProject(id: string) {
      this.loading = true;
      try {
        const response = await apiClient.get<Project>(`/projects/${id}`);
        this.currentProject = response.data;
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
    async createProject(name: string) {
      this.loading = true;
      try {
        const response = await apiClient.post<Project>('/projects', { name });
        this.projects.push(response.data);
        return response.data;
      } catch (err: any) {
        this.error = err.message;
        throw err;
      } finally {
        this.loading = false;
      }
    },
  },
});
