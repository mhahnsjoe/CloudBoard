<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <div v-if="loading" class="flex items-center justify-center gap-2 mt-8">
      <LoadingIcon className="h-5 w-5 text-blue-600" />
      Loading board...
    </div>

    <!-- Empty State: No Boards -->
    <div v-else-if="!board && projectBoards.length === 0" class="flex items-center justify-center min-h-[60vh]">
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

    <div v-else>
      <!-- Board Header -->
      <BoardHeader
        :board="board"
        :boardId="boardId"
        :projectBoards="projectBoards"
        @switch-board="switchBoard"
        @create-board="openCreateBoardModal"
        @edit-board="editCurrentBoard"
        @delete-board="handleDeleteCurrentBoard"
        @create-sprint="openCreateSprintModal"
      >
        <template #sprint-selector>
          <SprintSelector
            :sprints="sprintStore.sprints"
            :selectedSprintId="selectedSprintId"
            @select="handleSprintSelect"
          />
        </template>
      </BoardHeader>

      <!-- Sprint Info Bar -->
      <SprintInfoBar
        :sprint="selectedSprint"
        @start-sprint="handleStartSprint"
        @complete-sprint="handleCompleteSprint"
        @edit-sprint="editSprint"
        @delete-sprint="handleDeleteSprint"
      />

      <!-- Kanban Board -->
      <KanbanBoard
        :workItems="filteredWorkItems"
        @create-workitem="openCreateModalWithStatus"
        @edit-workitem="editWorkItem"
        @delete-workitem="handleDelete"
        @update-status="handleUpdateWorkItemStatus"
      />
    </div>

    <WorkItemModal
      v-if="showWorkItemModal"
      :workItem="selectedWorkItem"
      :boardId="boardId"
      :defaultStatus="defaultStatus"
      :sprintId="selectedSprintId"
      @close="closeWorkItemModal"
      @save="handleSaveWorkItem"
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

    <!-- Sprint Modal -->
    <SprintModal
      v-if="showSprintModal"
      :sprint="selectedSprint"
      @close="closeSprintModal"
      @save="handleSaveSprint"
    />
  </div>
</template>


