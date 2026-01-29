<template>
  <div 
    class="flex-1 bg-gray-50 rounded-lg border border-gray-200 flex flex-col"
    :class="{ 'ring-2 ring-blue-400': isDragOver }"
  >
    <!-- Header -->
    <div class="p-4 border-b border-gray-200 bg-white rounded-t-lg">
      <div class="flex items-center justify-between">
        <div>
          <h3 class="font-semibold text-gray-900">{{ title }}</h3>
          <p class="text-sm text-gray-500">
            {{ items.length }} items • {{ totalHours }}h
          </p>
        </div>
        <slot name="header-actions"></slot>
      </div>
      
      <!-- Capacity Bar (for sprint column) -->
      <SprintCapacityBar 
        v-if="capacity"
        :used="totalHours"
        :total="capacity.totalCapacityHours"
        class="mt-3"
      />
    </div>

    <!-- Items List -->
    <div 
      class="flex-1 p-3 space-y-2 overflow-y-auto min-h-[400px]"
      @drop="onDrop"
      @dragover.prevent="onDragOver"
      @dragleave="onDragLeave"
    >
      <SprintPlanningCard
        v-for="item in items"
        :key="item.id"
        :item="item"
        @dragstart="(item, e) => emit('item-dragstart', item, e)"
      />
      
      <!-- Empty State -->
      <div 
        v-if="items.length === 0" 
        class="flex items-center justify-center h-32 text-gray-400 text-sm border-2 border-dashed border-gray-300 rounded-lg"
      >
        {{ emptyMessage }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import type { WorkItemSummary, SprintCapacity } from '@/types/Sprint'
import SprintPlanningCard from './SprintPlanningCard.vue'
import SprintCapacityBar from './SprintCapacityBar.vue'

interface Props {
  title: string
  items: WorkItemSummary[]
  capacity?: SprintCapacity
  emptyMessage?: string
}

const props = withDefaults(defineProps<Props>(), {
  emptyMessage: 'Drop items here'
})

const emit = defineEmits<{
  'item-drop': [item: WorkItemSummary]
  'item-dragstart': [item: WorkItemSummary, event: DragEvent]
}>()

const isDragOver = ref(false)

const totalHours = computed(() => 
  props.items.reduce((sum, item) => sum + (item.estimatedHours || 0), 0)
)

const onDrop = (event: DragEvent) => {
  isDragOver.value = false
  const data = event.dataTransfer?.getData('application/json')
  if (data) {
    const item = JSON.parse(data) as WorkItemSummary
    emit('item-drop', item)
  }
}

const onDragOver = () => {
  isDragOver.value = true
}

const onDragLeave = () => {
  isDragOver.value = false
}
</script>
