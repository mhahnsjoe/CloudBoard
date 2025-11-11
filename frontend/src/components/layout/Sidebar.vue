<template>
  <div class="fixed left-0 top-0 h-full w-64 bg-gray-900 text-white flex flex-col shadow-lg">
    <!-- Logo/Header -->
    <div class="p-6 border-b border-gray-700">
      <h1 class="text-2xl font-bold">CloudBoard</h1>
      <p class="text-sm text-gray-400 mt-1">Project Management</p>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 p-4">
      <router-link
        to="/"
        class="nav-item"
        :class="{ 'active': route.path === '/' }"
      >
        <FolderIcon className="w-5 h-5" />
        <span>Projects</span>
      </router-link>

      <router-link
        to="/work-items"
        class="nav-item"
        :class="{ 'active': route.path === '/work-items' }"
      >
        <ClipboardIcon className="w-5 h-5" />
        <span>Work Items</span>
      </router-link>
    </nav>

    <!-- Footer -->
    <div class="p-4 border-t border-gray-700 text-sm text-gray-400">
      <p>v1.0.0</p>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { FolderIcon, ClipboardIcon } from '@/components/icons';
import { useRoute } from 'vue-router';

export default defineComponent({
  name: 'SidebarComponent',
  components: {
    FolderIcon,
    ClipboardIcon
  },
  setup() {
    const route = useRoute();
    return { route };
  }
});
</script>

<style scoped>
.nav-item {
  @apply flex items-center gap-3 px-4 py-3 rounded-lg text-gray-300 hover:bg-gray-800 hover:text-white transition-all mb-2 cursor-pointer;
}

.nav-item.active {
  @apply bg-blue-600 text-white;
}
</style>