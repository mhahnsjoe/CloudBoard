<template>
  <div class="relative" ref="dropdownRef">
    <!-- Selected Value / Trigger -->
    <button
      type="button"
      @click="toggleDropdown"
      class="w-full flex items-center justify-between px-3 py-2 border border-gray-300 rounded-lg bg-white hover:border-gray-400 transition-colors text-left"
      :class="{ 'ring-2 ring-blue-500 border-transparent': isOpen }"
    >
      <span v-if="selectedParent" class="flex items-center gap-2 truncate">
        <WorkItemTypeBadge :type="selectedParent.type" />
        <span class="text-sm text-gray-700 truncate">#{{ selectedParent.id }} - {{ selectedParent.title }}</span>
      </span>
      <span v-else class="text-gray-500 text-sm">No parent (top-level item)</span>
      
      <svg class="w-4 h-4 text-gray-400 flex-shrink-0 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
      </svg>
    </button>

    <!-- Dropdown -->
    <div
      v-if="isOpen"
      class="absolute top-full left-0 right-0 mt-1 bg-white border border-gray-200 rounded-lg shadow-xl z-50 max-h-80 flex flex-col"
    >
      <!-- Search Input -->
      <div class="p-2 border-b border-gray-200">
        <div class="relative">
          <SearchIcon className="w-4 h-4 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            ref="searchInputRef"
            v-model="searchQuery"
            type="text"
            placeholder="Search by title or #ID..."
            class="w-full pl-9 pr-3 py-2 text-sm border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
            @keydown.escape="closeDropdown"
          />
        </div>
      </div>

      <!-- Options List -->
      <div class="overflow-y-auto flex-1">
        <!-- No Parent Option -->
        <button
          type="button"
          @click="selectParent(null)"
          class="w-full flex items-center gap-2 px-3 py-2.5 hover:bg-gray-50 transition-colors text-left border-b border-gray-100"
          :class="{ 'bg-blue-50': modelValue === null }"
        >
          <span class="w-5 h-5 flex items-center justify-center text-gray-400">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"/>
            </svg>
          </span>
          <span class="text-sm text-gray-600">No parent (top-level item)</span>
        </button>

        <!-- Parent Options -->
        <button
          v-for="parent in filteredParents"
          :key="parent.id"
          type="button"
          @click="selectParent(parent.id)"
          class="w-full flex items-center gap-2 px-3 py-2.5 hover:bg-gray-50 transition-colors text-left"
          :class="{ 'bg-blue-50': modelValue === parent.id }"
        >
          <WorkItemTypeBadge :type="parent.type" />
          <div class="flex-1 min-w-0">
            <div class="text-sm font-medium text-gray-800 truncate">
              {{ parent.title }}
            </div>
            <div class="text-xs text-gray-500">
              #{{ parent.id }}
            </div>
          </div>
          <svg v-if="modelValue === parent.id" class="w-4 h-4 text-blue-600 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
          </svg>
        </button>

        <!-- Empty State -->
        <div v-if="filteredParents.length === 0 && searchQuery" class="px-3 py-4 text-center text-sm text-gray-500">
          No matching items found
        </div>
        <div v-else-if="parents.length === 0" class="px-3 py-4 text-center text-sm text-gray-500">
          No valid parents available for this type
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, watch, nextTick, type PropType, onMounted, onBeforeUnmount } from 'vue'
import type { WorkItem } from '@/types/WorkItem'
import WorkItemTypeBadge from './WorkItemTypeBadge.vue'
import { SearchIcon } from '@/components/icons'

export default defineComponent({
  name: 'ParentSelector',
  components: {
    WorkItemTypeBadge,
    SearchIcon
  },
  props: {
    modelValue: {
      type: [Number, null] as PropType<number | null>,
      default: null
    },
    parents: {
      type: Array as PropType<WorkItem[]>,
      required: true
    }
  },
  emits: ['update:modelValue'],
  setup(props, { emit }) {
    const isOpen = ref(false)
    const searchQuery = ref('')
    const dropdownRef = ref<HTMLElement | null>(null)
    const searchInputRef = ref<HTMLInputElement | null>(null)

    const selectedParent = computed(() => {
      if (props.modelValue === null) return null
      return props.parents.find(p => p.id === props.modelValue) || null
    })

    const filteredParents = computed(() => {
      if (!searchQuery.value.trim()) {
        return props.parents
      }
      
      const query = searchQuery.value.toLowerCase()
      return props.parents.filter(parent => 
        parent.title.toLowerCase().includes(query) ||
        `#${parent.id}`.includes(query) ||
        parent.id.toString().includes(query)
      )
    })

    const toggleDropdown = () => {
      isOpen.value = !isOpen.value
      if (isOpen.value) {
        nextTick(() => {
          searchInputRef.value?.focus()
        })
      }
    }

    const closeDropdown = () => {
      isOpen.value = false
      searchQuery.value = ''
    }

    const selectParent = (parentId: number | null) => {
      emit('update:modelValue', parentId)
      closeDropdown()
    }

    // Close on click outside
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.value && !dropdownRef.value.contains(event.target as Node)) {
        closeDropdown()
      }
    }

    onMounted(() => {
      document.addEventListener('click', handleClickOutside)
    })

    onBeforeUnmount(() => {
      document.removeEventListener('click', handleClickOutside)
    })

    // Reset search when dropdown closes
    watch(isOpen, (newVal) => {
      if (!newVal) {
        searchQuery.value = ''
      }
    })

    return {
      isOpen,
      searchQuery,
      dropdownRef,
      searchInputRef,
      selectedParent,
      filteredParents,
      toggleDropdown,
      closeDropdown,
      selectParent
    }
  }
})
</script>