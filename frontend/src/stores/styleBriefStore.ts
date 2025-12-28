import { defineStore } from 'pinia';
import apiClient from '../app/api/client';

export interface StyleBriefFields {
  visualTone: string;
  audienceContext: string;
  density: string;
  emphasis: string;
  contrast: string;
  typographyHierarchy: string;
  layoutRhythm: string;
  motionFeel: string;
  mediaTreatment: string;
  brandConstraints: string;
}

export interface StyleBrief {
  id: string;
  deckId: string;
  status: string;
  fields: StyleBriefFields;
  createdAt: string;
}

export const useStyleBriefStore = defineStore('styleBrief', {
  state: () => ({
    styleBrief: null as StyleBrief | null,
    loading: false,
    error: null as string | null,
  }),
  actions: {
    async fetchStyleBrief(projectId: string) {
      this.loading = true;
      try {
        const response = await apiClient.get<StyleBrief>(`/projects/${projectId}/style-brief`);
        this.styleBrief = response.data;
      } catch (err: any) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
    async updateStyleBrief(projectId: string, fields: StyleBriefFields) {
      this.loading = true;
      try {
        const response = await apiClient.put<StyleBrief>(`/projects/${projectId}/style-brief`, { fields });
        this.styleBrief = response.data;
      } catch (err: any) {
        this.error = err.message;
        throw err;
      } finally {
        this.loading = false;
      }
    },
    async approveStyleBrief(projectId: string) {
      this.loading = true;
      try {
        const response = await apiClient.post<StyleBrief>(`/projects/${projectId}/style-brief/approve`);
        this.styleBrief = response.data;
      } catch (err: any) {
        this.error = err.message;
        throw err;
      } finally {
        this.loading = false;
      }
    },
  },
});
