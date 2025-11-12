<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <div v-if="loading" class="flex items-center justify-center gap-2 mt-8">
      <LoadingIcon className="h-5 w-5 text-blue-600" />
      Loading board...
    </div>

    <div v-else>
      <!-- Board Header with Selector -->
      <div class="flex justify-between items-center mb-6">
        <div class="flex items-center gap-4">
          <!-- Board Dropdown Selector -->
          <div class="relative">
            <button
              @click="showBoardDropdown = !showBoardDropdown"
              class="flex items-center gap-2 bg-white px-4 py-2.5 rounded-lg border border-gray-300 hover:border-gray-400 transition-all shadow-sm"
            >
              <FolderIcon className="w-5 h-5 text-gray-600" />
              <span class="text-xl font-bold text-gray-800">{{ board?.name }}</span>
              <svg class="w-5 h-5 text-gray-500" :class="{ 'rotate-180': showBoardDropdown }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
              </svg>
            </button>

            <!-- Board Dropdown Menu -->
            <div
              v-if="showBoardDropdown"
              class="absolute top-full left-0 mt-2 w-72 bg-white rounded-lg shadow-xl border border-gray-200 py-2 z-50"
            >
              <div class="px-3 py-2 text-xs text-gray-500 uppercase tracking-wide border-b border-gray-200">
                Switch Board
              </div>
              
              <!-- Board List -->
              <div class="max-h-64 overflow-y-auto">
                <button
                  v-for="b in projectBoards"
                  :key="b.id"
                  @click="switchBoard(b.id)"
                  class="w-full text-left px-4 py-3 hover:bg-gray-50 transition-colors flex items-center justify-between group"
                  :class="{ 'bg-blue-50': b.id === boardId }"
                >
                  <div class="flex items-center gap-3">
                    <ClipboardIcon className="w-4 h-4 text-gray-500" />
                    <div>
                      <div class="font-medium text-gray-800" :class="{ 'text-blue-700': b.id === boardId }">
                        {{ b.name }}
                      </div>
                      <div class="text-xs text-gray-500">
                        {{ b.tasks?.length || 0 }} tasks
                      </div>
                    </div>
                  </div>
                  <span v-if="b.id === boardId" class="text-blue-600 text-xs font-medium">
                    Current
                  </span>
                </button>
              </div>

              <!-- Add New Board -->
              <div class="border-t border-gray-200 mt-2">
                <button
                  @click="openCreateBoardModal"
                  class="w-full text-left px-4 py-3 hover:bg-gray-50 transition-colors flex items-center gap-3 text-blue-600 font-medium"
                >
                  <PlusIcon className="w-4 h-4" />
                  <span>Create New Board</span>
                </button>
              </div>
            </div>
          </div>

          <!-- Board Type Badge -->
          <span :class="getBoardTypeClass(board?.type || '')" class="text-xs px-3 py-1.5 rounded-full font-medium">
            {{ board?.type }}
          </span>
        </div>

        <!-- Board Actions -->
        <div class="flex items-center gap-2">
          <button
            @click="editCurrentBoard"
            class="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-all flex items-center gap-2"
          >
            <EditIcon className="w-4 h-4" />
            Edit Board
          </button>
          <button
            @click="handleDeleteCurrentBoard"
            class="px-4 py-2 bg-red-50 text-red-600 rounded-lg hover:bg-red-100 transition-all flex items-center gap-2"
          >
            <DeleteIcon className="w-4 h-4" />
            Delete Board
          </button>
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
      v-if="showTaskModal"
      :task="selectedTask"
      :boardId="boardId"
      :defaultStatus="defaultStatus"
      @close="closeTaskModal"
      @save="handleSaveTask"
    />

    <!-- Board Modal -->
    <Modal
      :show="showBoardModal"
      :title="isEditingBoard ? 'Edit Board' : 'Create Board'"
      :submitText="isEditingBoard ? 'Update' : 'Create'"
      @close="closeBoardModal"
      @submit="submitBoardForm"
    >
      <div class="space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Board Name *</label>
          <input
            v-model="boardForm.name"
            type="text"
            placeholder="e.g., Sprint 1, Backlog, Bug Tracker"
            class="input"
            required
          />
        </div>
        
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Board Type</label>
          <select v-model="boardForm.type" class="input">
            <option v-for="type in BOARD_TYPES" :key="type">{{ type }}</option>
          </select>
        </div>
      </div>
    </Modal>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted, watch, onBeforeUnmount } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { getBoard, getBoards, createBoard, updateBoard, deleteBoard, createTask, updateTask, deleteTask } from '@/services/api';
