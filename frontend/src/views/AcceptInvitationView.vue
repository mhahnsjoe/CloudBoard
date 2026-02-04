<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50">
    <div class="max-w-md w-full p-6 bg-white rounded-lg shadow">
      <div v-if="loading" class="text-center">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
        <p class="mt-2 text-gray-600">Accepting invitation...</p>
      </div>

      <div v-else-if="success" class="text-center">
        <div class="mx-auto h-12 w-12 text-green-500">
          <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
        </div>
        <h2 class="mt-4 text-xl font-semibold">Welcome to the team!</h2>
        <p class="mt-2 text-gray-600">You've successfully joined the team.</p>
        <router-link
          :to="{ name: 'Teams' }"
          class="mt-4 inline-block px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
        >
          Go to Teams
        </router-link>
      </div>

      <div v-else class="text-center">
        <div class="mx-auto h-12 w-12 text-red-500">
          <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
        </div>
        <h2 class="mt-4 text-xl font-semibold">Invitation Failed</h2>
        <p class="mt-2 text-gray-600">{{ error }}</p>
        <router-link
          :to="{ name: 'Teams' }"
          class="mt-4 inline-block text-blue-600 hover:underline"
        >
          Go to Teams
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useTeamsStore } from '@/stores/teams'

const route = useRoute()
const teamsStore = useTeamsStore()

const loading = ref(true)
const success = ref(false)
const error = ref('')

onMounted(async () => {
  const token = route.query.token as string

  if (!token) {
    error.value = 'Invalid invitation link'
    loading.value = false
    return
  }

  try {
    await teamsStore.acceptInvitation(token)
    success.value = true
  } catch (e: unknown) {
    const err = e as { response?: { data?: { error?: string } } }
    error.value = err.response?.data?.error || 'Failed to accept invitation'
  } finally {
    loading.value = false
  }
})
</script>
