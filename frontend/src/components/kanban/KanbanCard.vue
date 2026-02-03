<template> <!--TODO: RENAME TO WORKITEM CARD? BoardCanvas use this for both sprint & kanban-->
  <div 
    class="bg-white rounded-lg shadow-sm border border-gray-200 border-l-[3px] overflow-hidden hover:shadow-md transition-all"
    :class="getBorderClass(workItem.type)"
  >
    <!-- Parent Item -->
    <div
      class="p-3 cursor-move group relative"
      draggable="true"
      @dragstart="$emit('dragstart', workItem)"
    >
      <!-- Header: Type, ID, Title, Priority -->
      <div class="flex items-center justify-between gap-2 mb-2">
        <div class="flex items-center gap-2 min-w-0">
          <WorkItemTypeBadge :type="workItem.type as WorkItemType" :iconOnly="true" class="flex-shrink-0" />
          <span class="text-sm font-bold text-gray-900 flex-shrink-0">{{ workItem.id }}</span>
          <button
            type="button"
            @click.stop="$emit('view-details', workItem.id)"
            class="text-sm font-normal text-gray-900 hover:text-blue-600 truncate text-left leading-tight min-w-0"
            :title="workItem.title"
          >
            {{ workItem.title }}
          </button>
        </div>
        
        <span :class="getPriorityClass(workItem.priority)" class="px-1.5 py-0.5 rounded-[3px] text-[10px] font-bold uppercase tracking-wider flex-shrink-0 border border-current border-opacity-20">
          {{ workItem.priority }}
        </span>
      </div>

      <!-- WorkItem Description (if exists) -->
      <p v-if="workItem.description" class="text-xs text-gray-500 mb-3 line-clamp-2 pl-6">
        {{ workItem.description }}
      </p>

      <!-- Footer: Assignee & Meta -->
      <div class="flex items-center justify-between mt-2 pt-2 border-t border-gray-50">
        <!-- Left: Assignee -->
        <div class="flex items-center gap-1.5 text-xs text-gray-600 min-w-0">
          <div v-if="workItem.assignedToName" class="flex items-center gap-1.5 min-w-0" :title="workItem.assignedToName">
              <div class="w-5 h-5 bg-blue-100 text-blue-700 rounded-full flex items-center justify-center text-[10px] font-medium border border-blue-200 flex-shrink-0">
                {{ getInitials(workItem.assignedToName) }}
              </div>
              <span class="font-medium truncate max-w-[90px]">{{ workItem.assignedToName }}</span>
          </div>
          <div v-else class="flex items-center gap-1.5 text-gray-400 min-w-0" title="Unassigned">
              <div class="w-5 h-5 bg-gray-100 text-gray-400 rounded-full flex items-center justify-center border border-gray-200 flex-shrink-0">
                <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path></svg>
              </div>
              <span class="italic text-[10px]">Unassigned</span>
          </div>
        </div>

        <!-- Right: Stats -->
        <div class="flex items-center gap-2 text-xs text-gray-500 flex-shrink-0">
           <!-- Child Count Arrow -->
           <button 
             v-if="hasChildren || workItem.type === 'PBI' || workItem.type === 'Feature' || workItem.type === 'Bug'"
             @click.stop="toggleExpanded"
             class="flex items-center gap-1 px-1.5 py-0.5 bg-yellow-50 text-yellow-700 border border-yellow-200 rounded-[3px] hover:bg-yellow-100 transition-colors" 
             :class="{ 'bg-yellow-100 border-yellow-300 text-yellow-800': isExpanded }"
             title="Toggle Child Tasks"
           >
             <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/></svg>
             <span v-if="workItem.children && workItem.children.length > 0">{{ workItem.children.length }}</span>
             <span v-else>0</span>
             <svg class="w-2.5 h-2.5 transition-transform" :class="{ 'rotate-180': isExpanded }" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
           </button>

           <!-- Hours -->
           <div v-if="totalEstimatedHours" class="text-xs text-gray-500 font-mono" title="Total Estimated Hours">
             {{ totalEstimatedHours }}h
           </div>
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
          <WorkItemTypeBadge :type="child.type as WorkItemType" size="xs" :iconOnly="true" />
          <button 
            @click.stop="$emit('view-details', child.id)"
            class="truncate text-gray-700 hover:text-blue-600 font-medium text-left"
          >
            {{ child.title }}
          </button>
        </div>
        <div class="flex items-center gap-2">
          <span v-if="child.estimatedHours" class="text-gray-500 font-medium">
            {{ child.estimatedHours }}h
          </span>
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
import { DeleteIcon, CalendarIcon, ClockIcon } from '@/components/icons';

export default defineComponent({
  name: 'KanbanCard',
  components: {
    WorkItemTypeBadge,
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
  emits: ['dragstart', 'click', 'delete', 'return-to-backlog', 'add-child-task', 'view-details'],
  setup(props) {
    const isExpanded = ref(false)
    
    const hasChildren = computed(() => {
      return props.workItem.children && props.workItem.children.length > 0
    })
    
    const totalEstimatedHours = computed(() => {
      if (!hasChildren.value) {
        return props.workItem.estimatedHours || 0
      }
      const childHours = (props.workItem.children || []).reduce((sum: number, c: WorkItem) => sum + (c.estimatedHours || 0), 0)
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
    },
    getInitials(name: string) {
      return name ? name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase() : '';
    },
    getBorderClass(type: string) {
      const classes: Record<string, string> = {
        'Task': 'border-l-yellow-400',
        'Bug': 'border-l-red-500',
        'PBI': 'border-l-blue-500',
        'Feature': 'border-l-purple-500',
        'Epic': 'border-l-orange-500'
      };
      return classes[type] || 'border-l-gray-300';
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