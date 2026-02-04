<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h2 class="text-2xl font-bold text-gray-900">{{ team.name }}</h2>
        <p class="text-gray-500">{{ team.description || 'No description' }}</p>
      </div>
      <div class="flex gap-2">
        <button
          v-if="canEdit"
          @click="showEditModal = true"
          class="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition"
        >
          Edit
        </button>
        <button
          v-if="isOwner"
          @click="handleDelete"
          class="px-4 py-2 text-red-600 bg-red-50 rounded-lg hover:bg-red-100 transition"
        >
          Delete Team
        </button>
      </div>
    </div>

    <!-- Members Section -->
    <div class="bg-white rounded-lg border">
      <div class="px-4 py-3 border-b flex justify-between items-center">
        <h3 class="font-medium">Members ({{ team.members.length }})</h3>
        <button
          v-if="canManageMembers"
          @click="showInviteModal = true"
          class="px-3 py-1 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition flex items-center gap-1"
        >
          <PlusIcon className="w-4 h-4" />
          Invite
        </button>
      </div>

      <ul class="divide-y">
        <li
          v-for="member in team.members"
          :key="member.userId"
          class="px-4 py-3 flex items-center justify-between"
        >
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-full bg-gray-200 flex items-center justify-center">
              <span class="text-sm font-medium text-gray-600">
                {{ member.name.charAt(0).toUpperCase() }}
              </span>
            </div>
            <div>
              <p class="font-medium text-gray-900">
                {{ member.name }}
                <span v-if="member.userId === currentUserId" class="text-gray-500">(you)</span>
              </p>
              <p class="text-sm text-gray-500">{{ member.email }}</p>
            </div>
          </div>

          <div class="flex items-center gap-2">
            <select
              v-if="canChangeRole(member)"
              :value="member.role"
              @change="updateRole(member.userId, ($event.target as HTMLSelectElement).value as TeamRole)"
              class="text-sm border rounded px-2 py-1"
            >
              <option value="Member">Member</option>
              <option value="Admin">Admin</option>
            </select>
            <span
              v-else
              class="text-sm px-2 py-1 rounded"
              :class="roleClass(member.role)"
            >
              {{ member.role }}
            </span>

            <button
              v-if="canRemove(member)"
              @click="handleRemoveMember(member.userId)"
              class="p-1 text-gray-400 hover:text-red-600"
              title="Remove member"
            >
              <XIcon className="w-4 h-4" />
            </button>
          </div>
        </li>
      </ul>
    </div>

    <!-- Pending Invitations -->
    <div v-if="canManageMembers && team.pendingInvitations.length > 0" class="bg-white rounded-lg border">
      <div class="px-4 py-3 border-b">
        <h3 class="font-medium">Pending Invitations ({{ team.pendingInvitations.length }})</h3>
      </div>

      <ul class="divide-y">
        <li
          v-for="invitation in team.pendingInvitations"
          :key="invitation.id"
          class="px-4 py-3 flex items-center justify-between"
        >
          <div>
            <p class="font-medium text-gray-900">{{ invitation.email }}</p>
            <p class="text-sm text-gray-500">
              Invited as {{ invitation.role }} by {{ invitation.invitedBy.name }}
            </p>
          </div>
          <div class="flex items-center gap-2">
            <span class="text-xs text-gray-500">
              Expires {{ formatDate(invitation.expiresAt) }}
            </span>
            <button
              @click="handleCancelInvitation(invitation.id)"
              class="px-2 py-1 text-sm text-red-600 hover:bg-red-50 rounded"
            >
              Cancel
            </button>
          </div>
        </li>
      </ul>
    </div>

    <!-- Projects Section -->
    <div class="bg-white rounded-lg border">
      <div class="px-4 py-3 border-b">
        <h3 class="font-medium">Projects ({{ team.projects.length }})</h3>
      </div>

      <div v-if="team.projects.length === 0" class="px-4 py-8 text-center text-gray-500">
        No projects yet. Create a project to get started.
      </div>

      <ul v-else class="divide-y">
        <li
          v-for="project in team.projects"
          :key="project.id"
          class="px-4 py-3 flex items-center justify-between hover:bg-gray-50"
        >
          <div>
            <p class="font-medium text-gray-900">{{ project.name }}</p>
            <p class="text-sm text-gray-500">Created {{ formatDate(project.createdAt) }}</p>
          </div>
          <div class="flex items-center gap-2">
            <router-link
              :to="`/projects/${project.id}/backlog`"
              class="text-blue-600 hover:underline text-sm"
            >
              View
            </router-link>
            <button
              v-if="canDeleteProject"
              @click="handleDeleteProject(project.id, project.name)"
              class="p-1 text-gray-400 hover:text-red-600"
              title="Delete project"
            >
              <XIcon className="w-4 h-4" />
            </button>
          </div>
        </li>
      </ul>
    </div>

    <!-- Leave Team -->
    <div v-if="!isOwner" class="pt-4 border-t">
      <button
        @click="handleLeave"
        class="text-red-600 hover:text-red-700"
      >
        Leave team
      </button>
    </div>

    <!-- Invite Modal -->
    <InviteMemberModal
      v-if="showInviteModal"
      :team-id="team.id"
      @close="showInviteModal = false"
      @invited="$emit('refresh')"
    />

    <!-- Edit Modal -->
    <EditTeamModal
      v-if="showEditModal"
      :team="team"
      @close="showEditModal = false"
      @updated="$emit('refresh')"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { PlusIcon, XIcon } from '@/components/icons'
