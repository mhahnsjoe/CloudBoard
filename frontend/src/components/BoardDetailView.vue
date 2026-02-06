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
        :workItems="displayWorkItems"
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
        @create-workitem="openBoardAddItemModal"
        @edit-workitem="editWorkItem"
        @delete-workitem="handleDelete"
        @update-status="handleUpdateWorkItemStatus"
        @return-to-backlog="handleReturnToBacklog"
        @add-child-task="handleAddChildTask"
        @view-details="handleViewDetails"
        @work-item-updated="handleWorkItemUpdated"
      />

      <!-- Kanban Board (for Kanban and Backlog boards) -->
      <KanbanBoardView
        v-else
        :board="board"
        :boardId="boardId"
        :projectBoards="boardStore.boards"
        :workItems="displayWorkItems"
        @switch-board="switchBoard"
        @create-board="openCreateBoardModal"
        @edit-board="editCurrentBoard"
        @delete-board="handleDeleteCurrentBoard"
        @create-workitem="openBoardAddItemModal"
        @edit-workitem="editWorkItem"
        @delete-workitem="handleDelete"
        @update-status="handleUpdateWorkItemStatus"
        @return-to-backlog="handleReturnToBacklog"
        @add-child-task="handleAddChildTask"
        @view-details="handleViewDetails"
        @work-item-updated="handleWorkItemUpdated"
      />
    </div>

    <!-- Modals stay in BoardDetailView -->
    <WorkItemModal
      v-if="showWorkItemModal"
      :workItem="selectedWorkItem"
      :boardId="boardId"
      :defaultStatus="defaultStatus"
      :defaultType="defaultType"
      :parentId="defaultParentId"
      :sprintId="selectedSprintId"
      :availableStatuses="availableStatuses"
      :availableParents="workItems"
      :teamMembers="projectTeamMembers"
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

    <!-- New Components -->
    <BoardAddItemModal
      v-if="showBoardAddItemModal"
      :boardId="boardId"
      :defaultStatus="defaultStatus"
      :workItems="workItems"
      :availableStatuses="availableStatuses"
      :sprintId="selectedSprintId"
      :parentPreselected="defaultParentId || undefined"
      :teamMembers="projectTeamMembers"
      @close="closeBoardAddItemModal"
      @create="handleCreateBoardItem"
    />

    <WorkItemDetailPanel
      v-if="showDetailPanel"
      :details="detailWorkItem"
      :loading="detailLoading"
      @close="closeDetailPanel"
      @navigate="handleViewDetails"
      @edit="onEditFromDetails"
      @add-child="onAddChildFromDetails"
      @delete="handleDelete"
      @return-to-backlog="handleReturnToBacklog"
    />
  </div>
</template>


