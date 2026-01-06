<template> <!--TODO: RENAME TO WORKITEM CARD? BoardCanvas use this for both sprint & kanban-->
  <div
    class="bg-white rounded-lg shadow-sm border border-gray-200 p-4 mb-3 cursor-move hover:shadow-md transition-all group"
    draggable="true"
    @dragstart="$emit('dragstart', workItem)"
    @click="$emit('click', workItem)"
  >
    <!-- WorkItem Type Badge -->
    <div class="flex items-center justify-between mb-2">
      <WorkItemTypeBadge :type="workItem.type as WorkItemType" />
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

      <!-- Time Tracking (if exists) -->
      <div v-if="workItem.estimatedHours || workItem.actualHours" class="flex items-center gap-1">
        <ClockIcon className="w-3 h-3" />
        <span>{{ workItem.actualHours || 0 }}/{{ workItem.estimatedHours || 0 }}h</span>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { WorkItem, WorkItemType } from '@/types/WorkItem';
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
    }
  },
  emits: ['dragstart', 'click', 'edit', 'delete', 'return-to-backlog'],
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