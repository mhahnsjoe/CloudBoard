<template>
  <div class="bg-white shadow-sm hover:shadow-lg rounded-2xl p-6 border border-gray-100 transition-all duration-200">
    <div class="flex justify-between items-start mb-4">
      <h2
        @click="$emit('view', project.id)"
        class="text-xl font-semibold text-gray-800 cursor-pointer hover:text-blue-600 transition"
      >
        {{ project.name }}
      </h2>
      <span class="text-sm text-gray-500 bg-blue-50 px-3 py-1 rounded-full">
        {{ project.tasks?.length ?? 0 }} tasks
      </span>
      <DropdownMenu>
          <button
            @click="$emit('edit', project)"
            class="w-full text-left px-4 py-2 hover:bg-gray-100 flex items-center gap-2 text-sm text-gray-700"
          >
            <EditIcon className="w-4 h-4" />
            Edit
          </button>
          <button
            @click="$emit('delete', project.id)"
            class="w-full text-left px-4 py-2 hover:bg-gray-100 flex items-center gap-2 text-sm text-red-600"
          >
            <DeleteIcon className="w-4 h-4" />
            Delete
          </button>
        </DropdownMenu>
    </div>

    <p v-if="project.description" class="text-gray-600 text-sm mb-4 line-clamp-2">
      {{ project.description }}
    </p>

    <div class="flex gap-2 mt-4">
      <button
        @click.stop="$emit('view', project.id)"
        class="flex-1 px-4 py-2 bg-blue-50 text-blue-700 rounded-lg hover:bg-blue-100 transition-all font-medium flex items-center justify-center gap-2"
      >
        <EyeIcon />
        List
      </button>
      <button
        @click.stop="$emit('kanban', project.id)"
        class="flex-1 px-4 py-2 bg-purple-50 text-purple-700 rounded-lg hover:bg-purple-100 transition-all font-medium flex items-center justify-center gap-2"
      >
        Board
      </button>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';
import type { Project } from '@/types/Project';
import { EyeIcon, EditIcon, DeleteIcon } from '@/components/icons';
import DropdownMenu from '@/components/common/DropdownMenu.vue';

export default defineComponent({
  name: 'ProjectCard',
  components: { EyeIcon, EditIcon, DeleteIcon, DropdownMenu },
  props: {
    project: {
      type: Object as PropType<Project>,
      required: true
    }
  },
  emits: ['view', 'edit', 'delete', 'kanban']
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