<template>
  <div
    :class="[sizeClasses, 'rounded-full flex items-center justify-center font-semibold', colorClass]"
    :title="name"
  >
    {{ initials }}
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  name: string
  size?: 'xs' | 'sm' | 'md' | 'lg'
  userId?: number // Used to generate consistent color
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  userId: 0
})

const initials = computed(() => {
  if (!props.name) return '?'
  const parts = props.name.trim().split(/\s+/)
  if (parts.length === 0 || !parts[0]) return '?'
  
  if (parts.length === 1) {
    return parts[0].substring(0, 2).toUpperCase()
  }
  
  const first = parts[0]!
  const last = parts[parts.length - 1]!
  return (first.charAt(0) + last.charAt(0)).toUpperCase()
})

const sizeClasses = computed(() => {
  switch (props.size) {
    case 'xs': return 'w-5 h-5 text-[10px]'
    case 'sm': return 'w-6 h-6 text-xs'
    case 'md': return 'w-8 h-8 text-sm'
    case 'lg': return 'w-10 h-10 text-base'
    default: return 'w-8 h-8 text-sm'
  }
})

// Generate consistent color based on userId or name
const colorClass = computed(() => {
  const colors = [
    'bg-blue-500 text-white',
    'bg-green-500 text-white',
    'bg-purple-500 text-white',
    'bg-orange-500 text-white',
    'bg-pink-500 text-white',
    'bg-teal-500 text-white',
    'bg-indigo-500 text-white',
    'bg-red-500 text-white'
  ]
  const hash = props.userId || props.name.split('').reduce((a, b) => a + b.charCodeAt(0), 0)
  return colors[hash % colors.length]
})
</script>
