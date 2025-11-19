<template>
  <tr class="hover:bg-gray-50 transition-colors">
    <!-- ID -->
    <td class="px-6 py-4 whitespace-nowrap">
      <span class="text-sm font-mono text-gray-900">#{{ workItem.id }}</span>
    </td>

    <!-- Type Badge -->
    <td class="px-6 py-4 whitespace-nowrap">
      <WorkItemTypeBadge :type="workItem.type" />
    </td>

    <!-- Title -->
    <td class="px-6 py-4">
      <div class="text-sm font-medium text-gray-900">{{ workItem.title }}</div>
      <div v-if="workItem.description" class="text-sm text-gray-500 truncate max-w-md">
        {{ workItem.description }}
      </div>
    </td>

    <!-- Status -->
    <td class="px-6 py-4 whitespace-nowrap">
      <span
        class="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full"
        :class="getStatusClass(workItem.status)"
      >
        {{ workItem.status }}
      </span>
    </td>

    <!-- Priority -->
    <td class="px-6 py-4 whitespace-nowrap">
      <span
        class="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full"
        :class="getPriorityClass(workItem.priority)"
      >
        {{ workItem.priority }}
      </span>
    </td>

    <!-- Board -->
    <td class="px-6 py-4 whitespace-nowrap">
      <span class="text-sm text-gray-600">{{ boardName }}</span>
    </td>

    <!-- Actions -->
    <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
      <div class="flex items-center justify-end gap-2">
        <button
          @click="$emit('edit', workItem)"
          class="text-blue-600 hover:text-blue-900"
          title="Edit"
        >
          <EditIcon className="w-4 h-4" />
        </button>
        <button
          @click="$emit('delete', workItem.id, workItem.boardId)"
          class="text-red-600 hover:text-red-900"
          title="Delete"
        >
          <DeleteIcon className="w-4 h-4" />
        </button>
      </div>
    </td>
  </tr>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { WorkItem } from '@/types/WorkItem';
import WorkItemTypeBadge from '../workItem/WorkItemTypeBadge.vue';
import { EditIcon, DeleteIcon } from '@/components/icons';

export default defineComponent({
  name: 'BacklogListItem',
  components: {
    WorkItemTypeBadge,
    EditIcon,
    DeleteIcon
  },
  props: {
    workItem: {
      type: Object as PropType<WorkItem>,
      required: true
    },
    boardName: {
      type: String,
      required: true
    }
  },
  emits: ['edit', 'delete'],
  methods: {
    getStatusClass(status: string) {
      const classes: Record<string, string> = {
        'To Do': 'bg-gray-100 text-gray-800',
        'In Progress': 'bg-blue-100 text-blue-800',
        'Done': 'bg-green-100 text-green-800'
      };
      return classes[status] || 'bg-gray-100 text-gray-800';
    },
    getPriorityClass(priority: string) {
      const classes: Record<string, string> = {
        'Low': 'bg-gray-100 text-gray-600',
        'Medium': 'bg-blue-100 text-blue-700',
        'High': 'bg-orange-100 text-orange-700',
        'Critical': 'bg-red-100 text-red-700'
      };
      return classes[priority] || 'bg-gray-100 text-gray-600';
    }
  }
});
</script>