import type { TeamDetail, TeamMember, TeamRole } from '@/types/Team'
import { useAuthStore } from '@/stores/auth'
import { useTeamsStore } from '@/stores/teams'
import { useProjectStore } from '@/stores/projects'
import InviteMemberModal from './InviteMemberModal.vue'
import EditTeamModal from './EditTeamModal.vue'

const props = defineProps<{
  team: TeamDetail
}>()

const emit = defineEmits<{
  refresh: []
  leave: []
}>()

const authStore = useAuthStore()
const teamsStore = useTeamsStore()
const projectStore = useProjectStore()

const showEditModal = ref(false)
const showInviteModal = ref(false)

const currentUserId = computed(() => authStore.user?.id)
const isOwner = computed(() => props.team.currentUserRole === 'Owner')
const isAdmin = computed(() => props.team.currentUserRole === 'Admin' || isOwner.value)
const canEdit = computed(() => isAdmin.value)
const canManageMembers = computed(() => isAdmin.value)
const canDeleteProject = computed(() => isAdmin.value)

function canChangeRole(member: TeamMember): boolean {
  if (member.userId === currentUserId.value) return false
  if (member.role === 'Owner') return false
  if (!isAdmin.value) return false
  if (props.team.currentUserRole === 'Admin' && member.role === 'Admin') return false
  return true
}

function canRemove(member: TeamMember): boolean {
  if (member.userId === currentUserId.value) return false
  if (member.role === 'Owner') return false
  return canChangeRole(member)
}

function roleClass(role: TeamRole) {
  switch (role) {
    case 'Owner': return 'bg-purple-100 text-purple-800'
    case 'Admin': return 'bg-blue-100 text-blue-800'
    default: return 'bg-gray-100 text-gray-800'
  }
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString()
}

async function updateRole(userId: number, role: TeamRole) {
  try {
    await teamsStore.updateMemberRole(props.team.id, userId, { role })
    emit('refresh')
  } catch (e) {
    console.error('Failed to update role:', e)
  }
}

async function handleRemoveMember(userId: number) {
  if (!confirm('Remove this member from the team?')) return
  try {
    await teamsStore.removeMember(props.team.id, userId)
    emit('refresh')
  } catch (e) {
    console.error('Failed to remove member:', e)
  }
}

async function handleCancelInvitation(invitationId: number) {
  if (!confirm('Cancel this invitation?')) return
  try {
    await teamsStore.cancelInvitation(props.team.id, invitationId)
    emit('refresh')
  } catch (e) {
    console.error('Failed to cancel invitation:', e)
  }
}

async function handleLeave() {
  if (!confirm('Are you sure you want to leave this team?')) return
  try {
    await teamsStore.leaveTeam(props.team.id)
    emit('leave')
  } catch (e) {
    console.error('Failed to leave team:', e)
  }
}

async function handleDelete() {
  if (!confirm('Are you sure you want to delete this team? This action cannot be undone.')) return
  try {
    await teamsStore.deleteTeam(props.team.id)
    emit('leave')
  } catch (e) {
    console.error('Failed to delete team:', e)
  }
}

async function handleDeleteProject(projectId: number, projectName: string) {
  if (!confirm(`Are you sure you want to delete "${projectName}"? This will permanently delete all boards and work items in this project.`)) return
  try {
    await projectStore.deleteProject(projectId)
    emit('refresh')
  } catch (e) {
    console.error('Failed to delete project:', e)
    alert('Failed to delete project')
  }
}
</script>
