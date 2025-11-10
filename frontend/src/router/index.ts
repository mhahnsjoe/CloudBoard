import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/projects/:id',
      name: 'ProjectDetail',
      component: () => import('../components/ProjectDetail.vue'),
      props: (route) => ({ id: Number(route.params.id) }),
    }

  ],
})

export default router
