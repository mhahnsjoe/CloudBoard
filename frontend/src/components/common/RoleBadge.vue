<template>
  <span :class="['px-2 py-0.5 text-xs font-medium rounded', roleClass]">
    {{ role }}
  </span>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { TeamRole } from '@/types/Team'

const props = defineProps<{
  role: TeamRole
}>()

const roleClass = computed(() => {
  switch (props.role) {
    case 'Owner': return 'bg-purple-100 text-purple-800'
    case 'Admin': return 'bg-blue-100 text-blue-800'
    case 'Member': return 'bg-gray-100 text-gray-800'
    default: return 'bg-gray-100 text-gray-800'
  }
})
</script>
