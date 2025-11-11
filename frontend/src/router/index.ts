import { createRouter, createWebHistory } from 'vue-router'
import ProjectDetailView from "../components/ProjectDetailView.vue";
import ProjectsView from '@/components/ProjectsView.vue';
// import WorkItemsView from '@/components/WorkItemsView.vue';
import BoardDetailView from '@/components/BoardDetailView.vue';

const routes = [
  {
    path: "/",
    name: "ProjectsList",
    component: ProjectsView,
  },
  // {
  //   path: "/work-items",
  //   name: "WorkItems",
  //   component: WorkItemsView,
  // },
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