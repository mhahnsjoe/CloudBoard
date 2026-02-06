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
    path: "/projects/:projectId/boards/:boardId/taskboard",
    name: "Taskboard",
    component: () => import('@/components/BoardDetailView.vue'),
    props: true,
    meta: { requiresAuth: true }
  },
  {
    path: "/projects/:projectId/backlog",
    name: "Backlog",
    component: () => import('@/components/backlog/BacklogView.vue'),
    props: true,
    meta: { requiresAuth: true }
  },
  {
    path: '/projects/:projectId/boards/:boardId/sprint-planning',
    name: 'SprintPlanning',
    component: () => import('@/views/SprintPlanningView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/projects/:projectId/boards/:boardId/sprints/:sprintId/summary',
    name: 'SprintSummary',
    component: () => import('@/views/SprintSummaryView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/teams',
    redirect: '/'
  },
  /*
  {
    path: '/teams',
    name: 'Teams',
    component: () => import('@/views/TeamsView.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/teams/:id',
    name: 'TeamDetail',
    component: () => import('@/views/TeamDetailView.vue'),
    meta: { requiresAuth: true }
  },
  */
  {
    path: '/invitations/accept',
    name: 'AcceptInvitation',
    component: () => import('@/views/AcceptInvitationView.vue'),
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