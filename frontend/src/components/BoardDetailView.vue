<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <div v-if="loading" class="flex items-center justify-center gap-2 mt-8">
      <LoadingIcon className="h-5 w-5 text-blue-600" />
      Loading board...
    </div>

    <!-- Empty State: No Boards -->
    <div v-else-if="!board && boardStore.boards.length === 0" class="flex items-center justify-center min-h-[60vh]">
      <div class="text-center max-w-md">
        <ClipboardIcon className="w-20 h-20 mx-auto mb-4 text-gray-300" />
        <h2 class="text-2xl font-bold text-gray-800 mb-2">No Boards Yet</h2>
        <p class="text-gray-600 mb-6">
          Get started by creating your first board for this project.
        </p>
        <button
          @click="openCreateBoardModal"
          class="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition-all shadow-sm hover:shadow-md flex items-center gap-2 mx-auto"
        >
          <PlusIcon className="w-5 h-5" />
          Create First Board
        </button>
      </div>
    </div>

    <!-- Dynamic Board View Based on Type -->
    <div v-else>
      <!-- Sprint Board (for Scrum boards) -->
      <SprintBoardView
        v-if="board?.type === 'Scrum'"
        :board="board"
        :boardId="boardId"
        :projectBoards="boardStore.boards"
        :workItems="workItems"
        :sprints="sprintStore.sprints"
        :selectedSprintId="selectedSprintId"
        @switch-board="switchBoard"
        @create-board="openCreateBoardModal"
        @edit-board="editCurrentBoard"
        @delete-board="handleDeleteCurrentBoard"
        @create-sprint="openCreateSprintModal"
        @select-sprint="handleSprintSelect"
        @start-sprint="handleStartSprint"
        @complete-sprint="handleCompleteSprint"
        @edit-sprint="editSprint"
        @delete-sprint="handleDeleteSprint"
        @create-workitem="openCreateModalWithStatus"
        @edit-workitem="editWorkItem"
        @delete-workitem="handleDelete"
        @update-status="handleUpdateWorkItemStatus"
        @return-to-backlog="handleReturnToBacklog"
      />

      <!-- Kanban Board (for Kanban and Backlog boards) -->
      <KanbanBoardView
        v-else
        :board="board"
        :boardId="boardId"
        :projectBoards="boardStore.boards"
        :workItems="workItems"
        @switch-board="switchBoard"
        @create-board="openCreateBoardModal"
        @edit-board="editCurrentBoard"
        @delete-board="handleDeleteCurrentBoard"
        @create-workitem="openCreateModalWithStatus"
        @edit-workitem="editWorkItem"
        @delete-workitem="handleDelete"
        @update-status="handleUpdateWorkItemStatus"
        @return-to-backlog="handleReturnToBacklog"
      />
    </div>

    <!-- Modals stay in BoardDetailView -->
    <WorkItemModal
      v-if="showWorkItemModal"
      :workItem="selectedWorkItem"
      :boardId="boardId"
      :defaultStatus="defaultStatus"
      :sprintId="selectedSprintId"
      :availableStatuses="availableStatuses"
      @close="closeWorkItemModal"
      @save="handleSaveWorkItem"
    />

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
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
            required
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Board Type</label>
          <select v-model="boardForm.type" class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none">
            <option v-for="type in BOARD_TYPES" :key="type">{{ type }}</option>
          </select>
        </div>

        <!-- Column Editor -->
        <div v-if="showBoardModal" class="border-t border-gray-200 pt-4">
          <ColumnEditor
            :key="isEditingBoard ? 'edit' : 'create'"
            :columns="boardForm.columns || []"
            @update:columns="boardForm.columns = $event"
          />
        </div>
      </div>
    </Modal>

    <SprintModal
      v-if="showSprintModal"
      :sprint="selectedSprint"
      @close="closeSprintModal"
      @save="handleSaveSprint"
    />
  </div>
</template>


