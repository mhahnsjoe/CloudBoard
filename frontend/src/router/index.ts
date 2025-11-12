import { createRouter, createWebHistory } from 'vue-router'
import ProjectDetailView from "../components/ProjectDetailView.vue";
import SummaryView from '@/components/SummaryView.vue';
import BoardDetailView from '@/components/BoardDetailView.vue';

const routes = [
  {
    path: "/",
    name: "Sumary",
    component: SummaryView,
  },
  {
    path: "/projects/:id",
    name: "ProjectDetail",
    component: ProjectDetailView,
    props: true,
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