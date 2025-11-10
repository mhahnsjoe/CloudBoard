import { createRouter, createWebHistory } from 'vue-router'
import ProjectDetailView from "../components/ProjectDetailView.vue";
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
    component: ProjectDetailView,
    props: true, // Pass route param `id` as prop
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

export default router
