<template>
  <div class="container mx-auto px-4 py-6">
    <!-- Back button -->
    <button
      @click="router.push({ name: 'Teams' })"
      class="mb-4 flex items-center text-gray-600 hover:text-gray-900"
    >
      <ArrowLeftIcon className="w-4 h-4 mr-2" />
      Back to Teams
    </button>

    <div v-if="teamsStore.loading" class="text-center py-12">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
      <p class="mt-2 text-gray-500">Loading team...</p>
    </div>

    <div v-else-if="teamsStore.error" class="text-center py-12">
      <p class="text-red-600">{{ teamsStore.error }}</p>
      <router-link
        :to="{ name: 'Teams' }"
        class="mt-4 inline-block text-blue-600 hover:underline"
      >
        Go to Teams
      </router-link>
    </div>

    <div v-else-if="team">
      <TeamSettings
        :team="team"
        @refresh="fetchTeam"
        @leave="handleLeave"
      />
    </div>

    <div v-else class="text-center py-12">
      <p class="text-gray-500">Team not found</p>
      <router-link
        :to="{ name: 'Teams' }"
        class="mt-4 inline-block text-blue-600 hover:underline"
      >
        Go to Teams
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTeamsStore } from '@/stores/teams'
import TeamSettings from '@/components/team/TeamSettings.vue'
import { ArrowLeftIcon } from '@/components/icons'

const route = useRoute()
const router = useRouter()
const teamsStore = useTeamsStore()

const teamId = computed(() => Number(route.params.id))
const team = computed(() => teamsStore.currentTeam)

onMounted(() => {
  fetchTeam()
})

async function fetchTeam() {
  try {
    await teamsStore.fetchTeam(teamId.value)
  } catch (e) {
    console.error('Failed to fetch team:', e)
  }
}

function handleLeave() {
  router.push({ name: 'Teams' })
}
</script>
