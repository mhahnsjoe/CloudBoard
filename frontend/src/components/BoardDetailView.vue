<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <div class="mb-6">
      <button class="btn btn-light flex items-center gap-2" @click="$router.push(`/projects/${projectId}`)">
        <ArrowLeftIcon />
        Back to Project
      </button>
    </div>

    <div v-if="loading" class="flex items-center justify-center gap-2 mt-8">
      <LoadingIcon className="h-5 w-5 text-blue-600" />
      Loading board...
    </div>

    <div v-else>
      <div class="flex justify-between items-center mb-6">
        <div>
          <h1 class="text-3xl font-bold text-gray-800">{{ board?.name }}</h1>
          <!-- <p v-if="board?.description" class="text-gray-600 mt-1">{{ board.description }}</p> -->
        </div>
      </div>

      <!-- Kanban Board -->
      <div class="grid grid-cols-3 gap-6">
        <div
          v-for="status in STATUSES"
          :key="status"
          class="bg-gray-100 rounded-lg p-4"
        >
          <!-- Column Header -->
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-lg font-semibold text-gray-800 flex items-center gap-2">
              <span :class="getStatusIconClass(status)">●</span>
              {{ status }}
            </h2>
            <div class="flex items-center gap-2">
              <span class="text-sm text-gray-500 bg-white px-2 py-1 rounded-full">
                {{ getTasksByStatus(status).length }}
              </span>
              <button
                @click="openCreateModalWithStatus(status)"
                class="p-1 hover:bg-white rounded transition-colors text-gray-600 hover:text-blue-600"
                title="Add task"
              >
                <PlusIcon className="w-4 h-4" />
              </button>
            </div>
          </div>

          <!-- Drop Zone -->
          <div
            @drop="onDrop($event, status)"
            @dragover.prevent
            @dragenter.prevent
            class="min-h-[500px] space-y-3"
          >
            <KanbanCard
              v-for="task in getTasksByStatus(status)"
              :key="task.id"
              :task="task"
              @dragstart="onDragStart($event, task)"
              @edit="editTask(task)"
              @delete="handleDelete(task.id)"
            />
            <!-- Empty State -->
            <div
              v-if="getTasksByStatus(status).length === 0"
              class="flex items-center justify-center h-32 text-gray-400 text-sm"
            >
              Drop tasks here
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Task Modal -->
    <TaskModal
      v-if="showModal"
      :task="selectedTask"
      :boardId="boardId"
      :defaultStatus="defaultStatus"
      @close="closeModal"
      @save="handleSave"
    />
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { getBoard, createTask, updateTask, deleteTask } from '@/services/api';
import { useConfirm } from '@/composables/useConfirm';
import type { TaskItem, Board, TaskCreate } from '@/types/Project';
import { STATUSES } from '@/types/Project';

import KanbanCard from './kanban/KanbanCard.vue'
import TaskModal from './task/TaskModal.vue';
import { 
  ArrowLeftIcon, 
  LoadingIcon, 
  PlusIcon, 
} from './icons';

export default defineComponent({
  name: 'BoardDetailView',
  components: {
    TaskModal,
    ArrowLeftIcon,
    LoadingIcon,
    PlusIcon,
    KanbanCard
  },
  setup() {
    const route = useRoute();
    const projectId = ref(Number(route.params.projectId));
    const boardId = ref(Number(route.params.boardId));
    const { confirm } = useConfirm();

    const board = ref<Board>();
    const tasks = ref<TaskItem[]>([]);
    const loading = ref(false);
    const showModal = ref(false);
    const selectedTask = ref<TaskItem | null>(null);
    const draggedTask = ref<TaskItem | null>(null);
    const defaultStatus = ref<string>('To Do');

    const fetchBoard = async () => {
      loading.value = true;
      try {
        const res = await getBoard(projectId.value, boardId.value);
        board.value = res.data;
        tasks.value = res.data.tasks || [];
      } catch (error) {
        console.error('Failed to fetch board:', error);
      } finally {
        loading.value = false;
      }
    };

    const getTasksByStatus = (status: string) => {
      return tasks.value.filter(task => task.status === status);
    };

    const getStatusIconClass = (status: string) => {
      const classes: Record<string, string> = {
        'To Do': 'text-gray-500',
        'In Progress': 'text-blue-500',
        'Done': 'text-green-500'
      };
      return classes[status] || 'text-gray-500';
    };

    const getPriorityClass = (priority: string) => {
      const classes: Record<string, string> = {
        'Low': 'bg-gray-100 text-gray-700',
        'Medium': 'bg-blue-100 text-blue-700',
        'High': 'bg-orange-100 text-orange-700',
        'Critical': 'bg-red-100 text-red-700'
      };
      return classes[priority] || 'bg-gray-100 text-gray-700';
    };

    const formatDate = (dateString: string) => {
      const date = new Date(dateString);
      const today = new Date();
      const isOverdue = date < today && date.toDateString() !== today.toDateString();
      
      const formatted = date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
      return isOverdue ? `⚠️ ${formatted}` : formatted;
    };

    const onDragStart = (event: DragEvent, task: TaskItem) => {
      draggedTask.value = task;
      if (event.dataTransfer) {
        event.dataTransfer.effectAllowed = 'move';
      }
    };

    const openCreateModal = () => {
      selectedTask.value = null;
      defaultStatus.value = 'To Do';
      showModal.value = true;
    };

    const openCreateModalWithStatus = (status: string) => {
      selectedTask.value = null;
      defaultStatus.value = status;
      showModal.value = true;
    };

    const editTask = (task: TaskItem) => {
      selectedTask.value = task;
      showModal.value = true;
    };

    const closeModal = () => {
      showModal.value = false;
      selectedTask.value = null;
    };

    const onDrop = async (event: DragEvent, newStatus: string) => {
      event.preventDefault();
      if (draggedTask.value && draggedTask.value.status !== newStatus) {
        try {
          await updateTask(boardId.value, draggedTask.value.id, {
            ...draggedTask.value,
            status: newStatus
          });
          await fetchBoard();
        } catch (error) {
          console.error('Failed to update task status:', error);
        }
      }
      draggedTask.value = null;
    };

    const handleSave = async (taskData: TaskItem | TaskCreate) => {
      try {
        if ('id' in taskData && taskData.id) {
          await updateTask(boardId.value, taskData.id, taskData as TaskItem);
        } else {
          await createTask(boardId.value, taskData as TaskCreate);
        }
        closeModal();
        await fetchBoard();
      } catch (error) {
        console.error('Failed to save task:', error);
      }
    };

    const handleDelete = async (id: number) => {
      if (confirm('Are you sure you want to delete this task?')) {
        try {
          await deleteTask(boardId.value, id);
          await fetchBoard();
        } catch (error) {
          console.error('Failed to delete task:', error);
        }
      }
    };

    onMounted(fetchBoard);

    watch(
      () => route.params.boardId,
      (newBoardId) => {
        boardId.value = Number(newBoardId);
        fetchBoard();
      }
    );

    return {
      projectId,
      boardId,
      board,
      tasks,
      loading,
      showModal,
      selectedTask,
      defaultStatus,
      STATUSES,
      getTasksByStatus,
      getStatusIconClass,
      getPriorityClass,
      formatDate,
      onDragStart,
      onDrop,
      openCreateModal,
      openCreateModalWithStatus,
      editTask,
      closeModal,
      handleSave,
      handleDelete
    };
  }
});
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>