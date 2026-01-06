<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <!-- Loading State -->
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
            <p class="text-gray-600 mt-1">All work items across {{ projectName }}</p>
          </div>
          <div class="flex items-center gap-3">
            <!-- Quick Add Dropdown -->
            <div class="relative" ref="addDropdownRef">
              <button
                @click="addDropdownOpen = !addDropdownOpen"
                class="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-all shadow-sm hover:shadow-md flex items-center gap-2"
              >
                <PlusIcon className="w-5 h-5" />
                New Work Item
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
                </svg>
              </button>

              <!-- Type Selection Dropdown -->
              <div
                v-if="addDropdownOpen"
                class="absolute top-full right-0 mt-2 w-56 bg-white border border-gray-200 rounded-lg shadow-xl z-50"
              >
                <div class="p-2">
                  <button
                    v-for="type in creatableTypes"
                    :key="type"
                    @click="openCreateModal(type)"
                    class="w-full flex items-center gap-3 px-3 py-2 hover:bg-gray-50 rounded-lg transition-colors"
                  >
                    <WorkItemTypeBadge :type="type" />
                    <span class="text-sm text-gray-700">New {{ type }}</span>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Filters -->
        <BacklogFilters
          v-model:searchQuery="searchQuery"
          v-model:selectedTypes="selectedTypes"
          v-model:selectedStatuses="selectedStatuses"
          :filteredCount="treeHelpers.filteredItems.value.length"
          :totalCount="allWorkItems.length"
          @expand-all="treeHelpers.expandAll"
          @collapse-all="treeHelpers.collapseAll"
          @clear-filters="clearAllFilters"
        />
      </div>

      <!-- Tree View -->
      <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden">
        <!-- Table Header -->
        <div class="flex items-center py-3 px-4 bg-gray-50 border-b border-gray-200 text-xs font-medium text-gray-500 uppercase tracking-wider">
          <div class="w-6 mr-1"></div> <!-- Expand spacer -->
          <div class="w-24">Type</div>
          <div class="w-16">ID</div>
          <div class="flex-1">Title</div>
          <div class="w-28">Status</div>
          <div class="w-24">Priority</div>
          <div class="w-28">Actions</div>
        </div>

        <!-- Tree Items -->
        <div v-if="treeHelpers.flattenedTree.value.length > 0">
          <BacklogTreeItem
            v-for="node in treeHelpers.flattenedTree.value"
            :key="node.id"
            :node="node"
            :isSelected="selectedItemId === node.id"
            @toggle="treeHelpers.toggleExpanded"
            @select="handleSelectItem"
            @edit="openEditModal"
            @delete="handleDelete"
            @add-child="openCreateChildModal"
            @move-to-board="openMoveToBoardModal"
          />
        </div>

        <!-- Empty State -->
        <div v-else class="text-center py-16">
          <ClipboardIcon className="w-16 h-16 mx-auto mb-4 text-gray-300" />
          <p class="text-gray-500 text-lg">No work items found</p>
          <p class="text-gray-400 text-sm mt-1">
            {{ allWorkItems.length === 0 
              ? 'Create your first work item to get started' 
              : 'Try adjusting your filters' 
            }}
          </p>
          <button
            v-if="allWorkItems.length === 0"
            @click="openCreateModal('Epic')"
            class="mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Create your first Epic
          </button>
        </div>
      </div>

      <!-- Hierarchy Legend -->
      <div class="mt-4 flex items-center gap-6 text-sm text-gray-500">
        <span class="font-medium">Hierarchy:</span>
        <div class="flex items-center gap-4">
          <span class="flex items-center gap-1">
            <span class="w-3 h-3 rounded bg-orange-200"></span>
            Epic
          </span>
          <span>→</span>
          <span class="flex items-center gap-1">
            <span class="w-3 h-3 rounded bg-purple-200"></span>
            Feature
          </span>
          <span>→</span>
          <span class="flex items-center gap-1">
            <span class="w-3 h-3 rounded bg-blue-200"></span>
            PBI
          </span>
          <span>→</span>
          <span class="flex items-center gap-1">
            <span class="w-3 h-3 rounded bg-yellow-200"></span>
            Task
          </span>
        </div>
        <span class="text-gray-400">|</span>
        <span class="flex items-center gap-1">
          <span class="w-3 h-3 rounded bg-red-200"></span>
          Bug (can be added anywhere)
        </span>
      </div>
    </div>

    <!-- Work Item Modal -->
    <WorkItemModal
      v-if="showWorkItemModal"
      :workItem="selectedWorkItem"
      :boardId="selectedBoardId"
      :defaultStatus="'To Do'"
      :defaultType="defaultType"
      :parentId="selectedParentId"
      :availableParents="availableParents"
      :boards="boards"
      @close="closeWorkItemModal"
      @save="handleSaveWorkItem"
    />

    <!-- Move To Board Modal -->
    <MoveToBoardModal
      v-if="showMoveToBoardModal"
      :workItem="workItemToMove"
      :boards="boards"
      @close="closeMoveToBoardModal"
      @move="handleMoveToBoard"
    />
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { 
  getBoards, 
  getProjectBacklog, 
  createBacklogItem, 
  updateWorkItem, 
  deleteWorkItem,
  moveToBoard 
} from '@/services/api'
import { useConfirm } from '@/composables/useConfirm'
import { useWorkItemTree, HIERARCHY_RULES } from '@/composables/useWorkItemTree'
import type { Board } from '@/types/Project'
import type { WorkItem, WorkItemType, WorkItemCreate } from '@/types/WorkItem'
import WorkItemModal from '@/components/workItem/WorkItemModal.vue'
import MoveToBoardModal from '@/components/workItem/MoveToBoardModal.vue'
import BacklogTreeItem from './BacklogTreeItem.vue'
import BacklogFilters from './BacklogFilters.vue'
import WorkItemTypeBadge from '@/components/workItem/WorkItemTypeBadge.vue'
import { LoadingIcon, PlusIcon, ClipboardIcon } from '@/components/icons'

