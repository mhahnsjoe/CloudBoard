<template>
  <div class="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
    <div class="flex items-center justify-between">
      <div>
        <p class="text-sm text-gray-600 mb-1">{{ title }}</p>
        <p class="text-3xl font-bold text-gray-800">{{ value }}</p>
      </div>
      <component :is="iconComponent" :class="['w-12 h-12', iconColorClass]" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { FolderIcon, ClipboardIcon, UsersIcon } from '@/components/icons'

const props = defineProps<{
  title: string
  value: number | string
  icon: 'folder' | 'clipboard' | 'task' | 'users'
  color: 'blue' | 'green' | 'purple' | 'orange'
}>()

const iconComponent = computed(() => {
  switch (props.icon) {
    case 'folder': return FolderIcon
    case 'clipboard': return ClipboardIcon
    case 'users': return UsersIcon
    case 'task': return ClipboardIcon // Using ClipboardIcon for tasks as well
    default: return ClipboardIcon
  }
})

const iconColorClass = computed(() => {
  switch (props.color) {
    case 'blue': return 'text-blue-500'
    case 'green': return 'text-green-500'
    case 'purple': return 'text-purple-500'
    case 'orange': return 'text-orange-500'
    default: return 'text-gray-500'
  }
})
</script>
