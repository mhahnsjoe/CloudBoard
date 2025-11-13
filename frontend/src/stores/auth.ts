import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface User {
  id: number
  email: string
  name: string
  createdAt: string
}

export interface AuthResponse {
  token: string
  user: User
}

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('auth_token'))
  const user = ref<User | null>(null)

  const isAuthenticated = computed(() => !!token.value)

  function setAuth(authResponse: AuthResponse) {
    token.value = authResponse.token
    user.value = authResponse.user
    localStorage.setItem('auth_token', authResponse.token)
    localStorage.setItem('auth_user', JSON.stringify(authResponse.user))
  }

  function loadUserFromStorage() {
    const storedUser = localStorage.getItem('auth_user')
    if (storedUser) {
      try {
        user.value = JSON.parse(storedUser)
      } catch (e) {
        console.error('Failed to parse stored user', e)
      }
    }
  }

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem('auth_token')
    localStorage.removeItem('auth_user')
  }

  // Load user from storage on initialization
  if (token.value) {
    loadUserFromStorage()
  }

  return {
    token,
    user,
    isAuthenticated,
    setAuth,
    logout,
    loadUserFromStorage
  }
})