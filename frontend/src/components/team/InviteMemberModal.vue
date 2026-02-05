<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
    <div class="bg-white rounded-lg shadow-xl w-full max-w-md mx-4 overflow-hidden" ref="modalRef">
      <!-- Header -->
      <div class="px-6 py-4 border-b border-gray-100 flex items-center justify-between bg-gray-50">
        <h3 class="text-lg font-semibold text-gray-800">Invite Member</h3>
        <button 
          @click="$emit('close')" 
          class="text-gray-400 hover:text-gray-600 transition-colors"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <!-- Body -->
      <form @submit.prevent="handleSubmit" class="p-6 space-y-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Email Address *</label>
          <input 
            v-model="form.email"
            type="email" 
            placeholder="colleague@example.com"
            class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition-all"
            required
            :disabled="loading"
          />
        </div>

        <div>
           <label class="block text-sm font-medium text-gray-700 mb-1">Role</label>
           <select 
             v-model="form.role"
             class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition-all"
             :disabled="loading"
           >
             <option value="Member">Member (Standard Access)</option>
             <option value="Admin">Admin (Full Access)</option>
           </select>
        </div>

        <div v-if="error" class="text-sm text-red-600 bg-red-50 p-3 rounded-lg flex items-center gap-2">
           <svg class="w-4 h-4 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 1118 0z" /></svg>
           {{ error }}
        </div>

        <!-- Footer -->
        <div class="pt-2 flex items-center justify-end gap-3">
          <button 
            type="button" 
            @click="$emit('close')" 
            class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            :disabled="loading"
          >
            Cancel
          </button>
          <button 
            type="submit" 
            class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
            :disabled="loading"
          >
            <svg v-if="loading" class="animate-spin -ml-1 mr-1 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            {{ loading ? 'Sending...' : 'Send Invitation' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useTeamsStore } from '@/stores/teams'
import type { InviteMemberDto, TeamRole } from '@/types/Team'

const props = defineProps<{ teamId: number }>()
const emit = defineEmits(['close', 'invited'])

const teamsStore = useTeamsStore()
const loading = ref(false)
const error = ref<string | null>(null)

const form = reactive<{
  email: string
  role: TeamRole
}>({
  email: '',
  role: 'Member'
})

const handleSubmit = async () => {
  loading.value = true
  error.value = null
  
  try {
    const dto: InviteMemberDto = {
        email: form.email,
        role: form.role
    }
    
    await teamsStore.inviteMember(props.teamId, dto)
    emit('invited')
    emit('close')
  } catch (e: any) {
    error.value = e.response?.data?.message || e.message || 'Failed to send invitation'
  } finally {
    loading.value = false
  }
}
</script>
