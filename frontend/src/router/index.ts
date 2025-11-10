import { createRouter, createWebHistory } from 'vue-router'
import ProjectDetail from "../components/ProjectDetail.vue";
import ProjectsView from '@/components/ProjectsView.vue';

const routes = [
  {
    path: "/",
    name: "ProjectsList",
    component: ProjectsView,
  },
  {
    path: "/projects/:id",
    name: "ProjectDetail",
    component: ProjectDetail,
    props: true, // Pass route param `id` as prop
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

export default router
