<template> <!--TODO: RENAME TO WORKITEM CARD? BoardCanvas use this for both sprint & kanban-->
  <div class="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-all">
    <!-- Parent Item -->
    <div
      class="p-4 cursor-move group"
      draggable="true"
      @dragstart="$emit('dragstart', workItem)"
    >
      <!-- WorkItem Type Badge -->
      <div class="flex items-center justify-between mb-2">
        <div class="flex items-center gap-2">
          <!-- Expand/Collapse Button for Parents or PBIs/Features/Bugs -->
          <button
            v-if="hasChildren || workItem.type === 'PBI' || workItem.type === 'Feature' || workItem.type === 'Bug'"
            @click.stop="toggleExpanded"
            class="flex-shrink-0 text-gray-400 hover:text-gray-600"
          >
            <svg
              class="w-4 h-4 transition-transform"
              :class="{ 'rotate-90': isExpanded }"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </button>
          <WorkItemTypeBadge :type="workItem.type as WorkItemType" />
          <span v-if="hasChildren" class="text-xs text-blue-600 font-medium">
            {{ workItem.children.length }}
          </span>
        </div>
        <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
          <!-- Return to Backlog -->
          <button
            @click.stop="$emit('return-to-backlog', workItem)"
            class="p-1 hover:bg-purple-50 rounded transition"
            title="Return to Backlog"
          >
            <svg class="w-4 h-4 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 17l-5-5m0 0l5-5m-5 5h12"/>
            </svg>
          </button>
          <button
            @click.stop="$emit('edit', workItem)"
            class="p-1 hover:bg-gray-100 rounded transition"
          >
            <EditIcon className="w-4 h-4 text-gray-600" />
          </button>
          <button
            @click.stop="$emit('delete', workItem.id)"
            class="p-1 hover:bg-red-50 rounded transition"
          >
            <DeleteIcon className="w-4 h-4 text-red-600" />
          </button>
        </div>
      </div>

      <!-- WorkItem Title -->
      <h3 class="text-sm font-medium text-gray-800 mb-2">
        {{ workItem.title }}
      </h3>

      <!-- WorkItem Description (if exists) -->
      <p v-if="workItem.description" class="text-xs text-gray-600 mb-3 line-clamp-2">
        {{ workItem.description }}
      </p>

      <!-- WorkItem Meta Information -->
      <div class="flex items-center justify-between text-xs text-gray-500">
        <!-- Priority Badge -->
        <span :class="getPriorityClass(workItem.priority)" class="px-2 py-1 rounded">
          {{ workItem.priority }}
        </span>

        <!-- Due Date (if exists and with overdue warning) -->
        <div v-if="workItem.dueDate" class="flex items-center gap-1">
          <CalendarIcon className="w-3 h-3" />
          <span :class="{ 'text-red-600 font-medium': isOverdue(workItem.dueDate) }">
            {{ formatDueDate(workItem.dueDate) }}
          </span>
        </div>

        <!-- Time Tracking (if exists) - Show aggregated hours for parents -->
        <div v-if="totalEstimatedHours || workItem.actualHours" class="flex items-center gap-1">
          <ClockIcon className="w-3 h-3" />
          <span :title="hasChildren ? 'Total including children' : ''">
            {{ workItem.actualHours || 0 }}/{{ totalEstimatedHours }}h
          </span>
        </div>
      </div>
    </div>

    <!-- Children (Expanded) -->
    <div
      v-if="isExpanded && hasChildren"
      class="border-t border-gray-200 bg-gray-50 px-4 py-2 space-y-2"
    >
      <div
        v-for="child in workItem.children"
        :key="child.id"
        class="flex items-center justify-between text-xs py-2 px-3 bg-white rounded border border-gray-100 hover:border-gray-300 transition-colors group/child"
        @click.stop
      >
        <div class="flex items-center gap-2 flex-1 min-w-0">
          <WorkItemTypeBadge :type="child.type as WorkItemType" size="xs" />
          <span class="truncate text-gray-700">{{ child.title }}</span>
        </div>
        <div class="flex items-center gap-2">
          <span v-if="child.estimatedHours" class="text-gray-500 font-medium">
            {{ child.estimatedHours }}h
          </span>
          <div class="flex items-center gap-1 opacity-0 group-hover/child:opacity-100 transition-opacity">
            <button
              @click.stop="$emit('edit', child)"
              class="p-1 hover:bg-gray-100 rounded transition"
              title="Edit child item"
            >
              <EditIcon className="w-3 h-3 text-gray-600" />
            </button>
          </div>
        </div>
      </div>
    </div>
    <!-- Add Task Button (for PBIs, Features, and Bugs when expanded) -->
    <div
      v-if="isExpanded && (workItem.type === 'PBI' || workItem.type === 'Feature' || workItem.type === 'Bug')"
      class="border-t border-gray-200 bg-gray-50 px-4 py-2 relative z-10"
    >
      <button
        type="button"
        @click.stop="onAddChildTask"
        class="w-full py-2 px-3 border border-dashed border-gray-300 rounded text-xs text-gray-500 hover:border-blue-400 hover:text-blue-600 hover:bg-blue-50 transition-colors flex items-center justify-center gap-1 cursor-pointer"
      >
        <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
        </svg>
        Add Task
      </button>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType, ref, computed } from 'vue';
