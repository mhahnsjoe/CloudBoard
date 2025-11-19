<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <div v-if="loading" class="flex items-center justify-center gap-2 mt-8">
      <LoadingIcon className="h-5 w-5 text-blue-600" />
      Loading backlog...
    </div>

    <div v-else>
      <!-- Header -->
      <div class="mb-6">
        <div class="flex items-center justify-between mb-4">
          <div>
            <h1 class="text-3xl font-bold text-gray-900">Backlog</h1>
            <p class="text-gray-600 mt-1">All work items across {{ currentProjectName }}</p>
          </div>
          <button
            @click="openCreateWorkItemModal"
            class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-all shadow-sm hover:shadow-md flex items-center gap-2"
          >
            <PlusIcon className="w-5 h-5" />
            New Work Item
          </button>
        </div>

        <!-- Filters -->
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
          <div class="flex items-center gap-4">
            <span class="text-sm font-medium text-gray-700">Filter by Status:</span>
            <label
              v-for="status in STATUSES"
              :key="status"
              class="flex items-center gap-2 cursor-pointer"
            >
              <input
                type="checkbox"
                :value="status"
                v-model="selectedStatuses"
                class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
              />
              <span class="text-sm text-gray-700">{{ status }}</span>
            </label>
          </div>
        </div>
      </div>

      <!-- Work Items List -->
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-20">
                ID
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-32">
                Type
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Title
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-32">
                Status
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-32">
                Priority
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-32">
                Board
              </th>
              <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider w-20">
                Actions
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <BacklogListItem
              v-for="item in filteredWorkItems"
              :key="item.id"
              :workItem="item"
              :boardName="getBoardName(item.boardId)"
              @edit="editWorkItem"
              @delete="handleDelete"
            />
          </tbody>
        </table>

        <!-- Empty State -->
        <div v-if="filteredWorkItems.length === 0" class="text-center py-12">
          <ClipboardIcon className="w-16 h-16 mx-auto mb-4 text-gray-300" />
          <p class="text-gray-500 text-lg">No work items found</p>
          <p class="text-gray-400 text-sm mt-1">
            {{ allWorkItems.length === 0 ? 'Create your first work item to get started' : 'Try adjusting your filters' }}
          </p>
        </div>
      </div>
    </div>

    <!-- Work Item Modal -->
    <WorkItemModal
      v-if="showWorkItemModal"
      :workItem="selectedWorkItem"
      :boardId="defaultBoardId"
      :defaultStatus="'To Do'"
      @close="closeWorkItemModal"
      @save="handleSaveWorkItem"
    />
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { getBoards, createWorkItem, updateWorkItem, deleteWorkItem } from '@/services/api';
import { useConfirm } from '@/composables/useConfirm';
import type { Board } from '@/types/Project';
import type { WorkItem, WorkItemCreate } from '@/types/WorkItem';
import { STATUSES } from '@/types/Project';
import WorkItemModal from './workItem/WorkItemModal.vue';
import BacklogListItem from './backlog/BacklogListItem.vue';
import { LoadingIcon, PlusIcon, ClipboardIcon } from '@/components/icons';

export default defineComponent({
  name: 'BacklogView',
  components: {
    WorkItemModal,
    BacklogListItem,
    LoadingIcon,
    PlusIcon,
    ClipboardIcon
  },
  setup() {
    const route = useRoute();
    const { confirm } = useConfirm();
    const projectId = ref(Number(route.params.projectId));
    
    const boards = ref<Board[]>([]);
    const allWorkItems = ref<WorkItem[]>([]);
    const loading = ref(false);
    const selectedStatuses = ref<string[]>([...STATUSES]);
    
    // Work Item Modal
    const showWorkItemModal = ref(false);
    const selectedWorkItem = ref<WorkItem | null>(null);

    const currentProjectName = computed(() => {
      // You might want to pass this as a prop or get it from a store
      return 'Project';
    });

    const defaultBoardId = computed(() => {
      return boards.value[0]?.id || 0;
    });

    const filteredWorkItems = computed(() => {
      return allWorkItems.value.filter(item => 
        selectedStatuses.value.includes(item.status)
      );
    });

    const getBoardName = (boardId: number) => {
      const board = boards.value.find(b => b.id === boardId);
      return board?.name || 'Unknown';
    };

    const fetchBacklogData = async () => {
      loading.value = true;
      try {
        const res = await getBoards(projectId.value);
        boards.value = res.data;
        
        // Collect all work items from all boards
        allWorkItems.value = boards.value.flatMap(board => 
          board.workItems || []
        );
      } catch (error) {
        console.error('Failed to fetch backlog data:', error);
      } finally {
        loading.value = false;
      }
    };

    const openCreateWorkItemModal = () => {
      selectedWorkItem.value = null;
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
        const boardId = 'id' in workItemData && workItemData.id 
          ? workItemData.boardId 
          : defaultBoardId.value;

        if ('id' in workItemData && workItemData.id) {
          await updateWorkItem(boardId, workItemData.id, workItemData as WorkItem);
        } else {
          await createWorkItem(boardId, { ...workItemData as WorkItemCreate, boardId });
        }
        closeWorkItemModal();
        await fetchBacklogData();
      } catch (error) {
        console.error('Failed to save work item:', error);
        alert('Failed to save work item');
      }
    };

    const handleDelete = async (id: number, boardId: number) => {
      if (confirm('Are you sure you want to delete this work item?')) {
        try {
          await deleteWorkItem(boardId, id);
          await fetchBacklogData();
        } catch (error) {
          console.error('Failed to delete work item:', error);
          alert('Failed to delete work item');
        }
      }
    };

    onMounted(() => {
      fetchBacklogData();
    });

    return {
      loading,
      allWorkItems,
      filteredWorkItems,
      selectedStatuses,
      showWorkItemModal,
      selectedWorkItem,
      currentProjectName,
      defaultBoardId,
      STATUSES,
      getBoardName,
      openCreateWorkItemModal,
      editWorkItem,
      closeWorkItemModal,
      handleSaveWorkItem,
      handleDelete
    };
  }
});
</script>

