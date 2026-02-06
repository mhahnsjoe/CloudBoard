<template>
  <div v-if="invitations.length > 0" class="invitations-panel">
    <h3 class="panel-title">
      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M22 10.5V6a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2v12c0 1.1.9 2 2 2h12.5"/>
        <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7"/>
        <path d="M18 15.28c.2-.4.5-.8.9-1a2.1 2.1 0 0 1 2.6.4c.3.4.5.8.5 1.3 0 1.3-2 2-2 2"/>
        <path d="M20 21v.01"/>
      </svg>
      Pending Invitations
      <span class="badge">{{ invitations.length }}</span>
    </h3>

    <div class="invitations-list">
      <div
        v-for="invitation in invitations"
        :key="invitation.id"
        class="invitation-card"
      >
        <div class="invitation-info">
          <div class="team-name">{{ invitation.teamName }}</div>
          <div class="team-description" v-if="invitation.teamDescription">
            {{ invitation.teamDescription }}
          </div>
          <div class="invitation-meta">
            <span class="invited-by">
              Invited by {{ invitation.invitedBy.name }}
            </span>
            <span class="separator">•</span>
            <span class="role">Role: {{ invitation.role }}</span>
            <span class="separator">•</span>
            <span class="expires" :class="{ 'expiring-soon': isExpiringSoon(invitation.expiresAt) }">
              Expires {{ formatDate(invitation.expiresAt) }}
            </span>
          </div>
        </div>

        <div class="invitation-actions">
          <button
            class="btn btn-primary btn-sm"
            @click="handleAccept(invitation)"
            :disabled="loading"
          >
            Accept
          </button>
          <button
            class="btn btn-outline btn-sm"
            @click="handleDecline(invitation.id)"
            :disabled="loading"
          >
            Decline
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useTeamsStore } from '@/stores/teams'
import { useProjectStore } from '@/stores/projects'
import type { MyInvitation } from '@/types/Team'

const teamsStore = useTeamsStore()
const projectStore = useProjectStore()

const invitations = computed(() => teamsStore.myInvitations)
const loading = computed(() => teamsStore.loading)

async function handleAccept(invitation: MyInvitation) {
  try {
    await teamsStore.acceptInvitation(invitation.token)
    // Refresh projects since user now has access to new team's projects
    await projectStore.fetchProjects()
  } catch {
    // Error is handled by store
  }
}

async function handleDecline(invitationId: number) {
  try {
    await teamsStore.declineInvitation(invitationId)
  } catch {
    // Error is handled by store
  }
}

function formatDate(dateStr: string): string {
  const date = new Date(dateStr)
  const now = new Date()
  const diffMs = date.getTime() - now.getTime()
  const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24))

  if (diffDays <= 0) return 'today'
  if (diffDays === 1) return 'tomorrow'
  if (diffDays <= 7) return `in ${diffDays} days`
  return date.toLocaleDateString()
}

function isExpiringSoon(dateStr: string): boolean {
  const date = new Date(dateStr)
  const now = new Date()
  const diffMs = date.getTime() - now.getTime()
  const diffDays = diffMs / (1000 * 60 * 60 * 24)
  return diffDays <= 2
}
</script>

<style scoped>
.invitations-panel {
  background: var(--card-bg, #fff);
  border: 1px solid var(--border-color, #e5e7eb);
  border-radius: 12px;
  padding: 1.25rem;
  margin-bottom: 1.5rem;
}

.panel-title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1rem;
  font-weight: 600;
  color: var(--text-primary, #111827);
  margin: 0 0 1rem 0;
}

.panel-title svg {
  color: var(--color-primary, #6366f1);
}

.badge {
  background: var(--color-primary, #6366f1);
  color: white;
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.125rem 0.5rem;
  border-radius: 9999px;
  margin-left: auto;
}

.invitations-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.invitation-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem;
  background: var(--bg-secondary, #f9fafb);
  border-radius: 8px;
  border: 1px solid var(--border-color, #e5e7eb);
}

.invitation-info {
  flex: 1;
  min-width: 0;
}

.team-name {
  font-weight: 600;
  color: var(--text-primary, #111827);
  margin-bottom: 0.25rem;
}

.team-description {
  font-size: 0.875rem;
  color: var(--text-secondary, #6b7280);
  margin-bottom: 0.5rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.invitation-meta {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.75rem;
  color: var(--text-tertiary, #9ca3af);
}

.separator {
  color: var(--border-color, #e5e7eb);
}

.expires.expiring-soon {
  color: var(--color-warning, #f59e0b);
  font-weight: 500;
}

.invitation-actions {
  display: flex;
  gap: 0.5rem;
  flex-shrink: 0;
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: 6px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s ease;
  border: none;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.8125rem;
}

.btn-primary {
  background: var(--color-primary, #6366f1);
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: var(--color-primary-hover, #4f46e5);
}

.btn-outline {
  background: transparent;
  border: 1px solid var(--border-color, #e5e7eb);
  color: var(--text-secondary, #6b7280);
}

.btn-outline:hover:not(:disabled) {
  background: var(--bg-hover, #f3f4f6);
  border-color: var(--text-tertiary, #9ca3af);
}

@media (max-width: 640px) {
  .invitation-card {
    flex-direction: column;
    align-items: stretch;
  }

  .invitation-actions {
    justify-content: flex-end;
    margin-top: 0.75rem;
  }
}
</style>
