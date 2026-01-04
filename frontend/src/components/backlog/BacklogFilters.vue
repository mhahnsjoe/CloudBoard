<template>
  <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-4 mb-4">
    <div class="flex flex-wrap items-center gap-4">
      <!-- Search -->
      <div class="relative flex-1 min-w-[200px] max-w-md">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <SearchIcon className="w-4 h-4 text-gray-400" />
        </div>
        <input
          :value="searchQuery"
          @input="$emit('update:searchQuery', ($event.target as HTMLInputElement).value)"
          type="text"
          placeholder="Search by title, description, or #ID..."
          class="w-full pl-9 pr-4 py-2 text-sm border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
        />
      </div>

      <!-- Type Filter -->
      <div class="relative" ref="typeDropdownRef">
        <button
          @click="typeDropdownOpen = !typeDropdownOpen"
          class="flex items-center gap-2 px-3 py-2 text-sm border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          :class="{ 'bg-blue-50 border-blue-300': selectedTypes.length > 0 }"
        >
          <span>Type</span>
          <span v-if="selectedTypes.length > 0" class="bg-blue-600 text-white text-xs px-1.5 py-0.5 rounded-full">
            {{ selectedTypes.length }}
          </span>
          <svg class="w-4 h-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
          </svg>
        </button>

        <div
          v-if="typeDropdownOpen"
          class="absolute top-full left-0 mt-1 w-48 bg-white border border-gray-200 rounded-lg shadow-lg z-50"
        >
          <div class="p-2 space-y-1">
            <label
              v-for="type in workItemTypes"
              :key="type"
              class="flex items-center gap-2 px-2 py-1.5 hover:bg-gray-50 rounded cursor-pointer"
            >
              <input
                type="checkbox"
                :checked="selectedTypes.includes(type)"
                @change="toggleType(type)"
                class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
              />
              <WorkItemTypeBadge :type="type" />
            </label>
          </div>
        </div>
      </div>

      <!-- Status Filter -->
      <div class="relative" ref="statusDropdownRef">
        <button
          @click="statusDropdownOpen = !statusDropdownOpen"
          class="flex items-center gap-2 px-3 py-2 text-sm border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          :class="{ 'bg-blue-50 border-blue-300': selectedStatuses.length > 0 }"
        >
          <span>Status</span>
          <span v-if="selectedStatuses.length > 0" class="bg-blue-600 text-white text-xs px-1.5 py-0.5 rounded-full">
            {{ selectedStatuses.length }}
          </span>
          <svg class="w-4 h-4 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
          </svg>
        </button>

        <div
          v-if="statusDropdownOpen"
          class="absolute top-full left-0 mt-1 w-40 bg-white border border-gray-200 rounded-lg shadow-lg z-50"
        >
          <div class="p-2 space-y-1">
            <label
              v-for="status in statuses"
              :key="status"
              class="flex items-center gap-2 px-2 py-1.5 hover:bg-gray-50 rounded cursor-pointer"
            >
              <input
                type="checkbox"
                :checked="selectedStatuses.includes(status)"
                @change="toggleStatus(status)"
                class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
              />
              <span class="text-sm">{{ status }}</span>
            </label>
          </div>
        </div>
      </div>

      <!-- Expand/Collapse All -->
      <div class="flex items-center gap-2 border-l border-gray-200 pl-4">
        <button
          @click="$emit('expand-all')"
          class="px-3 py-2 text-sm text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
          title="Expand all"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4"/>
          </svg>
        </button>
        <button
          @click="$emit('collapse-all')"
          class="px-3 py-2 text-sm text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
          title="Collapse all"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 9V4.5M9 9H4.5M9 9L3.75 3.75M9 15v4.5M9 15H4.5M9 15l-5.25 5.25M15 9h4.5M15 9V4.5M15 9l5.25-5.25M15 15h4.5M15 15v4.5m0-4.5l5.25 5.25"/>
          </svg>
        </button>
      </div>

      <!-- Clear Filters -->
      <button
        v-if="hasActiveFilters"
        @click="$emit('clear-filters')"
        class="px-3 py-2 text-sm text-red-600 hover:bg-red-50 rounded-lg transition-colors"
      >
        Clear filters
      </button>

      <!-- Item Count -->
      <div class="ml-auto text-sm text-gray-500">
        {{ filteredCount }} of {{ totalCount }} items
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, type PropType, onMounted, onBeforeUnmount } from 'vue'
import type { WorkItemType } from '@/types/WorkItem'
import { WORKITEM_TYPES } from '@/types/WorkItem'
import { STATUSES } from '@/types/Project'
import WorkItemTypeBadge from '@/components/workItem/WorkItemTypeBadge.vue'
import { SearchIcon } from '@/components/icons'

export default defineComponent({
  name: 'BacklogFilters',
  components: {
    WorkItemTypeBadge,
    SearchIcon
  },
  props: {
    searchQuery: {
      type: String,
      default: ''
    },
    selectedTypes: {
      type: Array as PropType<WorkItemType[]>,
      default: () => []
    },
    selectedStatuses: {
      type: Array as PropType<string[]>,
      default: () => []
    },
    filteredCount: {
      type: Number,
      required: true
    },
    totalCount: {
      type: Number,
      required: true
    }
  },
  emits: [
    'update:searchQuery',
    'update:selectedTypes', 
    'update:selectedStatuses',
    'expand-all',
    'collapse-all',
    'clear-filters'
  ],
  setup(props, { emit }) {
    const typeDropdownOpen = ref(false)
    const statusDropdownOpen = ref(false)
    const typeDropdownRef = ref<HTMLElement | null>(null)
    const statusDropdownRef = ref<HTMLElement | null>(null)

    const workItemTypes = WORKITEM_TYPES
    const statuses = STATUSES

    const hasActiveFilters = computed(() => {
      return props.searchQuery.length > 0 || 
             props.selectedTypes.length > 0 || 
             props.selectedStatuses.length > 0
    })

    const toggleType = (type: WorkItemType) => {
      const current = [...props.selectedTypes]
      const index = current.indexOf(type)
      if (index >= 0) {
        current.splice(index, 1)
      } else {
        current.push(type)
      }
      emit('update:selectedTypes', current)
    }

    const toggleStatus = (status: string) => {
      const current = [...props.selectedStatuses]
      const index = current.indexOf(status)
      if (index >= 0) {
        current.splice(index, 1)
      } else {
        current.push(status)
      }
      emit('update:selectedStatuses', current)
    }

    // Close dropdowns on click outside
    const handleClickOutside = (event: MouseEvent) => {
      if (typeDropdownRef.value && !typeDropdownRef.value.contains(event.target as Node)) {
        typeDropdownOpen.value = false
      }
      if (statusDropdownRef.value && !statusDropdownRef.value.contains(event.target as Node)) {
        statusDropdownOpen.value = false
      }
    }

    onMounted(() => {
      document.addEventListener('click', handleClickOutside)
    })

    onBeforeUnmount(() => {
      document.removeEventListener('click', handleClickOutside)
    })

    return {
      typeDropdownOpen,
      statusDropdownOpen,
      typeDropdownRef,
      statusDropdownRef,
      workItemTypes,
      statuses,
      hasActiveFilters,
      toggleType,
      toggleStatus
    }
  }
})
</script>