export default defineComponent({
  name: 'BacklogView',
  components: {
    WorkItemModal,
    MoveToBoardModal,
    BacklogTreeItem,
    BacklogFilters,
    WorkItemTypeBadge,
    LoadingIcon,
    PlusIcon,
    ClipboardIcon
  },
  setup() {
    const route = useRoute()
    const { confirm } = useConfirm()
    const projectId = ref(Number(route.params.projectId))
    
    // Data
    const boards = ref<Board[]>([])
    const allWorkItems = ref<WorkItem[]>([])
    const loading = ref(false)
    const projectName = ref('Project')
    
    // UI State
    const addDropdownOpen = ref(false)
    const addDropdownRef = ref<HTMLElement | null>(null)
    const selectedItemId = ref<number | null>(null)
    
    // Work Item Modal State
    const showWorkItemModal = ref(false)
    const selectedWorkItem = ref<WorkItem | null>(null)
    const selectedBoardId = ref<number>(0)
    const selectedParentId = ref<number | null>(null)
    const defaultType = ref<WorkItemType>('Task')

    // Move To Board Modal State
    const showMoveToBoardModal = ref(false)
    const workItemToMove = ref<WorkItem | null>(null)

    // Filter State
    const searchQuery = ref('')
    const selectedTypes = ref<WorkItemType[]>([])
    const selectedStatuses = ref<string[]>([])

    // Tree helpers
    const treeHelpers = useWorkItemTree(allWorkItems)

    // Watch filters and update tree
    watch(searchQuery, (val) => treeHelpers.setSearchQuery(val))
    watch(selectedTypes, (val) => treeHelpers.setTypeFilter(val))
    watch(selectedStatuses, (val) => treeHelpers.setStatusFilter(val))

    // Types that can be created at root level
    const creatableTypes: WorkItemType[] = ['Epic', 'Feature', 'PBI', 'Task', 'Bug']

    // Available parents for the modal
    const availableParents = computed(() => {
      if (!defaultType.value) return []
      return treeHelpers.getValidParents(defaultType.value, selectedWorkItem.value?.id)
    })

    // =============================================
    // DATA FETCHING
    // =============================================
    
    const fetchBacklogItems = async () => {
      loading.value = true
      try {
        // Fetch backlog items (items with no board assigned)
        const backlogRes = await getProjectBacklog(projectId.value)
        allWorkItems.value = backlogRes.data
        
        // Also fetch boards for the "Move to Board" modal
        const boardsRes = await getBoards(projectId.value)
        boards.value = boardsRes.data
        
        // Set default board for potential use
        if (boards.value.length > 0) {
          selectedBoardId.value = boards.value[0]!.id
        }
      } catch (error) {
        console.error('Failed to fetch backlog data:', error)
      } finally {
        loading.value = false
      }
    }

    // =============================================
    // WORK ITEM MODAL HANDLERS
    // =============================================

    const openCreateModal = (type: WorkItemType) => {
      addDropdownOpen.value = false
      selectedWorkItem.value = null
      selectedParentId.value = null
      defaultType.value = type
      showWorkItemModal.value = true
    }

    const openCreateChildModal = (parent: WorkItem) => {
      selectedWorkItem.value = null
      selectedParentId.value = parent.id
      
      // Set default type to first allowed child type
      const allowedChildren = HIERARCHY_RULES[parent.type]
      defaultType.value = allowedChildren[0] || 'Task'
      
      showWorkItemModal.value = true
    }

    const openEditModal = (workItem: WorkItem) => {
      selectedWorkItem.value = workItem
      selectedBoardId.value = workItem.boardId || 0
      selectedParentId.value = workItem.parentId || null
      defaultType.value = workItem.type
      showWorkItemModal.value = true
    }

    const closeWorkItemModal = () => {
      showWorkItemModal.value = false
      selectedWorkItem.value = null
      selectedParentId.value = null
    }

    const handleSelectItem = (item: WorkItem) => {
      selectedItemId.value = item.id
      openEditModal(item)
    }

    const handleSaveWorkItem = async (workItemData: WorkItem | WorkItemCreate) => {
      try {
        if ('id' in workItemData && workItemData.id) {
          // Update existing - use the boardId if it has one, or 0 for backlog items
          const boardId = workItemData.boardId || 0
          await updateWorkItem(boardId, workItemData.id, workItemData as WorkItem)
        } else {
          // Create new backlog item (no board assigned)
          await createBacklogItem(projectId.value, { 
            ...workItemData as WorkItemCreate,
            parentId: selectedParentId.value || undefined
          })
        }
        closeWorkItemModal()
        await fetchBacklogItems()
      } catch (error) {
        console.error('Failed to save work item:', error)
        alert('Failed to save work item')
      }
    }

    const handleDelete = async (id: number, boardId: number) => {
      // Check if item has children
      const hasChildren = allWorkItems.value.some(i => i.parentId === id)
      
      if (hasChildren) {
        alert('Cannot delete this item because it has child items. Delete or move the children first.')
        return
      }

      if (confirm('Are you sure you want to delete this work item?')) {
        try {
          await deleteWorkItem(boardId || 0, id)
          await fetchBacklogItems()
        } catch (error) {
          console.error('Failed to delete work item:', error)
          alert('Failed to delete work item')
        }
      }
    }

    // =============================================
    // MOVE TO BOARD MODAL HANDLERS
    // =============================================

    const openMoveToBoardModal = (workItem: WorkItem) => {
      workItemToMove.value = workItem
      showMoveToBoardModal.value = true
    }

    const closeMoveToBoardModal = () => {
      showMoveToBoardModal.value = false
      workItemToMove.value = null
    }

    const handleMoveToBoard = async (workItemId: number, boardId: number) => {
      if (!workItemToMove.value) return
      
      try {
        await moveToBoard(workItemId, boardId)
        closeMoveToBoardModal()
        await fetchBacklogItems() // Refresh - item will disappear from backlog
      } catch (error) {
        console.error('Failed to move work item to board:', error)
        alert('Failed to move work item to board')
      }
    }

    // =============================================
    // FILTER HANDLERS
    // =============================================

    const clearAllFilters = () => {
      searchQuery.value = ''
      selectedTypes.value = []
      selectedStatuses.value = []
      treeHelpers.clearFilters()
    }

    // Close dropdown on click outside
    const handleClickOutside = (event: MouseEvent) => {
      if (addDropdownRef.value && !addDropdownRef.value.contains(event.target as Node)) {
        addDropdownOpen.value = false
      }
    }

    // =============================================
    // LIFECYCLE
    // =============================================

    onMounted(() => {
      fetchBacklogItems()
      document.addEventListener('click', handleClickOutside)
    })

    // Watch for route changes
    watch(() => route.params.projectId, (newId) => {
      if (newId) {
        projectId.value = Number(newId)
        fetchBacklogItems()
      }
    })

    return {
      loading,
      boards,
      allWorkItems,
      projectName,
      treeHelpers,
      addDropdownOpen,
      addDropdownRef,
      selectedItemId,
      // Work Item Modal
      showWorkItemModal,
      selectedWorkItem,
      selectedBoardId,
      selectedParentId,
      defaultType,
      availableParents,
      // Move To Board Modal
      showMoveToBoardModal,
      workItemToMove,
      // Filter state
      searchQuery,
      selectedTypes,
      selectedStatuses,
      creatableTypes,
      // Methods
      openCreateModal,
      openCreateChildModal,
      openEditModal,
      closeWorkItemModal,
      handleSelectItem,
      handleSaveWorkItem,
      handleDelete,
      openMoveToBoardModal,
      closeMoveToBoardModal,
      handleMoveToBoard,
      clearAllFilters
    }
  }
})
</script>