<script lang="ts">
import { defineComponent, ref, onMounted, watch, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { getBoard, createWorkItem, updateWorkItem, deleteWorkItem, returnWorkItemToBacklog } from '@/services/api';
import { useConfirm } from '@/composables/useConfirm';
import { useSprintStore } from '@/stores/sprint';
import { useBoardStore } from '@/stores/boards';
import type { Board, BoardColumn } from '@/types/Project';
import type { WorkItem, WorkItemCreate } from '@/types/WorkItem';
import type { Sprint, CreateSprintDto, UpdateSprintDto } from '@/types/Sprint';
import { BOARD_TYPES, getStatusesFromBoard } from '@/types/Project';
import WorkItemModal from './workItem/WorkItemModal.vue';
import Modal from './common/Modal.vue';
import SprintModal from './sprint/SprintModal.vue';
import SprintBoardView from './sprint/SprintBoardView.vue';
import KanbanBoardView from './kanban/KanbanBoard.vue';
import ColumnEditor from './board/ColumnEditor.vue';
import {
  LoadingIcon,
  PlusIcon,
  ClipboardIcon
} from './icons';

export default defineComponent({
  name: 'BoardDetailView',
  components: {
    WorkItemModal,
    Modal,
    SprintModal,
    LoadingIcon,
    PlusIcon,
    ClipboardIcon,
    SprintBoardView,
    KanbanBoardView,
    ColumnEditor
  },
  setup() {
    const route = useRoute();
    const router = useRouter();
    const projectId = ref(Number(route.params.projectId));
    const boardId = ref(Number(route.params.boardId));
    const { confirm } = useConfirm();
    const sprintStore = useSprintStore();
    const boardStore = useBoardStore();

    const board = ref<Board | null>(null);
    const workItems = ref<WorkItem[]>([]);
    const loading = ref(false);
    
    // WorkItem Modal
    const showWorkItemModal = ref(false);
    const selectedWorkItem = ref<WorkItem | null>(null);
    const defaultStatus = ref<string>('To Do');

    // Board Management
    const showBoardModal = ref(false);
    const isEditingBoard = ref(false);
    const boardForm = ref<{ id?: number; name: string; type: string; columns?: BoardColumn[] }>({
      name: "",
      type: "Kanban",
      columns: undefined
    });

    // Sprint Management
    const showSprintModal = ref(false);
    const selectedSprintId = ref<number | null>(null);
    
    const selectedSprint = computed(() => {
      if (selectedSprintId.value === null) return undefined;
      return sprintStore.sprints.find(s => s.id === selectedSprintId.value);
    });

    const filteredWorkItems = computed(() => {
      if (selectedSprintId.value === null) {
        // Show backlog items (items without sprint)
        return workItems.value.filter(item => !item.sprintId);
      }
      // Show items in selected sprint
      return workItems.value.filter(item => item.sprintId === selectedSprintId.value);
    });

    const availableStatuses = computed(() => {
      return getStatusesFromBoard(board.value);
    });

    const fetchBoard = async () => {
      loading.value = true;
      try {
        const res = await getBoard(projectId.value, boardId.value);
        board.value = res.data;
        workItems.value = res.data.workItems || [];
      } catch (error) {
        console.error('Failed to fetch board:', error);
        board.value = null;
        workItems.value = [];
      } finally {
        loading.value = false;
      }
    };

    const fetchSprints = async () => {
      if (!boardId.value) return;
      try {
        await sprintStore.fetchSprints(boardId.value);
      } catch (error) {
        console.error('Failed to fetch sprints:', error);
      }
    };

    const switchBoard = (newBoardId: number) => {
      router.push(`/projects/${projectId.value}/boards/${newBoardId}`);
    };

    // Sprint Management Functions
    const handleSprintSelect = (sprintId: number | null) => {
      selectedSprintId.value = sprintId;
    };

    const openCreateSprintModal = () => {
      selectedSprintId.value = null;
      showSprintModal.value = true;
    };

    const editSprint = (sprint: Sprint) => {
      selectedSprintId.value = sprint.id;
      showSprintModal.value = true;
    };

    const closeSprintModal = () => {
      showSprintModal.value = false;
    };

    const handleSaveSprint = async (sprintData: CreateSprintDto | UpdateSprintDto) => {
      try {
        if (selectedSprint.value) {
          await sprintStore.updateSprint(selectedSprint.value.id, sprintData as UpdateSprintDto);
        } else {
          const newSprint = await sprintStore.createSprint(boardId.value, sprintData as CreateSprintDto);
          selectedSprintId.value = newSprint.id;
        }
        closeSprintModal();
        await fetchSprints();
      } catch (error) {
        console.error('Failed to save sprint:', error);
        alert('Failed to save sprint');
      }
    };

    const handleStartSprint = async (sprintId: number) => {
      try {
        await sprintStore.startSprint(sprintId);
        await fetchSprints();
      } catch (error) {
        console.error('Failed to start sprint:', error);
        alert('Failed to start sprint. Make sure no other sprint is active.');
      }
    };

    const handleCompleteSprint = async (sprintId: number) => {
      if (confirm('Complete this sprint? Incomplete items will be moved to the backlog.')) {
        try {
          const result = await sprintStore.completeSprint(sprintId);
          await fetchSprints();
          await fetchBoard();
          alert(`Sprint completed! ${result.movedToBacklog} items moved to backlog.`);
        } catch (error) {
          console.error('Failed to complete sprint:', error);
          alert('Failed to complete sprint');
        }
      }
    };

    const handleDeleteSprint = async (sprintId: number) => {
      if (confirm('Delete this sprint? All items will be moved to the backlog.')) {
        try {
          await sprintStore.deleteSprint(sprintId);
          selectedSprintId.value = null;
          await fetchSprints();
          await fetchBoard();
        } catch (error) {
          console.error('Failed to delete sprint:', error);
          alert('Failed to delete sprint');
        }
      }
    };

    const openCreateModalWithStatus = (status: string) => {
      selectedWorkItem.value = null;
      defaultStatus.value = status;
      showWorkItemModal.value = true;
    };

    const editWorkItem = (workItem: WorkItem) => {
      selectedWorkItem.value = workItem;
      showWorkItemModal.value = true;
    };

    const closeWorkItemModal = () => {
      showWorkItemModal.value = false;
      selectedWorkItem.value = null;
    };

    const handleSaveWorkItem = async (workItemData: WorkItem | WorkItemCreate) => {
      try {
        if ('id' in workItemData && workItemData.id) {
          await updateWorkItem(boardId.value, workItemData.id, workItemData as WorkItem);
        } else {
          await createWorkItem(boardId.value, workItemData as WorkItemCreate);
        }
        closeWorkItemModal();
        await fetchBoard();
        await fetchSprints();
      } catch (error) {
        console.error('Failed to save WorkItem:', error);
      }
    };

    const handleDelete = async (id: number) => {
      if (confirm('Are you sure you want to delete this WorkItem?')) {
        try {
          await deleteWorkItem(boardId.value, id);
          await fetchBoard();
          await fetchSprints();
        } catch (error) {
          console.error('Failed to delete WorkItem:', error);
        }
      }
    };

    // Board Management - Using Store
    const openCreateBoardModal = () => {
      boardForm.value = { name: "", type: "Kanban", columns: undefined };
      isEditingBoard.value = false;
      showBoardModal.value = true;
    };

    const editCurrentBoard = () => {
      if (!board.value) return;
      boardForm.value = {
        id: board.value.id,
        name: board.value.name,
        type: board.value.type,
        columns: board.value.columns ? [...board.value.columns] : []
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
          // Update existing board via store
          await boardStore.updateBoard(projectId.value, boardForm.value.id, {
            name: boardForm.value.name,
            type: boardForm.value.type,
            projectId: projectId.value,
            columns: boardForm.value.columns
          });
          await fetchBoard();
        } else {
          // Create new board via store (this updates sidebar automatically)
          const newBoard = await boardStore.createBoard(projectId.value, {
            name: boardForm.value.name,
            type: boardForm.value.type,
            projectId: projectId.value,
            columns: boardForm.value.columns
          });
          // Navigate to the newly created board
          router.push(`/projects/${projectId.value}/boards/${newBoard.id}`);
        }
        closeBoardModal();
      } catch (error) {
        console.error('Failed to save board:', error);
        alert('Failed to save board');
      }
    };

    const handleDeleteCurrentBoard = async () => {
      if (!board.value) return;
      
      if (confirm("Are you sure you want to delete this board? All WorkItems in this board will be deleted.")) {
        try {
          // Delete via store (this updates sidebar automatically)
          await boardStore.deleteBoard(projectId.value, board.value.id);
          
          // Navigate to another board or show empty state
          if (boardStore.boards.length > 0) {
            const nextBoard = boardStore.boards[0];
            router.push(`/projects/${projectId.value}/boards/${nextBoard!.id}`);
          } else {
            // No boards left, clear the board
            board.value = null;
            workItems.value = [];
          }
        } catch (error) {
          console.error('Failed to delete board:', error);
          alert('Failed to delete board');
        }
      }
    };

    const handleUpdateWorkItemStatus = async (workItem: WorkItem, newStatus: string) => {
      try {
        await updateWorkItem(boardId.value, workItem.id, {
          ...workItem,
          status: newStatus
        });
        await fetchBoard();
      } catch (error) {
        console.error('Failed to update WorkItem status:', error);
      }
    };

    const handleReturnToBacklog = async (workItem: WorkItem) => {
      if (confirm('Return this item to the backlog? It will be removed from this board.')) {
        try {
          await returnWorkItemToBacklog(workItem.id)
          await fetchBoard() // Refresh board - item will disappear
        } catch (error) {
          console.error('Failed to return to backlog:', error)
          alert('Failed to return item to backlog')
        }
      }
    }

    onMounted(async () => {
      // Fetch boards for sidebar sync
      await boardStore.fetchBoards(projectId.value);
      await fetchBoard();
      await fetchSprints();
    });

    watch(
      () => route.params.boardId,
      (newBoardId) => {
        if (newBoardId) {
          boardId.value = Number(newBoardId);
          projectId.value = Number(route.params.projectId);
          fetchBoard();
          fetchSprints();
          selectedSprintId.value = null;
        }
      }
    );

    watch(
      () => route.params.projectId,
      async (newProjectId) => {
        if (newProjectId) {
          projectId.value = Number(newProjectId);
          await boardStore.fetchBoards(projectId.value);
        }
      }
    );

    return {
      projectId,
      boardId,
      board,
      workItems,
      loading,
      showWorkItemModal,
      selectedWorkItem,
      defaultStatus,
      showBoardModal,
      isEditingBoard,
      boardForm,
      sprintStore,
      boardStore,
      showSprintModal,
      selectedSprintId,
      selectedSprint,
      BOARD_TYPES,
      filteredWorkItems,
      availableStatuses,
      switchBoard,
      handleSprintSelect,
      openCreateSprintModal,
      editSprint,
      closeSprintModal,
      handleSaveSprint,
      handleStartSprint,
      handleCompleteSprint,
      handleDeleteSprint,
      openCreateModalWithStatus,
      editWorkItem,
      closeWorkItemModal,
      handleSaveWorkItem,
      handleDelete,
      openCreateBoardModal,
      editCurrentBoard,
      closeBoardModal,
      submitBoardForm,
      handleDeleteCurrentBoard,
      handleUpdateWorkItemStatus,
      handleReturnToBacklog
    };
  }
});
</script>

<style scoped>
.rotate-180 {
  transform: rotate(180deg);
}
</style>