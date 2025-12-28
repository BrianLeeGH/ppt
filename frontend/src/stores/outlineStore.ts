import { defineStore } from 'pinia';
import apiClient from '../app/api/client';

export interface OutlineSlide {
  title: string;
  keyPoints: string[];
}

export interface Outline {
  id: string;
  deckId: string;
  status: string;
  slides: OutlineSlide[];
  createdAt: string;
}

export const useOutlineStore = defineStore('outline', {
  state: () => ({
    outline: null as Outline | null,
    loading: false,
    error: null as string | null,
  }),
  actions: {
    async fetchOutline(projectId: string) {
      this.loading = true;
      try {
        const response = await apiClient.get<Outline>(`/projects/${projectId}/outline`);
        this.outline = response.data;
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
    async updateOutline(projectId: string, slides: OutlineSlide[]) {
      this.loading = true;
      try {
        const response = await apiClient.put<Outline>(`/projects/${projectId}/outline`, { slides });
        this.outline = response.data;
      } catch (err: any) {
        this.error = err.message;
        throw err;
      } finally {
        this.loading = false;
      }
    },
    async approveOutline(projectId: string) {
      this.loading = true;
      try {
        const response = await apiClient.post<Outline>(`/projects/${projectId}/outline/approve`);
        this.outline = response.data;
      } catch (err: any) {
        this.error = err.message;
        throw err;
      } finally {
        this.loading = false;
      }
    },
  },
});
