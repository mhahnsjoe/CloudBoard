<template>
  <div class="space-y-4">
    <div class="flex justify-between items-center">
      <h2 class="text-xl font-semibold text-gray-900">My Teams</h2>
      <button
        @click="$emit('create')"
        class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-all font-medium flex items-center gap-2"
      >
        <PlusIcon />
        New Team
      </button>
    </div>

    <div v-if="loading" class="text-center py-8">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
      <p class="mt-2 text-gray-500">Loading teams...</p>
    </div>

    <div v-else-if="teams.length === 0" class="text-center py-12 bg-gray-50 rounded-lg">
      <UsersIcon class="mx-auto h-12 w-12 text-gray-400" />
      <h3 class="mt-2 text-sm font-medium text-gray-900">No teams yet</h3>
      <p class="mt-1 text-sm text-gray-500">Create a team to start collaborating.</p>
      <button
        @click="$emit('create')"
        class="mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-all font-medium"
      >
        Create your first team
      </button>
    </div>

    <div v-else class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
      <div
        v-for="team in teams"
        :key="team.id"
        @click="$emit('select', team)"
        class="p-4 bg-white border rounded-lg hover:border-blue-500 hover:shadow-sm cursor-pointer transition-all"
      >
        <div class="flex items-start justify-between">
          <div>
            <h3 class="font-medium text-gray-900">{{ team.name }}</h3>
            <p v-if="team.description" class="mt-1 text-sm text-gray-500 line-clamp-2">
              {{ team.description }}
            </p>
          </div>
          <span
            class="px-2 py-1 text-xs font-medium rounded"
            :class="roleClass(team.currentUserRole)"
          >
            {{ team.currentUserRole }}
          </span>
        </div>
        <div class="mt-4 flex items-center gap-4 text-sm text-gray-500">
          <span class="flex items-center">
            <UsersIcon class="w-4 h-4 mr-1" />
            {{ team.memberCount }} {{ team.memberCount === 1 ? 'member' : 'members' }}
          </span>
          <span class="flex items-center">
            <FolderIcon class="w-4 h-4 mr-1" />
            {{ team.projectCount }} {{ team.projectCount === 1 ? 'project' : 'projects' }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Team, TeamRole } from '@/types/Team'
import { PlusIcon, UsersIcon, FolderIcon } from '@/components/icons'

defineProps<{
  teams: Team[]
  loading: boolean
}>()

defineEmits<{
  create: []
  select: [team: Team]
}>()

function roleClass(role: TeamRole) {
  switch (role) {
    case 'Owner': return 'bg-purple-100 text-purple-800'
    case 'Admin': return 'bg-blue-100 text-blue-800'
    default: return 'bg-gray-100 text-gray-800'
  }
}
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
