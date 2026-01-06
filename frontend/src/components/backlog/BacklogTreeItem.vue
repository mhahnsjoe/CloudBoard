<template>
  <div class="tree-item">
    <!-- Main Row -->
    <div 
      class="tree-row group"
      :class="{ 
        'bg-blue-50': isSelected,
        'hover:bg-gray-50': !isSelected 
      }"
    >
      <!-- Indentation & Expand Toggle -->
      <div class="flex items-center flex-shrink-0" :style="{ paddingLeft: `${node.depth * 24}px` }">
        <!-- Tree connector line for nested items -->
        <div v-if="node.depth > 0" class="flex items-center mr-1">
          <span class="text-gray-300 text-sm">└</span>
        </div>
        
        <!-- Expand/Collapse Button -->
        <button
          v-if="node.children.length > 0"
          @click.stop="$emit('toggle', node.id)"
          class="w-6 h-6 flex items-center justify-center hover:bg-gray-200 rounded transition-colors mr-1"
        >
          <svg 
            class="w-4 h-4 text-gray-500 transition-transform duration-200"
            :class="{ 'rotate-90': node.expanded }"
            fill="none" 
            stroke="currentColor" 
            viewBox="0 0 24 24"
          >
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
          </svg>
        </button>
        <!-- Spacer for leaf nodes -->
        <div v-else class="w-6 h-6 mr-1"></div>

        <!-- Work Item Type Icon -->
        <WorkItemTypeBadge :type="node.type" class="mr-3" />
      </div>

      <!-- ID -->
      <div class="w-16 text-sm font-mono text-gray-500">
        #{{ node.id }}
      </div>

      <!-- Title (clickable) -->
      <div 
        class="flex-1 min-w-0 cursor-pointer"
        @click="$emit('select', node)"
      >
        <span class="text-sm font-medium text-gray-900 hover:text-blue-600 truncate block">
          {{ node.title }}
        </span>
      </div>

      <!-- Status Badge -->
      <div class="w-28">
        <span 
          class="inline-flex px-2 py-1 text-xs font-medium rounded-full"
          :class="getStatusClass(node.status)"
        >
          {{ node.status }}
        </span>
      </div>

      <!-- Priority -->
      <div class="w-24">
        <span 
          class="inline-flex px-2 py-1 text-xs font-medium rounded-full"
          :class="getPriorityClass(node.priority)"
        >
          {{ node.priority }}
        </span>
      </div>

      <!-- Actions (visible on hover) -->
      <div class="w-28 flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
        <!-- Move to Board -->
        <button
          @click.stop="$emit('move-to-board', node)"
          class="p-1.5 hover:bg-indigo-100 rounded transition-colors"
          title="Move to Board"
        >
          <svg class="w-4 h-4 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7l5 5m0 0l-5 5m5-5H6"/>
          </svg>
        </button>

        <!-- Add Child (only if can have children) -->
        <button
          v-if="canHaveChildren"
          @click.stop="$emit('add-child', node)"
          class="p-1.5 hover:bg-green-100 rounded transition-colors"
          title="Add child item"
        >
          <svg class="w-4 h-4 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
          </svg>
        </button>

        <!-- Edit -->
        <button
          @click.stop="$emit('edit', node)"
          class="p-1.5 hover:bg-blue-100 rounded transition-colors"
          title="Edit"
        >
          <EditIcon className="w-4 h-4 text-blue-600" />
        </button>

        <!-- Delete -->
        <button
          @click.stop="$emit('delete', node.id, node.boardId)"
          class="p-1.5 hover:bg-red-100 rounded transition-colors"
          title="Delete"
        >
          <DeleteIcon className="w-4 h-4 text-red-600" />
        </button>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType, computed } from 'vue'
import type { TreeNode } from '@/composables/useWorkItemTree'
import { HIERARCHY_RULES } from '@/composables/useWorkItemTree'
import WorkItemTypeBadge from '@/components/workItem/WorkItemTypeBadge.vue'
import { EditIcon, DeleteIcon } from '@/components/icons'

export default defineComponent({
  name: 'BacklogTreeItem',
  components: {
    WorkItemTypeBadge,
    EditIcon,
    DeleteIcon
  },
  props: {
    node: {
      type: Object as PropType<TreeNode>,
      required: true
    },
    isSelected: {
      type: Boolean,
      default: false
    }
  },
  emits: ['toggle', 'select', 'edit', 'delete', 'add-child', 'move-to-board'],
  setup(props) {
    const canHaveChildren = computed(() => {
      return HIERARCHY_RULES[props.node.type]?.length > 0
    })

    const getStatusClass = (status: string) => {
      const classes: Record<string, string> = {
        'To Do': 'bg-gray-100 text-gray-700',
        'In Progress': 'bg-blue-100 text-blue-700',
        'Done': 'bg-green-100 text-green-700'
      }
      return classes[status] || 'bg-gray-100 text-gray-700'
    }

    const getPriorityClass = (priority: string) => {
      const classes: Record<string, string> = {
        'Low': 'bg-gray-100 text-gray-600',
        'Medium': 'bg-yellow-100 text-yellow-700',
        'High': 'bg-orange-100 text-orange-700',
        'Critical': 'bg-red-100 text-red-700'
      }
      return classes[priority] || 'bg-gray-100 text-gray-600'
    }

    return {
      canHaveChildren,
      getStatusClass,
      getPriorityClass
    }
  }
})
</script>

<style scoped>
.tree-row {
  @apply flex items-center py-2.5 px-4 border-b border-gray-100;
  min-height: 48px;
}

.rotate-90 {
  transform: rotate(90deg);
}
</style>