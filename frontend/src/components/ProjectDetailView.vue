<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <div class="mb-6">
      <button class="btn btn-light flex items-center gap-2" @click="$router.push('/')">
        <ArrowLeftIcon />
        Back to Projects
      </button>
    </div>

    <div v-if="loading" class="flex items-center justify-center gap-2 mt-8">
      <LoadingIcon className="h-5 w-5 text-blue-600" />
      Loading project...
    </div>

    <div v-else-if="project">
      <div class="flex justify-between items-center mb-6">
        <div>
          <h1 class="text-3xl font-bold text-gray-800">{{ project.name }}</h1>
          <p v-if="project.description" class="text-gray-600 mt-1">{{ project.description }}</p>
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
      <div v-if="project.boards && project.boards.length > 0" class="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="board in project.boards"
          :key="board.id"
          class="bg-white shadow-sm hover:shadow-lg rounded-2xl p-6 border border-gray-100 transition-all duration-200 cursor-pointer"
          @click="navigateToBoard(board.id)"
        >
          <div class="flex justify-between items-start mb-4">
            <h2 class="text-xl font-semibold text-gray-800">{{ board.name }}</h2>
            <div class="flex items-center gap-2">
              <span class="text-sm text-gray-500 bg-purple-50 px-3 py-1 rounded-full">
                {{ board.tasks?.length ?? 0 }} tasks
              </span>
              <DropdownMenu @click.stop>
                <button
                  @click="editBoard(board)"
                  class="w-full text-left px-4 py-2 hover:bg-gray-100 flex items-center gap-2 text-sm text-gray-700"
                >
                  <EditIcon className="w-4 h-4" />
                  Edit
                </button>
                <button
                  @click="deleteBoard(board.id)"
                  class="w-full text-left px-4 py-2 hover:bg-gray-100 flex items-center gap-2 text-sm text-red-600"
                >
                  <DeleteIcon className="w-4 h-4" />
                  Delete
                </button>
              </DropdownMenu>
            </div>
          </div>

          <p v-if="board.description" class="text-gray-600 text-sm mb-4">
            {{ board.description }}
          </p>

          <div class="flex items-center gap-2 text-sm text-gray-500">
            <span class="px-2 py-1 bg-gray-100 rounded">{{ getBoardTypeLabel(board.type) }}</span>
            <span>•</span>
            <span>{{ formatDate(board.createdAt) }}</span>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <EmptyState
        v-else
        :icon="FolderIcon"
        message="No boards yet. Create your first one!"
      />
    </div>

    <!-- Board Modal -->
    <Modal
      :show="modal.isOpen.value"
      :title="modal.isEditing.value ? 'Edit Board' : 'Create Board'"
      :submitText="modal.isEditing.value ? 'Update' : 'Create'"
      @close="modal.close"
      @submit="submitBoardForm"
    >
      <input
        v-model="boardForm.name"
        type="text"
        placeholder="Board name"
        class="input"
      />
      <textarea
        v-model="boardForm.description"
        placeholder="Board description (optional)"
        class="input min-h-[100px]"
      />
      <select v-model="boardForm.type" class="input">
        <option :value="0">Kanban</option>
        <option :value="1">Scrum</option>
        <option :value="2">Backlog</option>
      </select>
    </Modal>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { getProject, createBoard as apiCreateBoard, updateBoard as apiUpdateBoard, deleteBoard as apiDeleteBoard } from '@/services/api';
import { useModal } from '@/composables/useModal';
import { useConfirm } from '@/composables/useConfirm';
import type { Project, Board, BoardCreate } from '@/types/Project';

import Modal from '@/components/common/Modal.vue';
import EmptyState from '@/components/common/EmptyState.vue';
import DropdownMenu from '@/components/common/DropdownMenu.vue';
import { ArrowLeftIcon, LoadingIcon, PlusIcon, EditIcon, DeleteIcon, FolderIcon } from '@/components/icons';

export default defineComponent({
  name: 'ProjectDetailView',
  components: {
    Modal,
    EmptyState,
    DropdownMenu,
    ArrowLeftIcon,
    LoadingIcon,
    PlusIcon,
    EditIcon,
    DeleteIcon,
    FolderIcon
  },
  setup() {
    const route = useRoute();
    const router = useRouter();
    const { confirm } = useConfirm();
    const modal = useModal<Board>();

    const projectId = ref(Number(route.params.id));
    const project = ref<Project>();
    const loading = ref(false);

    const boardForm = ref<BoardCreate & { id?: number }>({
      name: '',
      description: '',
      type: 0
    });

    const fetchProject = async () => {
      loading.value = true;
      try {
        const res = await getProject(projectId.value);
        project.value = res.data;
      } catch (error) {
        console.error('Failed to fetch project:', error);
      } finally {
        loading.value = false;
      }
    };

    const openCreateBoardModal = () => {
      boardForm.value = { name: '', description: '', type: 0 };
      modal.open();
    };

    const editBoard = (board: Board) => {
      boardForm.value = {
        id: board.id,
        name: board.name,
        description: board.description,
        type: board.type
      };
      modal.open(board);
    };

    const submitBoardForm = async () => {
      try {
        if (modal.isEditing.value && boardForm.value.id) {
          await apiUpdateBoard(projectId.value, boardForm.value.id, {
            name: boardForm.value.name,
            description: boardForm.value.description,
            type: boardForm.value.type
          });
        } else {
          await apiCreateBoard(projectId.value, {
            name: boardForm.value.name,
            description: boardForm.value.description,
            type: boardForm.value.type
          });
        }
        modal.close();
        await fetchProject();
      } catch (error) {
        console.error('Failed to submit board:', error);
      }
    };

    const deleteBoard = async (boardId: number) => {
      if (confirm('Are you sure you want to delete this board? All tasks will be deleted.')) {
        try {
          await apiDeleteBoard(projectId.value, boardId);
          await fetchProject();
        } catch (error) {
          console.error('Failed to delete board:', error);
        }
      }
    };

    const navigateToBoard = (boardId: number) => {
      router.push(`/projects/${projectId.value}/boards/${boardId}`);
    };

    const getBoardTypeLabel = (type: number) => {
      const types = ['Kanban', 'Scrum', 'Backlog'];
      return types[type] || 'Kanban';
    };

    const formatDate = (dateString: string) => {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    };

    onMounted(fetchProject);

    return {
      project,
      loading,
      modal,
      boardForm,
      FolderIcon,
      openCreateBoardModal,
      editBoard,
      submitBoardForm,
      deleteBoard,
      navigateToBoard,
      getBoardTypeLabel,
      formatDate
    };
  }
});
</script>