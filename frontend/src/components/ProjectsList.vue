<template>
  <div class="container">
    <div class="header">
      <h1>Projects</h1>
      <button @click="openCreateModal">+ New Project</button>
    </div>
    <div v-if="loading" class="loading">Loading projects...</div>

    <ul v-else class="project-list">
      <li v-for="project in projectsList" :key="project.id" class="project-item">
        <div class="project-info">
          <h2>{{ project.name }}</h2>
          <p>{{ project.tasks?.length ?? 0 }} tasks</p>
        </div>
        <div class="actions">
          <button class="btn btn-light" @click="viewProject(project.id)">
            View
          </button>
          <button class="btn btn-light" @click="editProject(project)">
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
import { ref, onMounted, defineComponent } from "vue";
import { useRouter } from "vue-router";
import {
  getProjects,
  createProject,
  updateProject,
  deleteProject,
} from "../services/api";
import type { Project } from "../types/Project";

export default defineComponent({
  name: "ProjectsList",
  setup() {
    const projectsList = ref<Project[]>([]);
    const loading = ref(true);
    const showModal = ref(false);
    const isEditing = ref(false);

    const form = ref<{
      id?: number;
      name: string;
      description: string;
    }>({ name: "", description: "" });

    const router = useRouter();

    const fetchProjects = async () => {
      loading.value = true;
      const res = await getProjects();
      projectsList.value = res.data;
      loading.value = false;
    };

    const openCreateModal = () => {
      isEditing.value = false;
      form.value = { name: "", description: "" };
      showModal.value = true;
    };

    const editProject = (project: Project) => {
      isEditing.value = true;
      form.value = {
        id: project.id,
        name: project.name,
        description: project.description,
      };
      showModal.value = true;
    };

    const submitForm = async () => {
      if (isEditing.value && form.value.id != null) {
        await updateProject(form.value.id, {
          id: form.value.id,
          name: form.value.name,
          description: form.value.description,
        });
      } else {
        await createProject({
          name: form.value.name,
          description: form.value.description,
        });
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
.container {
  max-width: 800px;
  margin: 2rem auto;
  padding: 1rem;
  font-family: Arial, sans-serif;
  background: #fafafa;
  border-radius: 8px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}
</style>
