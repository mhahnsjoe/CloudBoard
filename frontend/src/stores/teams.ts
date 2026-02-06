import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as api from '@/services/api'
import type { Team, TeamDetail, TeamMember, CreateTeamDto, UpdateTeamDto, InviteMemberDto, UpdateMemberRoleDto, MyInvitation } from '@/types/Team'

export const useTeamsStore = defineStore('teams', () => {
  // State
  const teams = ref<Team[]>([])
  const currentTeam = ref<TeamDetail | null>(null)
  const selectedTeamId = ref<number | null>(null)
  const myInvitations = ref<MyInvitation[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const sortedTeams = computed(() =>
    [...teams.value].sort((a, b) => a.name.localeCompare(b.name))
  )

  const teamCount = computed(() => teams.value.length)

  const getTeamById = computed(() => {
    return (id: number) => teams.value.find(t => t.id === id)
  })

  const selectedTeam = computed(() => {
    if (!selectedTeamId.value) return null
    return teams.value.find(t => t.id === selectedTeamId.value) || null
  })

  // Actions
  async function fetchTeams() {
    loading.value = true
    error.value = null
    try {
      const response = await api.getTeams()
      teams.value = response.data
      // Auto-select first team if none selected
      if (!selectedTeamId.value && teams.value.length > 0) {
        selectedTeamId.value = teams.value[0]!.id
      }
      return teams.value
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to fetch teams'
      throw e
    } finally {
      loading.value = false
    }
  }

  function selectTeam(teamId: number) {
    selectedTeamId.value = teamId
  }

  async function fetchTeam(id: number) {
    loading.value = true
    error.value = null
    try {
      const response = await api.getTeam(id)
      currentTeam.value = response.data
      return response.data
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to fetch team'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function createTeam(dto: CreateTeamDto) {
    loading.value = true
    error.value = null
    try {
      const response = await api.createTeam(dto)
      teams.value.push(response.data)
      return response.data
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to create team'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function updateTeam(id: number, dto: UpdateTeamDto) {
    loading.value = true
    error.value = null
    try {
      await api.updateTeam(id, dto)
      // Refresh team data
      await fetchTeam(id)
      // Update in list
      const index = teams.value.findIndex(t => t.id === id)
      const existingTeam = teams.value[index]
      if (index >= 0 && existingTeam && currentTeam.value) {
        teams.value[index] = {
          ...existingTeam,
          name: currentTeam.value.name,
          description: currentTeam.value.description
        }
      }
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to update team'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function deleteTeam(id: number) {
    loading.value = true
    error.value = null
    try {
      await api.deleteTeam(id)
      teams.value = teams.value.filter(t => t.id !== id)
      if (currentTeam.value?.id === id) {
        currentTeam.value = null
      }
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to delete team'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function inviteMember(teamId: number, dto: InviteMemberDto) {
    loading.value = true
    error.value = null
    try {
      const response = await api.inviteMember(teamId, dto)
      // Refresh team to get updated invitations list
      if (currentTeam.value?.id === teamId) {
        await fetchTeam(teamId)
      }
      return response.data
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to invite member'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function cancelInvitation(teamId: number, invitationId: number) {
    loading.value = true
    error.value = null
    try {
      await api.cancelInvitation(teamId, invitationId)
      // Refresh team to get updated invitations list
      if (currentTeam.value?.id === teamId) {
        await fetchTeam(teamId)
      }
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to cancel invitation'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function updateMemberRole(teamId: number, memberId: number, dto: UpdateMemberRoleDto) {
    loading.value = true
    error.value = null
    try {
      await api.updateMemberRole(teamId, memberId, dto)
      // Refresh team to get updated members list
      if (currentTeam.value?.id === teamId) {
        await fetchTeam(teamId)
      }
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to update member role'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function removeMember(teamId: number, memberId: number) {
    loading.value = true
    error.value = null
    try {
      await api.removeMember(teamId, memberId)
      // Refresh team to get updated members list
      if (currentTeam.value?.id === teamId) {
        await fetchTeam(teamId)
      }
      // Update member count in teams list
      const index = teams.value.findIndex(t => t.id === teamId)
      const existingTeam = teams.value[index]
      if (index >= 0 && existingTeam) {
        teams.value[index] = {
          ...existingTeam,
          memberCount: existingTeam.memberCount - 1
        }
      }
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to remove member'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function leaveTeam(teamId: number) {
    loading.value = true
    error.value = null
    try {
      await api.leaveTeam(teamId)
      teams.value = teams.value.filter(t => t.id !== teamId)
      if (currentTeam.value?.id === teamId) {
        currentTeam.value = null
      }
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to leave team'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function acceptInvitation(token: string) {
    loading.value = true
    error.value = null
    try {
      const response = await api.acceptInvitation({ token })
      // Refresh teams list and invitations
      await fetchTeams()
      await fetchMyInvitations()
      return response.data
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to accept invitation'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function fetchMyInvitations() {
    loading.value = true
    error.value = null
    try {
      const response = await api.getMyInvitations()
      myInvitations.value = response.data
      return myInvitations.value
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to fetch invitations'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function declineInvitation(invitationId: number) {
    loading.value = true
    error.value = null
    try {
      await api.declineInvitation(invitationId)
      // Remove from local state
      myInvitations.value = myInvitations.value.filter(i => i.id !== invitationId)
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error || 'Failed to decline invitation'
      throw e
    } finally {
      loading.value = false
    }
  }

  function clearCurrentTeam() {
    currentTeam.value = null
  }

  function clearError() {
    error.value = null
  }

  async function getProjectTeamMembers(): Promise<TeamMember[]> {
    if (currentTeam.value) {
      return currentTeam.value.members || []
    }

    // Try to get selected team or fetch teams if none
    if (!selectedTeamId.value) {
      await fetchTeams()
    }

    if (selectedTeamId.value) {
      await fetchTeam(selectedTeamId.value)
    }

    const team = currentTeam.value as TeamDetail | null
    return team?.members || []
  }

  function $reset() {
    teams.value = []
    currentTeam.value = null
    selectedTeamId.value = null
    myInvitations.value = []
    loading.value = false
    error.value = null
  }

  return {
    // State
    teams,
    currentTeam,
    selectedTeamId,
    myInvitations,
    loading,
    error,
    // Getters
    sortedTeams,
    teamCount,
    getTeamById,
    selectedTeam,
    // Actions
    fetchTeams,
    fetchTeam,
    createTeam,
    updateTeam,
    deleteTeam,
    inviteMember,
    cancelInvitation,
    updateMemberRole,
    removeMember,
    leaveTeam,
    acceptInvitation,
    fetchMyInvitations,
    declineInvitation,
    selectTeam,
    clearCurrentTeam,
    clearError,
    getProjectTeamMembers,
    $reset
  }
})

