<template>
  <div
    class="bg-white border border-gray-200 rounded-lg overflow-hidden hover:shadow-md transition-shadow"
  >
    <!-- Parent Item -->
    <div
      class="p-3 cursor-grab active:cursor-grabbing"
      draggable="true"
      @dragstart="onDragStart"
      @dragend="onDragEnd"
    >
      <div class="flex items-start justify-between gap-2">
        <div class="flex-1 min-w-0">
          <div class="flex items-center gap-2 mb-1">
            <!-- Expand/Collapse Icon -->
            <button
              v-if="item.childCount > 0"
              @click.stop="toggleExpanded"
              class="flex-shrink-0 text-gray-400 hover:text-gray-600 transition-transform"
            >
              <svg
                class="w-4 h-4"
                :class="{ 'rotate-90': isExpanded }"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>
            <WorkItemTypeBadge :type="item.type" size="sm" />
            <span class="text-sm font-medium text-gray-900 truncate">
              {{ item.title }}
            </span>
          </div>
          
          <div class="flex items-center gap-3 text-xs text-gray-500">
            <span v-if="item.parentTitle" class="truncate max-w-[120px]" :title="item.parentTitle">
              ↳ {{ item.parentTitle }}
            </span>
            <span v-if="item.childCount > 0" class="flex items-center gap-1 text-blue-600 font-medium">
              <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
              </svg>
              {{ item.childCount }}
            </span>
            <span :class="getPriorityClass(item.priority)">
              {{ item.priority }}
            </span>
          </div>
        </div>
        
        <div class="flex flex-col items-end gap-1">
          <span 
            v-if="item.estimatedHours" 
            class="text-xs font-medium px-2 py-0.5 bg-blue-100 text-blue-700 rounded"
            :title="item.childCount > 0 ? 'Total including children' : ''"
          >
            {{ item.estimatedHours }}h
          </span>
          <span 
            v-else 
            class="text-xs text-gray-400"
          >
            No estimate
          </span>
        </div>
      </div>
    </div>

    <!-- Children (Expanded) -->
    <div
      v-if="isExpanded && item.children && item.children.length > 0"
      class="border-t border-gray-200 bg-gray-50 px-3 py-2 space-y-1"
    >
      <div
        v-for="child in item.children"
        :key="child.id"
        class="flex items-center justify-between text-xs py-1.5 px-2 bg-white rounded border border-gray-100 hover:border-gray-300 transition-colors"
      >
        <div class="flex items-center gap-2 flex-1 min-w-0">
          <WorkItemTypeBadge :type="child.type" size="xs" />
          <span class="truncate text-gray-700">{{ child.title }}</span>
        </div>
        <span v-if="child.estimatedHours" class="text-gray-500 ml-2 font-medium">
          {{ child.estimatedHours }}h
        </span>
        <span v-else class="text-gray-400 ml-2 text-xs">
          -
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { WorkItemSummary } from '@/types/Sprint'
import WorkItemTypeBadge from '@/components/workItem/WorkItemTypeBadge.vue'
import { getPriorityClass } from '@/utils/badges'

interface Props {
  item: WorkItemSummary
}

const props = defineProps<Props>()

const emit = defineEmits<{
  dragstart: [item: WorkItemSummary, event: DragEvent]
  dragend: [event: DragEvent]
}>()

const isExpanded = ref(false)

const toggleExpanded = () => {
  isExpanded.value = !isExpanded.value
}

const onDragStart = (event: DragEvent) => {
  event.dataTransfer?.setData('application/json', JSON.stringify(props.item))
  emit('dragstart', props.item, event)
}

const onDragEnd = (event: DragEvent) => {
  emit('dragend', event)
}
</script>
