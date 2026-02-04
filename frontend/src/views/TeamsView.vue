<template>
  <div class="container mx-auto px-4 py-6">
    <TeamList
      :teams="teamsStore.sortedTeams"
      :loading="teamsStore.loading"
      @create="showCreateModal = true"
      @select="navigateToTeam"
    />

    <CreateTeamModal
      v-if="showCreateModal"
      @close="showCreateModal = false"
      @created="handleCreated"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useTeamsStore } from '@/stores/teams'
import TeamList from '@/components/team/TeamList.vue'
import CreateTeamModal from '@/components/team/CreateTeamModal.vue'
import type { Team } from '@/types/Team'

const router = useRouter()
const teamsStore = useTeamsStore()
const showCreateModal = ref(false)

onMounted(() => {
  teamsStore.fetchTeams()
})

function navigateToTeam(team: Team) {
  router.push({ name: 'TeamDetail', params: { id: team.id } })
}

function handleCreated(team: Team) {
  showCreateModal.value = false
  navigateToTeam(team)
}
</script>
