<template>
  <div class="container">
    <div class="header">
      <h1>Projects</h1>
      <button class="btn btn-primary" @click="openCreateModal">
        + New Project
      </button>
    </div>

    <div v-if="loading" class="loading">Loading projects...</div>

    <ul v-else class="project-list">
      <li v-for="project in projectsList" :key="project.id" class="project-item">
        <div class="project-info">
          <h2>{{ project.name }}</h2>
          <p>{{ project.tasks.length }} tasks</p>
        </div>

        <div class="actions">
          <button class="btn btn-light" @click="viewProject(project.id)">
            View
          </button>
          <button class="btn btn-warning" @click="editProject(project)">
            Edit
          </button>
          <button class="btn btn-danger" @click="deleteProjectConfirm(project.id)">
            Delete
          </button>
        </div>
      </li>
    </ul>

    <!-- Create/Edit Modal -->
    <div v-if="showModal" class="modal-overlay">
      <div class="modal">
        <h2>{{ isEditing ? "Edit Project" : "Create Project" }}</h2>
        <input
          v-model="form.name"
          type="text"
          placeholder="Project name"
          class="input"
        />

        <div class="modal-actions">
          <button class="btn btn-light" @click="closeModal">Cancel</button>
          <button class="btn btn-primary" @click="submitForm">
            {{ isEditing ? "Update" : "Create" }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted } from "vue";
import {
  getProjects,
  createProject,
  updateProject,
  deleteProject,
} from "../services/api";
import type { Project } from "../types/dbo";
import { useRouter } from "vue-router";

export default defineComponent({
  name: "ProjectsList",
  setup() {
    const projectsList = ref<Project[]>([]);
    const loading = ref(true);
    const showModal = ref(false);
    const isEditing = ref(false);
    const form = ref<{ id?: number; name: string }>({ name: "" });

    const fetchProjects = async () => {
      loading.value = true;
      const res = await getProjects();
      projectsList.value = res.data;
      loading.value = false;
    };

    const openCreateModal = () => {
      isEditing.value = false;
      form.value = { name: "" };
      showModal.value = true;
    };

    const editProject = (project: Project) => {
      isEditing.value = true;
      form.value = { id: project.id, name: project.name };
      showModal.value = true;
    };

    const submitForm = async () => {
      if (isEditing.value && form.value.id) {
        await updateProject(form.value.id, form.value as Project);
      } else {
        await createProject(form.value as Project);
      }
      await fetchProjects();
      closeModal();
    };

    const deleteProjectConfirm = async (id: number) => {
      if (confirm("Are you sure you want to delete this project?")) {
        await deleteProject(id);
        await fetchProjects();
      }
    };

    const router = useRouter();
      const viewProject = (id: number) => {
        router.push(`/projects/${id}`);
    };

    const closeModal = () => (showModal.value = false);

    onMounted(fetchProjects);

    return {
      projectsList,
      loading,
      showModal,
      isEditing,
      form,
      openCreateModal,
      editProject,
      deleteProjectConfirm,
      submitForm,
      viewProject,
      closeModal,
    };
  },
});
</script>

<style scoped>
/* Layout */
.container {
  max-width: 800px;
  margin: 2rem auto;
  padding: 1rem;
  font-family: Arial, sans-serif;
  background: #fafafa;
  border-radius: 8px;
}

/* Header */
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

/* Project list */
.project-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.project-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: white;
  padding: 1rem;
  border-radius: 6px;
  margin-bottom: 0.75rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.project-info h2 {
  margin: 0;
  font-size: 1.1rem;
}

.project-info p {
  margin: 0.2rem 0 0;
  color: #666;
  font-size: 0.9rem;
}
</style>
