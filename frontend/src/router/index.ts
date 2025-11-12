import { createRouter, createWebHistory } from 'vue-router'
import SummaryView from '@/components/SummaryView.vue';
import BoardDetailView from '@/components/BoardDetailView.vue';

const routes = [
  {
    path: "/",
    name: "Summary",
    component: SummaryView,
  },
  {
    path: "/projects/:projectId/boards/:boardId",
    name: "BoardDetail",
    component: BoardDetailView,
    props: true,
  }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

export default router