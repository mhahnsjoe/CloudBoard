// frontend/src/composables/useProjects.ts
import { ref } from 'vue';
import { getProjects, createProject, updateProject, deleteProject } from '@/services/api';
import type { Project, ProjectCreate } from '@/types/Project';

export function useProjects() {
  const projects = ref<Project[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const fetchProjects = async () => {
    loading.value = true;
    error.value = null;
    try {
      const res = await getProjects();
      projects.value = res.data;
    } catch (err) {
      error.value = 'Failed to fetch projects';
      console.error(err);
    } finally {
      loading.value = false;
    }
  };

  const addProject = async (project: ProjectCreate) => {
    try {
      await createProject(project);
      await fetchProjects();
    } catch (err) {
      error.value = 'Failed to create project';
      console.error(err);
      throw err;
    }
  };

  const modifyProject = async (id: number, project: ProjectCreate) => {
    try {
      await updateProject(id, project);
      await fetchProjects();
    } catch (err) {
      error.value = 'Failed to update project';
      console.error(err);
      throw err;
    }
  };

  const removeProject = async (id: number) => {
    try {
      await deleteProject(id);
      await fetchProjects();
    } catch (err) {
      error.value = 'Failed to delete project';
      console.error(err);
      throw err;
    }
  };

  return {
    projects,
    loading,
    error,
    fetchProjects,
    addProject,
    modifyProject,
    removeProject
  };
}