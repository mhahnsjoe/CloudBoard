<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <!-- Header -->
    <div class="flex justify-between items-center mb-8">
      <h1 class="text-3xl font-bold text-gray-800">Projects</h1>
      <button
        @click="openCreateModal"
        class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition"
      >
        + New Project
      </button>
    </div>

    <!-- Projects grid -->
    <div v-if="projectsList.length" class="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <btn v-for="project in projectsList" :key="project.id" @click="viewProject(project.id)" class="bg-white shadow-sm hover:shadow-md rounded-2xl p-5 border border-gray-100 transition cursor-pointer">
            <div class="flex justify-between items-start">
                <h2 class="text-xl font-semibold text-gray-800">
                    {{ project.name }}
                </h2>
                <span class="text-sm text-gray-500 bg-gray-100 px-2 py-1 rounded-lg">
                    {{ project.tasks?.length ?? 0 }} tasks
                </span>
            </div>

            <div class="mt-4 flex gap-2">
                <button @click="editProject(project)" class="px-3 py-1 bg-yellow-100 text-yellow-700 rounded-md hover:bg-yellow-200 transition">
                    Edit
                </button>
                <button @click="deleteProjectConfirm(project.id)" class="px-3 py-1 bg-red-100 text-red-700 rounded-md hover:bg-red-200 transition">
                    Delete
                </button>
            </div>
        </btn>
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

    <!-- Empty state -->
    <div v-else class="text-center text-gray-500 mt-20">
      <p>No projects yet. Create your first one!</p>
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