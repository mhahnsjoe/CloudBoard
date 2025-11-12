<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <button class="btn btn-light flex items-center gap-2 mb-6" @click="$router.push('/')">
      <ArrowLeftIcon />
      Back to Dashboard
    </button>

    <div v-if="loading" class="loading flex items-center justify-center gap-2 mt-8">
      <LoadingIcon className="h-5 w-5 text-blue-600" />
      Loading project...
    </div>

    <div v-else>
      <div class="flex justify-between items-start mb-8">
        <div>
          <h1 class="text-3xl font-bold text-gray-800 mb-2">{{ project?.name }}</h1>
          <p v-if="project?.description" class="text-gray-600">{{ project.description }}</p>
        </div>
        <button
          @click="openCreateBoardModal"
          class="bg-blue-600 text-white px-5 py-2.5 rounded-lg hover:bg-blue-700 transition-all shadow-sm hover:shadow-md flex items-center gap-2"
        >
          <PlusIcon />
          New Board
        </button>
      </div>

      <!-- Boards Grid -->
      <div v-if="boards.length" class="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="board in boards"
          :key="board.id"
          @click="navigateToBoard(board.id)"
          class="bg-white shadow-sm hover:shadow-lg rounded-2xl p-6 border border-gray-100 transition-all duration-200 cursor-pointer group"
        >
          <div class="flex justify-between items-start mb-4">
            <div class="flex-1">
              <h2 class="text-xl font-semibold text-gray-800 group-hover:text-blue-600 transition mb-2">
                {{ board.name }}
              </h2>
              <span :class="getBoardTypeClass(board.type)" class="text-xs px-2 py-1 rounded-full">
                {{ board.type }}
              </span>
            </div>
            <div class="relative">
              <button
                @click.stop="toggleBoardMenu(board.id)"
                class="p-2 hover:bg-gray-100 rounded-lg transition"
              >
                <svg class="w-5 h-5 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"/>
                </svg>
              </button>
              <!-- Dropdown Menu -->
              <div
                v-if="activeBoardMenu === board.id"
                class="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-10"
              >
                <button
                  @click.stop="editBoard(board)"
                  class="w-full text-left px-4 py-2 hover:bg-gray-50 flex items-center gap-2 text-gray-700"
                >
                  <EditIcon className="w-4 h-4" />
                  Edit Board
                </button>
                <button
                  @click.stop="handleDeleteBoard(board.id)"
                  class="w-full text-left px-4 py-2 hover:bg-gray-50 flex items-center gap-2 text-red-600"
                >
                  <DeleteIcon className="w-4 h-4" />
                  Delete Board
                </button>
              </div>
            </div>
          </div>

          <div class="flex items-center justify-between text-sm text-gray-600 mt-4">
            <span class="flex items-center gap-1">
              <ClipboardIcon className="w-4 h-4" />
              {{ board.tasks?.length || 0 }} tasks
            </span>
            <span class="text-blue-600 group-hover:text-blue-700 font-medium">
              View →
            </span>
          </div>
        </div>
      </div>

      <EmptyState
        v-else
        :icon="ClipboardIcon"
        message="No boards yet. Create your first board!"
      />
    </div>

    <!-- Board Modal -->
    <Modal
      :show="modal.isOpen.value"
      :title="modal.isEditing.value ? 'Edit Board' : 'Create Board'"
      :submitText="modal.isEditing.value ? 'Update' : 'Create'"
      @close="closeModal"
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
import { defineComponent, ref, onMounted, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { getProject, createBoard, updateBoard, deleteBoard } from "@/services/api";
import { useModal } from "@/composables/useModal";
import { useConfirm } from "@/composables/useConfirm";
import type { Project, Board } from "@/types/Project";
import { BOARD_TYPES } from "@/types/Project";

import Modal from "@/components/common/Modal.vue";
import EmptyState from "@/components/common/EmptyState.vue";
import { ArrowLeftIcon, LoadingIcon, ClipboardIcon, PlusIcon, EditIcon, DeleteIcon } from "@/components/icons";

export default defineComponent({
  name: "ProjectDetail",
  components: {
    Modal,
    EmptyState,
    ArrowLeftIcon,
    LoadingIcon,
    ClipboardIcon,
    PlusIcon,
    EditIcon,
    DeleteIcon
  },
  setup() {
    const route = useRoute();
    const router = useRouter();
    const projectId = ref(Number(route.params.id));
    const project = ref<Project>();
    const boards = ref<Board[]>([]);
    const loading = ref(false);
    const modal = useModal<Board>();
    const { confirm } = useConfirm();
    const activeBoardMenu = ref<number | null>(null);

    const boardForm = ref<{ id?: number; name: string; type: string }>({
      name: "",
      type: "Kanban"
    });

    const fetchProject = async () => {
      loading.value = true;
      try {
        const response = await getProject(projectId.value);
        project.value = response.data;
        boards.value = response.data.boards || [];
      } catch (error) {
        console.error('Failed to fetch project:', error);
      } finally {
        loading.value = false;
      }
    };

    const openCreateBoardModal = () => {
      boardForm.value = { name: "", type: "Kanban" };
      modal.open();
    };

    const editBoard = (board: Board) => {
      boardForm.value = {
        id: board.id,
        name: board.name,
        type: board.type
      };
      modal.open(board);
      activeBoardMenu.value = null;
    };

    const closeModal = () => {
      modal.close();
      activeBoardMenu.value = null;
    };

    const submitBoardForm = async () => {
      if (!boardForm.value.name.trim()) {
        alert('Board name is required');
        return;
      }

      try {
        if (modal.isEditing.value && boardForm.value.id) {
          await updateBoard(projectId.value, boardForm.value.id, {
            name: boardForm.value.name,
            type: boardForm.value.type,
            projectId: projectId.value
          });
        } else {
          await createBoard(projectId.value,{
            name: boardForm.value.name,
            type: boardForm.value.type,
            projectId: projectId.value
          });
        }
        await fetchProject();
        closeModal();
      } catch (error) {
        console.error('Failed to save board:', error);
        alert('Failed to save board');
      }
    };

    const handleDeleteBoard = async (id: number) => {
      activeBoardMenu.value = null;
      if (confirm("Are you sure you want to delete this board? All tasks in this board will be deleted.")) {
        try {
          await deleteBoard(id);
          await fetchProject();
        } catch (error) {
          console.error('Failed to delete board:', error);
          alert('Failed to delete board');
        }
      }
    };

    const navigateToBoard = (boardId: number) => {
      router.push(`/projects/${projectId.value}/boards/${boardId}`);
    };

    const toggleBoardMenu = (boardId: number) => {
      activeBoardMenu.value = activeBoardMenu.value === boardId ? null : boardId;
    };

    const getBoardTypeClass = (type: string) => {
      const classes: Record<string, string> = {
        'Kanban': 'bg-blue-100 text-blue-700',
        'Scrum': 'bg-green-100 text-green-700',
        'Backlog': 'bg-purple-100 text-purple-700'
      };
      return classes[type] || 'bg-gray-100 text-gray-700';
    };

    // Close dropdown when clicking outside
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as HTMLElement;
      if (!target.closest('.relative')) {
        activeBoardMenu.value = null;
      }
    };

    onMounted(() => {
      fetchProject();
      document.addEventListener('click', handleClickOutside);
    });

    watch(
      () => route.params.id,
      (newId) => {
        projectId.value = Number(newId);
        fetchProject();
      }
    );

    return {
      project,
      boards,
      loading,
      modal,
      boardForm,
      activeBoardMenu,
      BOARD_TYPES,
      openCreateBoardModal,
      editBoard,
      closeModal,
      submitBoardForm,
      handleDeleteBoard,
      navigateToBoard,
      toggleBoardMenu,
      getBoardTypeClass,
      ClipboardIcon
    };
  }
});
</script>

<style scoped>
.btn {
  @apply border-none rounded px-4 py-2 cursor-pointer text-sm transition-colors duration-200;
}

.btn-light {
  @apply bg-gray-200 hover:bg-gray-300;
}

.input {
  @apply w-full px-3 py-2 mb-4 border border-gray-300 rounded;
}
</style>