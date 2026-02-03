<template>
  <div class="bg-white rounded-lg shadow-sm p-6">
    <div class="flex justify-between items-center mb-4">
      <h3 class="text-lg font-semibold text-gray-900">Sprint Burndown</h3>
      <div class="flex gap-4 text-sm">
        <span class="flex items-center gap-1.5">
          <span class="w-3 h-3 bg-blue-500 rounded-full"></span>
          <span class="text-gray-600">Ideal</span>
        </span>
        <span class="flex items-center gap-1.5">
          <span class="w-3 h-3 bg-green-500 rounded-full"></span>
          <span class="text-gray-600">Actual</span>
        </span>
      </div>
    </div>
    <div class="h-64">
      <Line :data="chartData" :options="chartOptions" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
} from 'chart.js'
import { Line } from 'vue-chartjs'
import type { BurndownPoint } from '@/types/Sprint'
import { formatShortDate } from '@/utils/dates'

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
)

interface Props {
  data: BurndownPoint[]
}

const props = defineProps<Props>()

const chartData = computed(() => ({
  labels: props.data.map(p => formatShortDate(p.date)),
  datasets: [
    {
      label: 'Actual Remaining',
      backgroundColor: 'rgba(34, 197, 94, 0.1)',
      borderColor: '#22c55e',
      pointBackgroundColor: '#22c55e',
      pointBorderColor: '#fff',
      data: props.data.map(p => p.remainingHours),
      fill: true,
      tension: 0.1
    },
    {
      label: 'Ideal Burn',
      borderColor: '#3b82f6',
      borderDash: [5, 5],
      pointRadius: 0,
      data: props.data.map(p => p.idealRemainingHours),
      fill: false,
      tension: 0
    }
  ]
}))

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false
    },
    tooltip: {
      mode: 'index' as const,
      intersect: false
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
