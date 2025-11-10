<template>
  <div>
    <h1>Projects</h1>

    <button @click="openCreateModal">+ New Project</button>

    <div v-if="loading">Loading projects...</div>

    <ul v-else>
      <li v-for="project in projectsList" :key="project.id">
        <strong>{{ project.name }}</strong> — {{ project.description }}
        <button @click="viewProject(project.id)">View</button>
        <button @click="editProject(project)">Edit</button>
        <button @click="deleteProjectConfirm(project.id)">Delete</button>
      </li>
    </ul>

    <!-- Modal -->
    <div v-if="showModal" class="modal">
      <div class="modal-content">
        <h2>{{ isEditing ? "Edit Project" : "Create Project" }}</h2>
        <form @submit.prevent="submitForm">
          <div>
            <label>Name</label>
            <input v-model="form.name" required />
          </div>
          <div>
            <label>Description</label>
            <textarea v-model="form.description"></textarea>
          </div>
          <button type="submit">{{ isEditing ? "Update" : "Create" }}</button>
          <button type="button" @click="closeModal">Cancel</button>
        </form>
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
.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.4);
  display: flex;
  justify-content: center;
  align-items: center;
}

.modal-content {
  background: white;
  padding: 1.5rem;
  border-radius: 6px;
  min-width: 300px;
}

button {
  margin: 0 0.3rem;
}

input,
textarea {
  width: 100%;
  padding: 0.4rem 0.6rem;
  margin-bottom: 0.8rem;
  border: 1px solid #ccc;
  border-radius: 4px;
}
</style>
