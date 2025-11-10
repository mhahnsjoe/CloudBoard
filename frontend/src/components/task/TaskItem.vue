<template>
  <li class="task-item">
    <div class="task-info">
      <h3 class="text-lg font-semibold text-gray-800">{{ task.title }}</h3>
      <div class="flex items-center gap-2 mt-2">
        <span class="text-sm text-gray-600">Status:</span>
        <select
          :value="task.status"
          @change="$emit('statusChange', task, ($event.target as HTMLSelectElement).value)"
          :class="getStatusClass(task.status)"
          class="px-3 py-1 rounded-md font-medium text-sm border-0 cursor-pointer transition-all focus:ring-2 focus:ring-offset-1"
        >
          <option v-for="s in statusOptions" :key="s">{{ s }}</option>
        </select>
      </div>
    </div>

    <div class="actions">
      <button
        class="px-4 py-2 bg-amber-50 text-amber-700 rounded-lg hover:bg-amber-100 transition-all font-medium flex items-center gap-2"
        @click="$emit('edit', task)"
      >
        <EditIcon />
        Edit
      </button>

      <button
        @click="$emit('delete', task.id)"
        class="px-4 py-2 bg-red-50 text-red-700 rounded-lg hover:bg-red-100 transition-all font-medium flex items-center gap-2"
      >
        <DeleteIcon />
        Delete
      </button>
    </div>
  </li>
</template>

<script lang="ts">
import { defineComponent, PropType } from 'vue';
import type { TaskItem as TaskItemType } from '@/types/Project';
import { EditIcon, DeleteIcon } from '@/components/icons';

export default defineComponent({
  name: 'TaskItem',
  components: { EditIcon, DeleteIcon },
  props: {
    task: {
      type: Object as PropType<TaskItemType>,
      required: true
    },
    statusOptions: {
      type: Array as PropType<string[]>,
      required: true
    }
  },
  emits: ['edit', 'delete', 'statusChange'],
  methods: {
    getStatusClass(status: string) {
      switch (status) {
        case "To Do":
          return "bg-gray-100 text-gray-700 focus:ring-gray-500";
        case "In Progress":
          return "bg-blue-100 text-blue-700 focus:ring-blue-500";
        case "Done":
          return "bg-green-100 text-green-700 focus:ring-green-500";
        default:
          return "bg-gray-100 text-gray-700 focus:ring-gray-500";
      }
    }
  }
});
</script>

<style scoped>
.task-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: white;
  padding: 1.5rem;
  border-radius: 12px;
  margin-bottom: 1rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  transition: box-shadow 0.2s;
}

.task-item:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.task-info {
  flex: 1;
}

.actions {
  display: flex;
  gap: 0.5rem;
}
</style>
