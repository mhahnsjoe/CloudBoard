<template>
  <div class="container">
    <button class="btn btn-light flex items-center gap-2" @click="$router.push('/')">
      <ArrowLeftIcon />
      Back to Projects
    </button>

    <div v-if="loading" class="loading flex items-center justify-center gap-2 mt-8">
      <LoadingIcon className="h-5 w-5 text-blue-600" />
      Loading project...
    </div>

    <div v-else>
      <h1 class="text-3xl font-bold text-gray-800 mt-6 mb-2">{{ project?.name }}</h1>
      <p v-if="project?.description" class="text-gray-600 mb-6">{{ project.description }}</p>

      <ProjectHeader 
        title="Tasks" 
        buttonText="New Task"
        @create="openCreateModal"
      />

      <ul v-if="tasks.length" class="task-list">
        <TaskItem
          v-for="task in tasks"
          :key="task.id"
          :task="task"
          :statusOptions="statusOptions"
          @edit="editTask"
          @delete="handleDeleteTask"
          @statusChange="handleStatusChange"
        />
      </ul>

      <EmptyState
        v-else
        :icon="ClipboardIcon"
        message="No tasks yet. Create your first one!"
      />
    </div>

    <!-- Modal -->
    <Modal
      :show="modal.isOpen.value"
      :title="modal.isEditing.value ? 'Edit Task' : 'Create Task'"
      :submitText="modal.isEditing.value ? 'Update' : 'Create'"
      @close="modal.close"
      @submit="submitForm"
    >
      <input
        v-model="form.title"
        type="text"
        placeholder="Task title"
        class="input"
      />
      <select v-model="form.status" class="input">
        <option v-for="s in statusOptions" :key="s">{{ s }}</option>
      </select>
    </Modal>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted, watch } from "vue";
import { useRoute } from "vue-router";
import { useTasks } from "@/composables/useTasks";
import { useModal } from "@/composables/useModal";
import { useConfirm } from "@/composables/useConfirm";
import type { TaskItem as TaskItemType } from "@/types/Project";

import ProjectHeader from "@/components/project/ProjectHeader.vue";
import TaskItem from "@/components/task/TaskItem.vue";
import Modal from "@/components/common/Modal.vue";
import EmptyState from "@/components/common/EmptyState.vue";
import { ArrowLeftIcon, LoadingIcon, SearchIcon } from "@/components/icons";

// Placeholder for ClipboardIcon
const ClipboardIcon = SearchIcon; // Replace with actual ClipboardIcon

export default defineComponent({
  name: "ProjectDetail",
  components: {
    ProjectHeader,
    TaskItem,
    Modal,
    EmptyState,
    ArrowLeftIcon,
    LoadingIcon
  },
  setup() {
    const route = useRoute();
    const projectId = ref(Number(route.params.id));
    const { project, tasks, loading, fetchTasks, addTask, modifyTask, removeTask, updateTaskStatus } = useTasks(projectId.value);
    const modal = useModal<TaskItemType>();
    const { confirm } = useConfirm();

    const form = ref<{ id?: number; title: string; status: string; priority: string }>({
      title: "",
      status: "To Do",
      priority: "Medium"
    });

    const statusOptions = ["To Do", "In Progress", "Done"];

    const openCreateModal = () => {
      form.value = { title: "", status: "To Do" , priority: "Medium"};
      modal.open();
    };

    const editTask = (task: TaskItemType) => {
      form.value = { id: task.id, title: task.title, status: task.status, priority: task.priority };
      modal.open(task);
    };

    const submitForm = async () => {
      try {
        if (modal.isEditing.value && form.value.id) {
          await modifyTask(form.value.id, {
            id: form.value.id,
            title: form.value.title,
            status: form.value.status,
            projectId: projectId.value,
            priority: form.value.priority
          });
        } else {
          await addTask({
            title: form.value.title,
            status: form.value.status,
            projectId: projectId.value,
            priority: form.value.priority
          });
        }
        modal.close();
      } catch (error) {
        console.error('Failed to submit form:', error);
      }
    };

    const handleDeleteTask = async (id: number) => {
      if (confirm("Are you sure you want to delete this task?")) {
        await removeTask(id);
      }
    };

    const handleStatusChange = async (task: TaskItemType, newStatus: string) => {
      await updateTaskStatus(task, newStatus);
    };

    onMounted(fetchTasks);

    watch(
      () => route.params.id,
      (newId) => {
        projectId.value = Number(newId);
        fetchTasks();
      }
    );

    return {
      project,
      tasks,
      loading,
      modal,
      form,
      statusOptions,
      openCreateModal,
      editTask,
      handleDeleteTask,
      handleStatusChange,
      submitForm,
      ClipboardIcon
    };
  }
});
</script>

<style scoped>
.container {
  max-width: 900px;
  margin: 2rem auto;
  padding: 1rem;
  font-family: Arial, sans-serif;
}

.task-list {
  list-style: none;
  padding: 0;
  margin: 0;
}
</style>