<template>
  <Teleport to="body">
    <div class="modal-overlay" @click.self="$emit('close')">
      <div class="modal max-w-md">
        <h2 class="text-2xl font-bold mb-4">Invite Team Member</h2>
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input
              v-model="form.email"
              type="email"
              required
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              placeholder="colleague@example.com"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Role</label>
            <select
              v-model="form.role"
              class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="Member">Member - Can view and work on projects</option>
              <option value="Admin">Admin - Can also manage members</option>
            </select>
          </div>

          <div v-if="error" class="text-sm text-red-600 bg-red-50 p-3 rounded-lg">
            {{ error }}
          </div>

          <div v-if="invitationToken" class="p-4 bg-green-50 rounded-lg border border-green-200">
            <p class="text-sm text-green-800 font-medium mb-2">Invitation created!</p>
            <p class="text-xs text-green-600 mb-2">
              Share this link with the person you're inviting:
            </p>
            <div class="flex items-center gap-2">
              <input
                :value="invitationUrl"
                readonly
                class="flex-1 text-xs bg-white px-2 py-1 rounded border"
              />
              <button
                type="button"
                @click="copyLink"
                class="px-3 py-1 text-xs bg-green-600 text-white rounded hover:bg-green-700"
              >
                {{ copied ? 'Copied!' : 'Copy' }}
              </button>
            </div>
          </div>

          <div class="flex justify-end gap-2 pt-4">
            <button
              type="button"
              @click="$emit('close')"
              class="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition"
            >
              {{ invitationToken ? 'Done' : 'Cancel' }}
            </button>
            <button
              v-if="!invitationToken"
              type="submit"
              :disabled="loading"
              class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition disabled:opacity-50"
            >
              {{ loading ? 'Sending...' : 'Send Invitation' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useTeamsStore } from '@/stores/teams'
import type { TeamRole } from '@/types/Team'

const props = defineProps<{
  teamId: number
}>()

const emit = defineEmits<{
  close: []
  invited: []
}>()

const teamsStore = useTeamsStore()

const form = ref({
  email: '',
  role: 'Member' as TeamRole
})

const loading = ref(false)
const error = ref('')
const invitationToken = ref('')
const copied = ref(false)

const invitationUrl = computed(() =>
  `${window.location.origin}/invitations/accept?token=${invitationToken.value}`
)

async function handleSubmit() {
  loading.value = true
  error.value = ''

  try {
    const invitation = await teamsStore.inviteMember(props.teamId, form.value)
    if (invitation.token) {
      invitationToken.value = invitation.token
    }
    emit('invited')
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    error.value = err.response?.data?.error || 'Failed to send invitation'
  } finally {
    loading.value = false
  }
}

function copyLink() {
  navigator.clipboard.writeText(invitationUrl.value)
  copied.value = true
  setTimeout(() => {
    copied.value = false
  }, 2000)
}
</script>
