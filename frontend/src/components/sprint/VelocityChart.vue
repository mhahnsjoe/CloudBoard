<template>
  <div class="bg-white rounded-lg shadow-sm p-6">
    <div class="flex justify-between items-center mb-4">
      <h3 class="text-lg font-semibold text-gray-900">Sprint Velocity</h3>
      <div class="flex gap-4 text-sm">
        <span class="flex items-center gap-1.5">
          <span class="w-3 h-3 bg-gray-300 rounded-full"></span>
          <span class="text-gray-600">Planned</span>
        </span>
        <span class="flex items-center gap-1.5">
          <span class="w-3 h-3 bg-blue-600 rounded-full"></span>
          <span class="text-gray-600">Completed</span>
        </span>
      </div>
    </div>
    <div class="h-64">
      <Bar :data="chartData" :options="chartOptions" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale
} from 'chart.js'
import { Bar } from 'vue-chartjs'
import type { SprintVelocity } from '@/types/Sprint'

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)

interface Props {
  velocities: SprintVelocity[]
}

const props = defineProps<Props>()

const chartData = computed(() => ({
  labels: props.velocities.map(v => v.sprintName),
  datasets: [
    {
      label: 'Planned Hours',
      backgroundColor: '#e5e7eb',
      data: props.velocities.map(v => v.plannedHours),
      borderRadius: 4
    },
    {
      label: 'Completed Hours',
      backgroundColor: '#3b82f6',
      data: props.velocities.map(v => v.completedHours),
      borderRadius: 4
    }
  ]
}))

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false
    }
  },
  scales: {
    y: {
      beginAtZero: true,
      title: {
        display: true,
        text: 'Hours'
      }
    },
    x: {
      grid: {
        display: false
      }
    }
  }
}
</script>
