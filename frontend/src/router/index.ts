import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth';

const routes = [
  {
    path: "/login",
    name: "Login",
    component: () => import('@/components/LoginView.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: "/register",
    name: "Register",
    component: () => import('@/components/RegisterView.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: "/",
    name: "Summary",
    component: () => import('@/components/SummaryView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: "/projects/:projectId/boards/:boardId",
    name: "BoardDetail",
    component: () => import('@/components/BoardDetailView.vue'),
    props: true,
    meta: { requiresAuth: true }
  },
  {
    path: "/projects/:projectId/backlog",
    name: "Backlog",
    component: () => import('@/components/BacklogView.vue'),
    props: true,
    meta: { requiresAuth: true }
  }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

// Navigation guard for authentication
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth !== false)

  if (requiresAuth && !authStore.isAuthenticated) {
    next({ name: 'Login' })
  } else if ((to.name === 'Login' || to.name === 'Register') && authStore.isAuthenticated) {
    next({ name: 'Summary' })
  } else {
    next()
  }
})

export default router