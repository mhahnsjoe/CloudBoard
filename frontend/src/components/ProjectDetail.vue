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
          <option value="Pending">Pending</option>
          <option value="InProgress">In Progress</option>
          <option value="Completed">Completed</option>
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
import { defineComponent, ref, onMounted } from "vue";
import {
  getProject,
  getTasks,
  createTask,
  updateTask,
  deleteTask,
} from "../services/api";
import type { Project, TaskItem } from "../types/dbo";

export default defineComponent({
  name: "ProjectDetail",
  props: {
    id: {
      type: Number,
      required: true,
    },
  },
  setup(props) {
    const project = ref<Project | null>(null);
    const tasks = ref<TaskItem[]>([]);
    const loading = ref(true);

    const showModal = ref(false);
    const isEditing = ref(false);
    const form = ref<{ id?: number; title: string; status: string }>({
      title: "",
      status: "Pending",
    });

    const fetchData = async () => {
      loading.value = true;
      const projectRes = await getProject(props.id);
      project.value = projectRes.data;

      const taskRes = await getTasks();
      // Filter tasks that belong to this project (assuming task.projectId exists)
      tasks.value = taskRes.data.filter(
        (t) => t.projectId === project.value?.id
      );
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
        await updateTask(form.value.id, form.value as TaskItem);
      } else {
        await createTask({
          ...form.value,
          projectId: project.value?.id,
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

    onMounted(fetchData);

    return {
      project,
      tasks,
      loading,
      showModal,
      isEditing,
      form,
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