import { useConfirm } from '@/composables/useConfirm';
import type { TaskItem, Board, TaskCreate } from '@/types/Project';
import { STATUSES, BOARD_TYPES } from '@/types/Project';

import KanbanCard from './kanban/KanbanCard.vue';
import TaskModal from './task/TaskModal.vue';
import Modal from './common/Modal.vue';
import { 
  LoadingIcon, 
  PlusIcon,
  EditIcon,
  DeleteIcon,
  FolderIcon,
  ClipboardIcon
} from './icons';

export default defineComponent({
  name: 'BoardDetailView',
  components: {
    TaskModal,
    Modal,
    LoadingIcon,
    PlusIcon,
    EditIcon,
    DeleteIcon,
    FolderIcon,
    ClipboardIcon,
    KanbanCard
  },
  setup() {
    const route = useRoute();
    const router = useRouter();
    const projectId = ref(Number(route.params.projectId));
    const boardId = ref(Number(route.params.boardId));
    const { confirm } = useConfirm();

    const board = ref<Board>();
    const projectBoards = ref<Board[]>([]);
    const tasks = ref<TaskItem[]>([]);
    const loading = ref(false);
    
    // Task Modal
    const showTaskModal = ref(false);
    const selectedTask = ref<TaskItem | null>(null);
    const defaultStatus = ref<string>('To Do');
    const draggedTask = ref<TaskItem | null>(null);

    // Board Management
    const showBoardDropdown = ref(false);
    const showBoardModal = ref(false);
    const isEditingBoard = ref(false);
    const boardForm = ref<{ id?: number; name: string; type: string }>({
      name: "",
      type: "Kanban"
    });

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

    const fetchProjectBoards = async () => {
      try {
        const res = await getBoards(projectId.value);
        projectBoards.value = res.data;
      } catch (error) {
        console.error('Failed to fetch project boards:', error);
      }
    };

    const switchBoard = (newBoardId: number) => {
      showBoardDropdown.value = false;
      router.push(`/projects/${projectId.value}/boards/${newBoardId}`);
    };

    const getBoardTypeClass = (type: string) => {
      const classes: Record<string, string> = {
        'Kanban': 'bg-blue-100 text-blue-700',
        'Scrum': 'bg-green-100 text-green-700',
        'Backlog': 'bg-purple-100 text-purple-700'
      };
      return classes[type] || 'bg-gray-100 text-gray-700';
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

    // Task Management
    const onDragStart = (event: DragEvent, task: TaskItem) => {
      draggedTask.value = task;
      if (event.dataTransfer) {
        event.dataTransfer.effectAllowed = 'move';
      }
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

    const openCreateModalWithStatus = (status: string) => {
      selectedTask.value = null;
      defaultStatus.value = status;
      showTaskModal.value = true;
    };

    const editTask = (task: TaskItem) => {
      selectedTask.value = task;
      showTaskModal.value = true;
    };

    const closeTaskModal = () => {
      showTaskModal.value = false;
      selectedTask.value = null;
    };

    const handleSaveTask = async (taskData: TaskItem | TaskCreate) => {
      try {
        if ('id' in taskData && taskData.id) {
          await updateTask(boardId.value, taskData.id, taskData as TaskItem);
        } else {
          await createTask(boardId.value, taskData as TaskCreate);
        }
        closeTaskModal();
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

    // Board Management
    const openCreateBoardModal = () => {
      boardForm.value = { name: "", type: "Kanban" };
      isEditingBoard.value = false;
      showBoardModal.value = true;
      showBoardDropdown.value = false;
    };

    const editCurrentBoard = () => {
      if (!board.value) return;
      boardForm.value = {
        id: board.value.id,
        name: board.value.name,
        type: board.value.type
      };
      isEditingBoard.value = true;
      showBoardModal.value = true;
    };

    const closeBoardModal = () => {
      showBoardModal.value = false;
      isEditingBoard.value = false;
    };

    const submitBoardForm = async () => {
      if (!boardForm.value.name.trim()) {
        alert('Board name is required');
        return;
      }

      try {
        if (isEditingBoard.value && boardForm.value.id) {
          await updateBoard(projectId.value, boardForm.value.id, {
            name: boardForm.value.name,
            type: boardForm.value.type,
            projectId: projectId.value
          });
          await fetchBoard();
        } else {
          const response = await createBoard(projectId.value, {
            name: boardForm.value.name,
            type: boardForm.value.type,
            projectId: projectId.value
          });
          // Navigate to the newly created board
          router.push(`/projects/${projectId.value}/boards/${response.data.id}`);
        }
        await fetchProjectBoards();
        closeBoardModal();
      } catch (error) {
        console.error('Failed to save board:', error);
        alert('Failed to save board');
      }
    };

    const handleDeleteCurrentBoard = async () => {
      if (!board.value) return;
      
      if (confirm("Are you sure you want to delete this board? All tasks in this board will be deleted.")) {
        try {
          await deleteBoard(projectId.value, board.value.id);
          
          // Navigate to another board or summary
          await fetchProjectBoards();
          if (projectBoards.value.length > 0) {
            const nextBoard = projectBoards.value.find(b => b.id !== board.value!.id);
            if (nextBoard) {
              router.push(`/projects/${projectId.value}/boards/${nextBoard.id}`);
            } else {
              router.push('/');
            }
          } else {
            router.push('/');
          }
        } catch (error) {
          console.error('Failed to delete board:', error);
          alert('Failed to delete board');
        }
      }
    };

    // Close dropdown when clicking outside
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as HTMLElement;
      if (!target.closest('.relative')) {
        showBoardDropdown.value = false;
      }
    };

    onMounted(() => {
      fetchBoard();
      fetchProjectBoards();
      document.addEventListener('click', handleClickOutside);
    });

    onBeforeUnmount(() => {
      document.removeEventListener('click', handleClickOutside);
    });

    watch(
      () => route.params.boardId,
      (newBoardId) => {
        boardId.value = Number(newBoardId);
        projectId.value = Number(route.params.projectId);
        fetchBoard();
        fetchProjectBoards();
      }
    );

    return {
      projectId,
      boardId,
      board,
      projectBoards,
      tasks,
      loading,
      showBoardDropdown,
      showTaskModal,
      selectedTask,
      defaultStatus,
      showBoardModal,
      isEditingBoard,
      boardForm,
      STATUSES,
      BOARD_TYPES,
      getBoardTypeClass,
      getTasksByStatus,
      getStatusIconClass,
      switchBoard,
      onDragStart,
      onDrop,
      openCreateModalWithStatus,
      editTask,
      closeTaskModal,
      handleSaveTask,
      handleDelete,
      openCreateBoardModal,
      editCurrentBoard,
      closeBoardModal,
      submitBoardForm,
      handleDeleteCurrentBoard
    };
  }
});
</script>

<style scoped>
.input {
  @apply w-full px-3 py-2 mb-4 border border-gray-300 rounded;
}

.rotate-180 {
  transform: rotate(180deg);
  transition: transform 0.2s;
}
</style>