import type { WorkItem, WorkItemType } from '@/types/WorkItem';
import type { BoardColumn } from '@/types/Project';
import WorkItemTypeBadge from '../workItem/WorkItemTypeBadge.vue';
import { EditIcon, DeleteIcon, CalendarIcon, ClockIcon } from '@/components/icons';

export default defineComponent({
  name: 'KanbanCard',
  components: {
    WorkItemTypeBadge,
    EditIcon,
    DeleteIcon,
    CalendarIcon,
    ClockIcon
  },
  props: {
    workItem: {
      type: Object as PropType<WorkItem>,
      required: true
    },
    columns: {
      type: Array as PropType<BoardColumn[]>,
      default: () => []
    }
  },
  emits: ['dragstart', 'click', 'edit', 'delete', 'return-to-backlog', 'add-child-task'],
  setup(props) {
    const isExpanded = ref(false)
    
    const hasChildren = computed(() => {
      return props.workItem.children && props.workItem.children.length > 0
    })
    
    const totalEstimatedHours = computed(() => {
      if (!hasChildren.value) {
        return props.workItem.estimatedHours || 0
      }
      const childHours = props.workItem.children.reduce((sum: number, c: WorkItem) => sum + (c.estimatedHours || 0), 0)
      return (props.workItem.estimatedHours || 0) + childHours
    })
    
    const toggleExpanded = () => {
      isExpanded.value = !isExpanded.value
    }
    
    return {
      isExpanded,
      hasChildren,
      totalEstimatedHours,
      toggleExpanded
    }
  },
  methods: {
    getPriorityClass(priority: string) {
      const classes: Record<string, string> = {
        'Low': 'bg-gray-100 text-gray-600',
        'Medium': 'bg-blue-100 text-blue-700',
        'High': 'bg-orange-100 text-orange-700',
        'Critical': 'bg-red-100 text-red-700'
      };
      return classes[priority] || 'bg-gray-100 text-gray-600';
    },
    isOverdue(dueDate: string) {
      return new Date(dueDate) < new Date();
    },
    formatDueDate(dateString: string) {
      const date = new Date(dateString);
      const today = new Date();
      const diffTime = date.getTime() - today.getTime();
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

      if (diffDays === 0) return 'Today';
      if (diffDays === 1) return 'Tomorrow';
      if (diffDays === -1) return 'Yesterday';
      if (diffDays < 0) return `${Math.abs(diffDays)}d overdue`;
      if (diffDays < 7) return `${diffDays}d`;
      
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    },
    onAddChildTask() {
      console.log('Add Task Clicked for:', this.workItem);
      this.$emit('add-child-task', this.workItem);
    }
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