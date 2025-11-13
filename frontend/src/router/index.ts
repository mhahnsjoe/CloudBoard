import { createRouter, createWebHistory } from 'vue-router'
import SummaryView from '@/components/SummaryView.vue';
import BoardDetailView from '@/components/BoardDetailView.vue';
import LoginView from '@/components/LoginView.vue';
import RegisterView from '@/components/RegisterView.vue';
import { useAuthStore } from '@/stores/auth';

const routes = [
  {
    path: "/login",
    name: "Login",
    component: LoginView,
    meta: { requiresAuth: false }
  },
  {
    path: "/register",
    name: "Register",
    component: RegisterView,
    meta: { requiresAuth: false }
  },
  {
    path: "/",
    name: "Summary",
    component: SummaryView,
    meta: { requiresAuth: true }
  },
  {
    path: "/projects/:projectId/boards/:boardId",
    name: "BoardDetail",
    component: BoardDetailView,
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
    // Redirect to login if accessing protected route without authentication
    next({ name: 'Login' })
  } else if ((to.name === 'Login' || to.name === 'Register') && authStore.isAuthenticated) {
    // Redirect to home if accessing login/register while authenticated
    next({ name: 'Summary' })
  } else {
    next()
  }
})

export default router