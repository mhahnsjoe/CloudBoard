import { createRouter, createWebHistory } from 'vue-router'
import ProjectsList from "../components/ProjectsList.vue";
import ProjectDetail from "../components/ProjectDetail.vue";

const routes = [
  {
    path: "/",
    name: "ProjectsList",
    component: ProjectsList,
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
