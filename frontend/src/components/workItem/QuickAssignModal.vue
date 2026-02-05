<template>
  <Teleport to="body">
    <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50" @click.self="$emit('close')">
      <div class="bg-white rounded-lg shadow-xl w-80 max-h-96 overflow-hidden">
        <div class="px-4 py-3 border-b border-gray-200 flex items-center justify-between">
          <h3 class="font-semibold text-gray-900">Assign Work Item</h3>
          <button @click="$emit('close')" class="text-gray-400 hover:text-gray-600">
            <XIcon class="w-5 h-5" />
          </button>
        </div>

        <div class="p-4">
          <div class="mb-3 text-sm text-gray-600">
            <span class="font-medium">#{{ workItem.id }}</span> {{ workItem.title }}
          </div>

          <!-- Search -->
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search team members..."
            class="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none mb-3"
          />

          <!-- Members List -->
          <div class="max-h-48 overflow-y-auto space-y-1">
            <!-- Unassigned -->
            <button
              @click="assign(null)"
              class="w-full text-left px-3 py-2 rounded-lg hover:bg-gray-50 flex items-center gap-2"
              :class="{ 'bg-blue-50': !workItem.assignedToId }"
            >
              <div class="w-6 h-6 rounded-full bg-gray-200 flex items-center justify-center">
                <UserIcon class="w-4 h-4 text-gray-400" />
              </div>
              <span class="text-sm text-gray-600">Unassigned</span>
            </button>

            <button
              v-for="member in filteredMembers"
              :key="member.userId"
              @click="assign(member.userId)"
              class="w-full text-left px-3 py-2 rounded-lg hover:bg-gray-50 flex items-center gap-3"
              :class="{ 'bg-blue-50': workItem.assignedToId === member.userId }"
            >
              <UserAvatar :name="member.name" :userId="member.userId" size="sm" />
              <div class="min-w-0 flex-1">
                <div class="text-sm font-medium text-gray-900 truncate">{{ member.name }}</div>
              </div>
              <CheckIcon v-if="workItem.assignedToId === member.userId" class="w-4 h-4 text-blue-600" />
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import type { WorkItem } from '@/types/WorkItem'
import type { TeamMember } from '@/types/Team'
import UserAvatar from '@/components/common/UserAvatar.vue'
import { XIcon, UserIcon, CheckIcon } from '@/components/icons'

const props = defineProps<{
  workItem: WorkItem
  members: TeamMember[]
}>()

const emit = defineEmits<{
  close: []
  assign: [workItemId: number, assignedToId: number | null]
}>()

const searchQuery = ref('')

const filteredMembers = computed(() => {
  if (!searchQuery.value) return props.members
  const q = searchQuery.value.toLowerCase()
  return props.members.filter(m =>
    m.name.toLowerCase().includes(q) || m.email.toLowerCase().includes(q)
  )
})

function assign(userId: number | null) {
  emit('assign', props.workItem.id, userId)
}
</script>