<script lang="ts">
import { defineComponent, ref, onMounted, watch, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { getBoard, createWorkItem, updateWorkItem, deleteWorkItem, returnWorkItemToBacklog, getWorkItemDetails } from '@/services/api';
import { useConfirm } from '@/composables/useConfirm';
import { useSprintStore } from '@/stores/sprint';
import { useBoardStore } from '@/stores/boards';
import { useTeamsStore } from '@/stores/teams';
import { useToast } from '@/composables/useToast';
import type { Board, BoardColumn } from '@/types/Project';
import type { WorkItem, WorkItemCreate, WorkItemDetailDto, WorkItemType } from '@/types/WorkItem';
import type { Sprint, CreateSprintDto, UpdateSprintDto } from '@/types/Sprint';
import { BOARD_TYPES, getStatusesFromBoard } from '@/types/Project';
import WorkItemModal from './workItem/WorkItemModal.vue';
import BoardAddItemModal from './workItem/BoardAddItemModal.vue';
import WorkItemDetailPanel from './workItem/WorkItemDetailPanel.vue';
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
    BoardAddItemModal,
    WorkItemDetailPanel,
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
    const teamsStore = useTeamsStore();
    const { success, error: toastError } = useToast();

    const board = ref<Board | null>(null);
    const workItems = ref<WorkItem[]>([]);
    const projectTeamMembers = ref<any[]>([])
    const loading = ref(false);
    
    // WorkItem Modal
    const showWorkItemModal = ref(false);
    const selectedWorkItem = ref<WorkItem | null>(null);
    const defaultStatus = ref<string>('To Do');
    const defaultParentId = ref<number | null>(null);
    const defaultType = ref<WorkItemType>('Task');

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

    // New workflow components state
    const showBoardAddItemModal = ref(false);
    const showDetailPanel = ref(false);
    const detailWorkItem = ref<WorkItemDetailDto | null>(null);
    const detailLoading = ref(false);
    
    const selectedSprint = computed(() => {
      if (selectedSprintId.value === null) return undefined;
      return sprintStore.sprints.find(s => s.id === selectedSprintId.value);
    });

    const displayWorkItems = computed(() => {
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
        
        // Auto-select sprint with hierarchy: Active > Planning > Latest Completed
        if (selectedSprintId.value === null) {
          const sprints = sprintStore.sprints;
          
          // Priority 1: Active sprint
          const activeSprint = sprints.find(s => s.status === 'Active');
          if (activeSprint) {
            selectedSprintId.value = activeSprint.id;
            return;
          }
          
          // Priority 2: First planning sprint
          const planningSprint = sprints.find(s => s.status === 'Planning');
          if (planningSprint) {
            selectedSprintId.value = planningSprint.id;
            return;
          }
          
          // Priority 3: Latest completed sprint
          const completedSprints = sprints
            .filter(s => s.status === 'Completed')
            .sort((a, b) => new Date(b.endDate).getTime() - new Date(a.endDate).getTime());
          if (completedSprints.length > 0) {
            selectedSprintId.value = completedSprints[0]!.id;
          }
        }
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

    const fetchTeamMembers = async () => {
      try {
        projectTeamMembers.value = await teamsStore.getProjectTeamMembers()
      } catch (error) {
        console.error('Failed to fetch team members:', error)
      }
    }

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
        success('Sprint saved successfully');
      } catch (err) {
        console.error('Failed to save sprint:', err);
        toastError('Failed to save sprint');
      }
    };

    const handleStartSprint = async (sprintId: number) => {
      try {
        await sprintStore.startSprint(sprintId);
        await fetchSprints();
        success('Sprint started!');
      } catch (err) {
        console.error('Failed to start sprint:', err);
        toastError('Failed to start sprint. Make sure no other sprint is active.');
      }
    };

    const handleCompleteSprint = async (sprintId: number) => {
      if (confirm('Complete this sprint? Incomplete items will be moved to the backlog.')) {
        try {
          const result = await sprintStore.completeSprint(sprintId);
          await fetchSprints();
          await fetchBoard();
          success(`Sprint completed! ${result.movedToBacklog} items moved to backlog.`);
        } catch (err) {
          console.error('Failed to complete sprint:', err);
          toastError('Failed to complete sprint');
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
          success('Sprint deleted');
        } catch (err) {
          console.error('Failed to delete sprint:', err);
          toastError('Failed to delete sprint');
        }
      }
    };

    const handleViewDetails = async (id: number) => {
      showDetailPanel.value = true;
      detailLoading.value = true;
      try {
        const response = await getWorkItemDetails(id);
        detailWorkItem.value = response.data;
      } catch (err) {
        console.error('Failed to fetch work item details:', err);
        toastError('Failed to load details');
        showDetailPanel.value = false;
      } finally {
        detailLoading.value = false;
      }
    };

    const refreshOpenDetails = async () => {
      if (showDetailPanel.value && detailWorkItem.value) {
        await handleViewDetails(detailWorkItem.value.id);
      }
    };

    const openCreateModalWithStatus = (status: string) => {
      selectedWorkItem.value = null;
      defaultStatus.value = status;
      showWorkItemModal.value = true;
    };

    const openBoardAddItemModal = (status: string) => {
      defaultStatus.value = status;
      showBoardAddItemModal.value = true;
    };

    const closeBoardAddItemModal = () => {
      showBoardAddItemModal.value = false;
      defaultParentId.value = null;
    };

    const handleCreateBoardItem = async (data: WorkItemCreate) => {
      try {
        await createWorkItem(boardId.value, data);
        closeBoardAddItemModal();
        
        await Promise.all([
          fetchBoard(),
          fetchSprints(),
          refreshOpenDetails()
        ]);

        success('Item created successfully');
      } catch (err) {
        console.error('Failed to create board item:', err);
        toastError('Failed to create item');
      }
    };

    const closeDetailPanel = () => {
      showDetailPanel.value = false;
      detailWorkItem.value = null;
    };

    const onEditFromDetails = (details: WorkItemDetailDto) => {
      // Convert DTO to WorkItem for the modal (rough conversion as partials are fine)
      const workItem = workItems.value.find(w => w.id === details.id);
      if (workItem) {
        editWorkItem(workItem);
      }
    };

    const onAddChildFromDetails = (details: WorkItemDetailDto) => {
      const parent = workItems.value.find(w => w.id === details.id);
      if (parent) {
        handleAddChildTask(parent);
      }
    };

    const editWorkItem = (workItem: WorkItem) => {
      selectedWorkItem.value = workItem;
      showWorkItemModal.value = true;
    };

    const handleAddChildTask = (parentWorkItem: WorkItem) => {
      selectedWorkItem.value = null;
      defaultParentId.value = parentWorkItem.id;
      defaultStatus.value = parentWorkItem.status;
      showBoardAddItemModal.value = true;
    };

    const closeWorkItemModal = () => {
      showWorkItemModal.value = false;
      selectedWorkItem.value = null;
      defaultParentId.value = null;
      defaultType.value = 'Task';
    };

    const handleSaveWorkItem = async (workItemData: WorkItem | WorkItemCreate) => {
      try {
        if ('id' in workItemData && workItemData.id) {
          await updateWorkItem(boardId.value, workItemData.id, workItemData as WorkItem);
        } else {
          await createWorkItem(boardId.value, workItemData as WorkItemCreate);
        }
        closeWorkItemModal();
        
        await Promise.all([
          fetchBoard(),
          fetchSprints(),
          refreshOpenDetails()
        ]);
        
        success('Item saved');
      } catch (error) {
        console.error('Failed to save WorkItem:', error);
      }
    };

    const handleDelete = async (id: number) => {
      if (confirm('Are you sure you want to delete this WorkItem?')) {
        try {
          await deleteWorkItem(boardId.value, id);
          
          // If the deleted item was the one in the detail panel, close it
          if (detailWorkItem.value?.id === id) {
            closeDetailPanel();
          }

          await Promise.all([
            fetchBoard(),
            fetchSprints(),
            refreshOpenDetails()
          ]);
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
        toastError('Board name is required');
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
          success('Board created');
        }
        closeBoardModal();
      } catch (err) {
        console.error('Failed to save board:', err);
        toastError('Failed to save board');
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
          success('Board deleted');
        } catch (err) {
          console.error('Failed to delete board:', err);
          toastError('Failed to delete board');
        }
      }
    };

    const handleUpdateWorkItemStatus = async (workItem: WorkItem, newStatus: string) => {
      try {
        await updateWorkItem(boardId.value, workItem.id, {
          ...workItem,
          status: newStatus
        });
        
        await Promise.all([
          fetchBoard(),
          refreshOpenDetails()
        ]);
      } catch (error) {
        console.error('Failed to update WorkItem status:', error);
      }
    };

    const handleWorkItemUpdated = async (updatedItem: WorkItem) => {
      const index = workItems.value.findIndex(w => w.id === updatedItem.id);
      if (index !== -1) {
        workItems.value[index] = updatedItem;
      }
      
      // Also refresh details panel if open
      if (detailWorkItem.value?.id === updatedItem.id) {
         await refreshOpenDetails();
      }
    };

    const handleMoveToBacklog = async (workItem: WorkItem) => {
      try {
        await returnWorkItemToBacklog(workItem.id);
        
        await Promise.all([
          fetchBoard(),
          refreshOpenDetails()
        ]);

        if (detailWorkItem.value?.id === workItem.id) {
          closeDetailPanel();
        }
        success('Item returned to backlog');
      } catch (error) {
        console.error('Failed to return work item to backlog:', error);
        toastError('Failed to return item to backlog');
      }
    };

    const handleReturnToBacklog = async (workItem: { id: number }) => {
      if (confirm('Return this item to the backlog? It will be removed from this board.')) {
        await handleMoveToBacklog(workItem as WorkItem);
      }
    }

    onMounted(async () => {
      // Fetch boards for sidebar sync
      await boardStore.fetchBoards(projectId.value);
      await fetchBoard();
      await fetchSprints();
      await fetchTeamMembers();
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
      projectTeamMembers,
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
      displayWorkItems,
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
      handleWorkItemUpdated,
      handleReturnToBacklog,
      handleAddChildTask,
      defaultParentId,
      defaultType,
      showBoardAddItemModal,
      showDetailPanel,
      detailWorkItem,
      detailLoading,
      openBoardAddItemModal,
      closeBoardAddItemModal,
      handleCreateBoardItem,
      handleViewDetails,
      closeDetailPanel,
      onEditFromDetails,
      onAddChildFromDetails
    };
  }
});
</script>

<style scoped>
.rotate-180 {
  transform: rotate(180deg);
}
</style>