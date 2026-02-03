<template>
  <div class="capacity-bar">
    <div class="flex justify-between items-center text-sm mb-1">
      <span class="text-gray-600">
        {{ used }}h / {{ total }}h allocated
      </span>
      <span :class="percentageClass" class="font-medium">
        {{ percentUsed.toFixed(0) }}%
      </span>
    </div>
    
    <div class="h-2 bg-gray-200 rounded-full overflow-hidden">
      <div 
        class="h-full transition-all duration-300 rounded-full"
        :class="barClass"
        :style="{ width: `${Math.min(percentUsed, 100)}%` }"
      />
    </div>
    
    <div v-if="isOverCapacity" class="text-xs text-red-600 mt-1">
      ⚠️ Over capacity by {{ (used - total).toFixed(1) }}h
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  used: number
  total: number
}

const props = defineProps<Props>()

const percentUsed = computed(() => 
  props.total > 0 ? (props.used / props.total) * 100 : 0
)

const isOverCapacity = computed(() => props.used > props.total)

const barClass = computed(() => {
  if (percentUsed.value > 100) return 'bg-red-500'
  if (percentUsed.value > 90) return 'bg-yellow-500'
  if (percentUsed.value > 70) return 'bg-blue-500'
  return 'bg-green-500'
})

const percentageClass = computed(() => {
  if (percentUsed.value > 100) return 'text-red-600'
  if (percentUsed.value > 90) return 'text-yellow-600'
  return 'text-gray-600'
})
</script>
