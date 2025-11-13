<template>
  <div id="app">
    <!-- Only show Sidebar when authenticated AND not on auth pages -->
    <Sidebar v-if="showSidebar" />
    
    <!-- Main content with conditional padding -->
    <div :class="showSidebar ? 'ml-64' : ''">
      <router-view />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import Sidebar from '@/components/layout/Sidebar.vue'

const route = useRoute()
const authStore = useAuthStore()

const showSidebar = computed(() => {
  const authPages = ['Login', 'Register']
  const isAuthPage = authPages.includes(route.name as string)
  return authStore.isAuthenticated && !isAuthPage
})
</script>