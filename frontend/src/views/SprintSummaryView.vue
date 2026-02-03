<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <div class="max-w-7xl mx-auto">
      <!-- Breadcrumbs -->
      <nav class="flex items-center text-sm text-gray-500 mb-6 gap-2">
        <router-link :to="`/projects/${projectId}/boards/${boardId}`" class="hover:text-blue-600 transition-colors">
          Sprint
        </router-link>
        <span>/</span>
        <span class="text-gray-900 font-medium">Insights</span>
      </nav>

      <!-- Header -->
      <div class="flex flex-col md:flex-row md:items-end justify-between gap-4 mb-8">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 mb-2">
            {{ sprint?.name || 'Sprint Insights' }}
          </h1>
          <div v-if="sprint" class="flex items-center gap-3">
            <span class="px-2 py-1 bg-blue-100 text-blue-700 rounded text-xs font-semibold uppercase tracking-wider">
              {{ sprint.status }}
            </span>
            <span class="text-gray-500 text-sm">
              {{ formatDateRange(sprint) }}
            </span>
          </div>
        </div>

        <div class="flex items-center gap-4">
          <!-- Navigation Tabs -->
          <div class="flex items-center bg-gray-100 rounded-lg p-1 border border-gray-200">
             <router-link
               :to="`/projects/${projectId}/boards/${boardId}`"
               class="px-4 py-1.5 text-xs font-medium rounded-md transition-all text-gray-600 hover:text-gray-900 hover:bg-gray-200/50"
             >
               Taskboard
             </router-link>
             <router-link
               :to="`/projects/${projectId}/boards/${boardId}`"
               class="px-4 py-1.5 text-xs font-medium rounded-md transition-all text-gray-600 hover:text-gray-900 hover:bg-gray-200/50"
             >
               Board
             </router-link>
             <div class="px-4 py-1.5 text-xs font-medium rounded-md transition-all bg-white shadow-sm text-blue-600">
               Analytics
             </div>
          </div>

          <SprintSelector
            :sprints="allSprints"
            :selectedSprintId="sprintId"
            @select="selectSprint"
          />
        </div>
      </div>

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
            {{ sprint?.capacityUtilization.toFixed(0) }}%
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
            @update="newSprint => sprint = newSprint"
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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { 
  getSprint, 
  getSprints, 
  getSprintBurndown, 
  getBoardVelocity 
} from '@/services/api'
import type { Sprint, BurndownPoint, BoardVelocity } from '@/types/Sprint'
import { formatDate, formatDateRange } from '@/utils/dates'
import BurndownChart from '@/components/sprint/BurndownChart.vue'
import VelocityChart from '@/components/sprint/VelocityChart.vue'
import SprintRetrospective from '@/components/sprint/SprintRetrospective.vue'
import SprintSelector from '@/components/sprint/SprintSelector.vue'

const route = useRoute()
const router = useRouter()

const projectId = computed(() => Number(route.params.projectId))
const boardId = computed(() => Number(route.params.boardId))
const sprintId = computed(() => Number(route.params.sprintId))

const sprint = ref<Sprint | null>(null)
const allSprints = ref<Sprint[]>([])
const burndownData = ref<BurndownPoint[]>([])
const velocityData = ref<BoardVelocity | null>(null)
const loading = ref(true)

const fetchData = async () => {
  loading.value = true
  try {
    const [sprintRes, sprintsRes, burndownRes, velocityRes] = await Promise.all([
      getSprint(sprintId.value),
      getSprints(boardId.value),
      getSprintBurndown(sprintId.value),
      getBoardVelocity(boardId.value)
    ])
    
    sprint.value = sprintRes.data
    allSprints.value = sprintsRes.data
    burndownData.value = burndownRes.data
    velocityData.value = velocityRes.data
  } catch (error) {
    console.error('Failed to fetch sprint summary:', error)
  } finally {
    loading.value = false
  }
}

const selectSprint = (id: number | null) => {
  if (id) {
    router.push(`/projects/${projectId.value}/boards/${boardId.value}/sprints/${id}/summary`)
  }
}

watch(() => route.params.sprintId, (newId) => {
  if (newId) fetchData()
})

onMounted(fetchData)
</script>
