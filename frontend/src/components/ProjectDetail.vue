<template>
  <div class="container">
    <button class="btn btn-light" @click="$router.push('/')">← Back</button>

    <div v-if="loading" class="loading">Loading project...</div>

    <div v-else>
      <h1>{{ project?.name }}</h1>

      <div class="header">
        <h2>Tasks</h2>
        <button class="btn btn-primary" @click="openCreateModal">
          + New Task
        </button>
      </div>

      <ul v-if="tasks.length" class="task-list">
        <li v-for="task in tasks" :key="task.id" class="task-item">
          <div class="task-info">
            <h3>{{ task.title }}</h3>
            <p>Status: {{ task.status }}</p>
          </div>

          <div class="actions">
            <button class="btn btn-warning" @click="editTask(task)">
              Edit
            </button>
            <button class="btn btn-danger" @click="deleteTaskConfirm(task.id)">
              Delete
            </button>
          </div>
        </li>
      </ul>

      <p v-else>No tasks found for this project.</p>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showModal" class="modal-overlay">
      <div class="modal">
        <h2>{{ isEditing ? "Edit Task" : "Create Task" }}</h2>
        <input
          v-model="form.title"
          type="text"
          placeholder="Task title"
          class="input"
        />
        <select v-model="form.status" class="input">
            <option v-for="s in statusOptions" :key="s">{{ s }}</option>
        </select>


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
import { defineComponent, ref, onMounted, watch } from "vue";
import { useRoute } from "vue-router";
import {
  getProject,
  createTask,
  updateTask,
  deleteTask,
} from "../services/api";
import type { Project, TaskItem } from "../types/Project";

export default defineComponent({
  name: "ProjectDetail",
  setup() {
    const route = useRoute();
    const projectId = Number(route.params.id);
    const project = ref<Project>();
    const tasks = ref<TaskItem[]>([]);
    const loading = ref(true);

    const showModal = ref(false);
    const isEditing = ref(false);
    const form = ref<{ id?: number; title: string; status: string }>({
      title: "",
      status: "Pending",
    });

    const statusOptions = ["To Do", "In Progress", "Done"];


    const fetchData = async () => {
        loading.value = true;
        const projectRes = await getProject(projectId);
        project.value = projectRes.data;
        tasks.value = project.value?.tasks || [];
        loading.value = false;
    };

    const openCreateModal = () => {
      isEditing.value = false;
      form.value = { title: "", status: "Pending" };
      showModal.value = true;
    };

    const editTask = (task: TaskItem) => {
      isEditing.value = true;
      form.value = { id: task.id, title: task.title, status: task.status };
      showModal.value = true;
    };

    const submitForm = async () => {
        if (isEditing.value && form.value.id) {
            await updateTask(form.value.id, {
                id: form.value.id,
                title: form.value.title,
                status: form.value.status,
                projectId: project.value.id!
            });
        } else {
            await createTask({
            title : form.value.title,
            status : form.value.status,
            projectId : projectId,
        } as TaskItem);
        }
        await fetchData();
        closeModal();
    };

    const deleteTaskConfirm = async (id: number) => {
      if (confirm("Delete this task?")) {
        await deleteTask(id);
        await fetchData();
      }
    };

    const closeModal = () => (showModal.value = false);

    form.value = { title: "", status: "To Do" };


    onMounted(fetchData);
    
    watch(
        () => route.params.id,
        () => {
            fetchData();
        }
    );
    return {
      project,
      tasks,
      loading,
      showModal,
      isEditing,
      form,
      statusOptions,
      openCreateModal,
      editTask,
      deleteTaskConfirm,
      submitForm,
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

/* Header */
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

/* Task list */
.task-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.task-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: white;
  padding: 1rem;
  border-radius: 6px;
  margin-bottom: 0.75rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.task-info h3 {
  margin: 0;
}

.task-info p {
  margin: 0.2rem 0 0;
  color: #666;
  font-size: 0.9rem;
}
</style>