<script lang="ts">
import { defineComponent, ref, onMounted, watch, onBeforeUnmount, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { getBoard, getBoards, createBoard, updateBoard, deleteBoard, createWorkItem, updateWorkItem, deleteWorkItem } from '@/services/api';
import { useConfirm } from '@/composables/useConfirm';
import { useSprintStore } from '@/stores/sprint';
import type { Board } from '@/types/Project';
import type { WorkItem, WorkItemCreate } from '@/types/WorkItem';
import type { Sprint, CreateSprintDto, UpdateSprintDto } from '@/types/Sprint';
import { STATUSES, BOARD_TYPES } from '@/types/Project';

import BoardHeader from './board/BoardHeader.vue';
import SprintInfoBar from './sprint/SprintInfoBar.vue';
import KanbanBoard from './kanban/KanbanBoard.vue';
import WorkItemModal from './workItem/WorkItemModal.vue';
import Modal from './common/Modal.vue';
import SprintSelector from './sprint/SprintSelector.vue';
import SprintModal from './sprint/SprintModal.vue';
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
    SprintSelector,
    SprintModal,
    LoadingIcon,
    PlusIcon,
    ClipboardIcon,
    BoardHeader,
    SprintInfoBar,
    KanbanBoard
  },
  setup() {
    const route = useRoute();
    const router = useRouter();
    const projectId = ref(Number(route.params.projectId));
    const boardId = ref(Number(route.params.boardId));
    const { confirm } = useConfirm();
    const sprintStore = useSprintStore();

    const board = ref<Board | null>(null);
    const projectBoards = ref<Board[]>([]);
    const workItems = ref<WorkItem[]>([]);
    const loading = ref(false);
    
    // WorkItem Modal
    const showWorkItemModal = ref(false);
    const selectedWorkItem = ref<WorkItem | null>(null);
    const defaultStatus = ref<string>('To Do');
    const draggedWorkItem = ref<WorkItem | null>(null);

    // Board Management
    const showBoardDropdown = ref(false);
    const showBoardModal = ref(false);
    const isEditingBoard = ref(false);
    const boardForm = ref<{ id?: number; name: string; type: string }>({
      name: "",
      type: "Kanban"
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

    const fetchProjectBoards = async () => {
      try {
        const res = await getBoards(projectId.value);
        projectBoards.value = res.data;
      } catch (error) {
        console.error('Failed to fetch project boards:', error);
        projectBoards.value = [];
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

    const getWorkItemsByStatus = (status: string) => {
      return filteredWorkItems.value.filter(workItem => workItem.status === status);
    };

    const getStatusIconClass = (status: string) => {
      const classes: Record<string, string> = {
        'To Do': 'text-gray-500',
        'In Progress': 'text-blue-500',
        'Done': 'text-green-500'
      };
      return classes[status] || 'text-gray-500';
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
      // Don't clear selectedSprintId here as it's used for viewing
    };

    const handleSaveSprint = async (sprintData: CreateSprintDto | UpdateSprintDto) => {
    try {
      if (selectedSprint.value) {
        // Editing existing sprint
        await sprintStore.updateSprint(selectedSprint.value.id, sprintData as UpdateSprintDto);
      } else {
        // Creating new sprint
        const newSprint = await sprintStore.createSprint(boardId.value, sprintData as CreateSprintDto);
        selectedSprintId.value = newSprint.id;  // <-- This line causes the issue
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
          await fetchBoard(); // Refresh work items
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
          await fetchBoard(); // Refresh work items
        } catch (error) {
          console.error('Failed to delete sprint:', error);
          alert('Failed to delete sprint');
        }
      }
    };

    const formatDateRange = (sprint: Sprint) => {
      const start = new Date(sprint.startDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
      const end = new Date(sprint.endDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
      return `${start} - ${end}`;
    };

    // WorkItem Management
    const onDragStart = (event: DragEvent, workItem: WorkItem) => {
      draggedWorkItem.value = workItem;
      if (event.dataTransfer) {
        event.dataTransfer.effectAllowed = 'move';
      }
    };

    const onDrop = async (event: DragEvent, newStatus: string) => {
      event.preventDefault();
      if (draggedWorkItem.value && draggedWorkItem.value.status !== newStatus) {
        try {
          await updateWorkItem(boardId.value, draggedWorkItem.value.id, {
            ...draggedWorkItem.value,
            status: newStatus
          });
          await fetchBoard();
        } catch (error) {
          console.error('Failed to update WorkItem status:', error);
        }
      }
      draggedWorkItem.value = null;
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
        await fetchSprints(); // Refresh sprint stats
      } catch (error) {
        console.error('Failed to save WorkItem:', error);
      }
    };

    const handleDelete = async (id: number) => {
      if (confirm('Are you sure you want to delete this WorkItem?')) {
        try {
          await deleteWorkItem(boardId.value, id);
          await fetchBoard();
          await fetchSprints(); // Refresh sprint stats
        } catch (error) {
          console.error('Failed to delete WorkItem:', error);
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
          await fetchProjectBoards();
        } else {
          const response = await createBoard(projectId.value, {
            name: boardForm.value.name,
            type: boardForm.value.type,
            projectId: projectId.value
          });
          // Navigate to the newly created board
          router.push(`/projects/${projectId.value}/boards/${response.data.id}`);
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
          await deleteBoard(projectId.value, board.value.id);
          
          // Refresh the boards list
          await fetchProjectBoards();
          
          // Navigate to another board or stay if no boards
          if (projectBoards.value.length > 0) {
            const nextBoard = projectBoards.value.find(b => b.id !== board.value!.id);
            if (nextBoard) {
              router.push(`/projects/${projectId.value}/boards/${nextBoard.id}`);
            } else {
              // This was the last board, stay on the page to show empty state
              board.value = null;
              workItems.value = [];
            }
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

    // Close dropdown when clicking outside
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as HTMLElement;
      if (!target.closest('.relative')) {
        showBoardDropdown.value = false;
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

    onMounted(() => {
      fetchProjectBoards();
      fetchBoard();
      fetchSprints();
      document.addEventListener('click', handleClickOutside);
    });

    onBeforeUnmount(() => {
      document.removeEventListener('click', handleClickOutside);
    });

    watch(
      () => route.params.boardId,
      (newBoardId) => {
        if (newBoardId) {
          boardId.value = Number(newBoardId);
          projectId.value = Number(route.params.projectId);
          fetchBoard();
          fetchSprints();
          selectedSprintId.value = null; // Reset sprint selection
        }
      }
    );

    watch(
      () => route.params.projectId,
      (newProjectId) => {
        if (newProjectId) {
          projectId.value = Number(newProjectId);
          fetchProjectBoards();
        }
      }
    );

    return {
      projectId,
      boardId,
      board,
      projectBoards,
      workItems,
      loading,
      showBoardDropdown,
      showWorkItemModal,
      selectedWorkItem,
      defaultStatus,
      showBoardModal,
      isEditingBoard,
      boardForm,
      sprintStore,
      showSprintModal,
      selectedSprintId,
      selectedSprint,
      STATUSES,
      BOARD_TYPES,
      filteredWorkItems,
      getBoardTypeClass,
      getWorkItemsByStatus,
      getStatusIconClass,
      switchBoard,
      handleSprintSelect,
      openCreateSprintModal,
      editSprint,
      closeSprintModal,
      handleSaveSprint,
      handleStartSprint,
      handleCompleteSprint,
      handleDeleteSprint,
      formatDateRange,
      onDragStart,
      onDrop,
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
      handleUpdateWorkItemStatus
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
}
</style>