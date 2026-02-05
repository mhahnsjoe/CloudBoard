<template>
  <div class="sprint-analytics p-6">
    <!-- Headers removed as they are provided by SprintBoardView/InfoBar -->

    <!-- Stats Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
      <div class="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
        <div class="text-sm text-gray-500 mb-1">Items Completed</div>
        <div class="text-3xl font-bold text-gray-900">
          {{ sprint?.completedWorkItems }} / {{ sprint?.totalWorkItems }}
        </div>
        <div class="mt-2 h-1.5 w-full bg-gray-100 rounded-full overflow-hidden">
          <div 
            class="h-full bg-green-500" 
            :style="{ width: `${sprint?.progressPercentage || 0}%` }"
          ></div>
        </div>
      </div>

      <div class="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
        <div class="text-sm text-gray-500 mb-1">Hours Completed</div>
        <div class="text-3xl font-bold text-gray-900">
          {{ sprint?.completedEstimatedHours }}h / {{ sprint?.totalEstimatedHours }}h
        </div>
        <div class="text-xs text-gray-400 mt-2">
          {{ (((sprint?.completedEstimatedHours || 0) / (sprint?.totalEstimatedHours || 1)) * 100).toFixed(0) }}% efficiency
        </div>
      </div>

      <div class="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
        <div class="text-sm text-gray-500 mb-1">Capacity Utilization</div>
        <div class="text-3xl font-bold text-gray-900">
          {{ sprint?.capacityUtilization?.toFixed(0) || 0 }}%
        </div>
        <div class="text-xs mt-2" :class="(sprint?.capacityUtilization || 0) > 100 ? 'text-red-500' : 'text-gray-400'">
          {{ sprint?.capacityHours || 0 }}h total capacity
        </div>
      </div>

      <div class="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
        <div class="text-sm text-gray-500 mb-1">Time Remaining</div>
        <div class="text-3xl font-bold text-gray-900">
          {{ (sprint?.daysRemaining || 0) < 0 
              ? `Ended ${Math.abs(sprint?.daysRemaining || 0)} days ago` 
              : `${sprint?.daysRemaining} Days` 
          }}
        </div>
        <div class="text-xs text-gray-400 mt-2">
          Until {{ sprint ? formatDate(sprint.endDate) : '' }}
        </div>
      </div>
    </div>

    <!-- Charts Row -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-8">
      <BurndownChart v-if="burndownData.length" :data="burndownData" />
      <VelocityChart v-if="velocityData" :velocities="velocityData.sprintVelocities" />
    </div>

    <!-- Bottom Row: Retrospective & Summary -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <div class="lg:col-span-2">
        <SprintRetrospective 
          v-if="sprint" 
          :sprint="sprint" 
          @update="$emit('update-sprint', $event)"
        />
      </div>
      
      <div class="bg-white rounded-lg shadow-sm p-6 border border-gray-100">
        <h3 class="text-lg font-semibold mb-4 text-gray-900">Sprint Summary</h3>
        <ul class="space-y-4">
          <li class="flex justify-between items-center text-sm">
            <span class="text-gray-500">Success Rate</span>
            <span class="font-medium" :class="sprint && sprint.progressPercentage >= 80 ? 'text-green-600' : 'text-yellow-600'">
              {{ sprint?.progressPercentage }}%
            </span>
          </li>
          <li class="flex justify-between items-center text-sm">
            <span class="text-gray-500">Planned Hours</span>
            <span class="text-gray-900 font-medium">{{ sprint?.totalEstimatedHours }}h</span>
          </li>
          <li class="flex justify-between items-center text-sm">
            <span class="text-gray-500">Completed Hours</span>
            <span class="text-gray-900 font-medium">{{ sprint?.completedEstimatedHours }}h</span>
          </li>
          <li class="flex justify-between items-center text-sm border-t pt-4">
            <span class="text-gray-500">Carry Over</span>
            <span class="text-red-600 font-medium">
              {{ (sprint?.totalWorkItems || 0) - (sprint?.completedWorkItems || 0) }} Items
            </span>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { 
  getSprintBurndown, 
  getBoardVelocity 
} from '@/services/api'
import type { Sprint, BurndownPoint, BoardVelocity } from '@/types/Sprint'
import { formatDate } from '@/utils/dates'
import BurndownChart from '@/components/sprint/BurndownChart.vue'
import VelocityChart from '@/components/sprint/VelocityChart.vue'
import SprintRetrospective from '@/components/sprint/SprintRetrospective.vue'

interface Props {
  sprint: Sprint | null
  boardId: number
}

const props = defineProps<Props>()
defineEmits<{
  'update-sprint': [sprint: Sprint]
}>()

const burndownData = ref<BurndownPoint[]>([])
const velocityData = ref<BoardVelocity | null>(null)
const loading = ref(true)

const fetchData = async () => {
  if (!props.sprint || !props.boardId) return
  
  loading.value = true
  try {
    const [burndownRes, velocityRes] = await Promise.all([
      getSprintBurndown(props.sprint.id),
      getBoardVelocity(props.boardId)
    ])
    
    burndownData.value = burndownRes.data
    velocityData.value = velocityRes.data
  } catch (error) {
    console.error('Failed to fetch analytics data:', error)
  } finally {
    loading.value = false
  }
}

// Watch for sprint changes to refetch data
watch(() => props.sprint?.id, (newId) => {
  if (newId) fetchData()
}, { immediate: true })

</script>
