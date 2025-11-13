<template>
  <span :class="getBadgeClass()" class="inline-flex items-center gap-1 px-2 py-1 text-xs font-medium rounded">
    <!-- Task Icon -->
    <svg v-if="type === 'Task'" class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/>
    </svg>
    
    <!-- Bug Icon -->
    <svg v-else-if="type === 'Bug'" class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/>
    </svg>

    <svg v-else-if="type === 'PBI'" class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z"/>
    </svg>
    
    <!-- Feature Icon -->
    <svg v-else-if="type === 'Feature'" class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z"/>
    </svg>
    
    <!-- Epic Icon -->
    <svg v-else-if="type === 'Epic'" class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zm0 0h12a2 2 0 002-2v-4a2 2 0 00-2-2h-2.343M11 7.343l1.657-1.657a2 2 0 012.828 0l2.829 2.829a2 2 0 010 2.828l-8.486 8.485M7 17h.01"/>
    </svg>
    
    {{ type }}
  </span>
</template>

<script lang="ts">
import { defineComponent, type PropType } from 'vue';

export default defineComponent({
  name: 'WorkItemTypeBadge',
  props: {
    type: {
      type: String as PropType<'Task' | 'Bug' | 'PBI' |'Feature' | 'Epic'>,
      required: true
    }
  },
  methods: {
    getBadgeClass() {
      const classes: Record<string, string> = {
        'Task': 'bg-yellow-100 text-yellow-700',
        'Bug': 'bg-red-100 text-red-700',
        'PBI': 'bg-blue-100 text-blue-700',
        'Feature': 'bg-purple-100 text-purple-700',
        'Epic': 'bg-orange-100 text-orange-700'
      };
      return classes[this.type] || 'bg-gray-100 text-gray-700';
    }
  }
});
</script>