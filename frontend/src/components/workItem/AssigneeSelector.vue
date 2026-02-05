<template>
  <div class="relative" ref="dropdownRef">
    <!-- Trigger Button -->
    <button
      @click="toggle"
      type="button"
      class="flex items-center gap-2 px-2 py-1 rounded hover:bg-gray-100 transition-colors min-w-0"
      :title="selectedMember?.name || 'Unassigned'"
    >
      <UserAvatar
        v-if="selectedMember"
        :name="selectedMember.name"
        :userId="selectedMember.userId"
        size="sm"
      />
      <div v-else class="w-6 h-6 rounded-full bg-gray-200 flex items-center justify-center">
        <UserIcon class="w-4 h-4 text-gray-400" />
      </div>
      <span class="text-sm text-gray-700 truncate max-w-[120px]">
        {{ selectedMember?.name || 'Unassigned' }}
      </span>
    </button>

    <!-- Dropdown -->
    <Transition name="fade">
      <div
        v-if="isOpen"
        class="absolute top-full left-0 mt-1 w-64 bg-white rounded-lg shadow-lg border border-gray-200 z-50 py-1 max-h-64 overflow-y-auto"
      >
        <!-- Search -->
        <div class="px-3 py-2 border-b border-gray-100">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search team members..."
            class="w-full px-2 py-1 text-sm border border-gray-200 rounded focus:ring-1 focus:ring-blue-500 outline-none"
            ref="searchInput"
          />
        </div>

        <!-- Unassigned Option -->
        <button
          @click="selectMember(null)"
          class="w-full text-left px-3 py-2 hover:bg-gray-50 flex items-center gap-2"
          :class="{ 'bg-blue-50': !modelValue }"
        >
          <div class="w-6 h-6 rounded-full bg-gray-200 flex items-center justify-center">
            <UserIcon class="w-4 h-4 text-gray-400" />
          </div>
          <span class="text-sm text-gray-600">Unassigned</span>
        </button>

        <div class="border-t border-gray-100 my-1"></div>

        <!-- Team Members -->
        <button
          v-for="member in filteredMembers"
          :key="member.userId"
          @click="selectMember(member)"
          class="w-full text-left px-3 py-2 hover:bg-gray-50 flex items-center gap-2"
          :class="{ 'bg-blue-50': modelValue === member.userId }"
        >
          <UserAvatar :name="member.name" :userId="member.userId" size="sm" />
          <div class="min-w-0 flex-1">
            <div class="text-sm font-medium text-gray-900 truncate">{{ member.name }}</div>
            <div class="text-xs text-gray-500 truncate">{{ member.email }}</div>
          </div>
          <CheckIcon v-if="modelValue === member.userId" class="w-4 h-4 text-blue-600 flex-shrink-0" />
        </button>

        <div v-if="filteredMembers.length === 0" class="px-3 py-4 text-sm text-gray-500 text-center">
          No members found
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick } from 'vue'
import { useClickOutside } from '@/composables/useClickOutside'
import UserAvatar from '@/components/common/UserAvatar.vue'
import { UserIcon, CheckIcon } from '@/components/icons'
import type { TeamMember } from '@/types/Team'

interface Props {
  modelValue: number | null
  members: TeamMember[]
}

const props = defineProps<Props>()
const emit = defineEmits<{
  'update:modelValue': [value: number | null]
}>()

const dropdownRef = ref<HTMLElement | null>(null)
const searchInput = ref<HTMLInputElement | null>(null)
const isOpen = ref(false)
const searchQuery = ref('')

useClickOutside(dropdownRef, () => {
  isOpen.value = false
})

const selectedMember = computed(() =>
  props.members.find(m => m.userId === props.modelValue) || null
)

const filteredMembers = computed(() => {
  if (!searchQuery.value) return props.members
  const query = searchQuery.value.toLowerCase()
  return props.members.filter(m =>
    m.name.toLowerCase().includes(query) ||
    m.email.toLowerCase().includes(query)
  )
})

function toggle() {
  isOpen.value = !isOpen.value
  if (isOpen.value) {
    searchQuery.value = ''
    nextTick(() => searchInput.value?.focus())
  }
}

function selectMember(member: TeamMember | null) {
  emit('update:modelValue', member?.userId || null)
  isOpen.value = false
}
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.15s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
