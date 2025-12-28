import { createRouter, createWebHistory } from 'vue-router';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('../pages/ProjectsPage.vue'),
    },
    {
      path: '/projects/:projectId/outline',
      name: 'outline',
      component: () => import('../pages/ConversationalOutlinePage.vue'),
    },
    {
      path: '/projects/:projectId/outline/manual',
      name: 'outline-manual',
      component: () => import('../pages/OutlinePage.vue'),
    },
    {
      path: '/projects/:projectId/style-brief',
      name: 'style-brief',
      component: () => import('../pages/StyleBriefPage.vue'),
    },
    {
      path: '/projects/:projectId/generate',
      name: 'generate',
      component: () => import('../pages/GeneratePage.vue'),
    },
    {
      path: '/projects/:projectId/preview',
      name: 'preview',
      component: () => import('../pages/PreviewPage.vue'),
    },
    {
      path: '/projects/:projectId/export',
      name: 'export',
      component: () => import('../pages/ExportPage.vue'),
    },
    {
      path: '/projects/:projectId/assets',
      name: 'assets',
      component: () => import('../pages/AssetsPage.vue'),
    },
  ],
});

export default router;
