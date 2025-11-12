<template>
  <div class="fixed left-0 top-0 h-full w-64 bg-gray-900 text-white flex flex-col shadow-lg">
    <!-- Logo/Header -->
    <div class="p-6 border-b border-gray-700">
      <h1 class="text-2xl font-bold">CloudBoard</h1>
      <p class="text-sm text-gray-400 mt-1">Project Management</p>
    </div>

    <!-- Project Selector -->
    <div class="px-4 py-3 border-b border-gray-700">
      <label class="text-xs text-gray-400 uppercase tracking-wide mb-2 block">Project</label>
      <div class="relative">
        <select
          v-model="selectedProjectId"
          @change="handleProjectChange"
          class="w-full bg-gray-800 text-white px-3 py-2 rounded-lg border border-gray-700 focus:border-blue-500 focus:ring-1 focus:ring-blue-500 outline-none cursor-pointer appearance-none pr-8"
        >
          <option v-for="project in projects" :key="project.id" :value="project.id">
            {{ project.name }}
          </option>
        </select>
        <div class="absolute right-2 top-1/2 transform -translate-y-1/2 pointer-events-none">
          <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
          </svg>
        </div>
      </div>
      
      <!-- Add Project Button -->
      <button
        @click="showAddProjectModal = true"
        class="w-full mt-2 px-3 py-1.5 text-xs bg-blue-600 hover:bg-blue-700 rounded-lg transition-all flex items-center justify-center gap-2"
      >
        <PlusIcon className="w-3 h-3" />
        New Project
      </button>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 p-4 overflow-y-auto">
      <router-link
        to="/"
        class="nav-item"
        :class="{ 'active': route.path === '/' }"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/>
        </svg>
        <span>Summary</span>
      </router-link>

      <div v-if="selectedProjectId" class="mt-4">
        <div class="text-xs text-gray-400 uppercase tracking-wide mb-2 px-4">
          {{ currentProjectName }}
        </div>

        <div class="nav-item" :class="{ 'active': isOnProjectBoard }">
          <FolderIcon className="w-5 h-5" />
          <span>Boards</span>
        </div>
      </div>
    </nav>

    <!-- Footer -->
    <div class="p-4 border-t border-gray-700 text-sm text-gray-400">
      <p>v1.0.0</p>
    </div>

    <!-- Add Project Modal -->
    <Modal
      :show="showAddProjectModal"
      title="Create New Project"
      submitText="Create"
      @close="showAddProjectModal = false"
      @submit="handleCreateProject"
    >
      <input
        v-model="newProjectName"
        type="text"
        placeholder="Project name"
        class="input"
        @keyup.enter="handleCreateProject"
      />
      <textarea
        v-model="newProjectDescription"
        placeholder="Project description (optional)"
        class="input min-h-[100px]"
      />
    </Modal>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { FolderIcon, PlusIcon } from '@/components/icons';
import Modal from '@/components/common/Modal.vue';
import { getProjects, createProject } from '@/services/api';
import type { Project } from '@/types/Project';

export default defineComponent({
  name: 'SidebarComponent',
  components: {
    FolderIcon,
    PlusIcon,
    Modal
  },
  setup() {
    const route = useRoute();
    const router = useRouter();
    const projects = ref<Project[]>([]);
    const selectedProjectId = ref<number | null>(null);
    const showAddProjectModal = ref(false);
    const newProjectName = ref('');
    const newProjectDescription = ref('');

    const currentProjectName = computed(() => {
      const project = projects.value.find(p => p.id === selectedProjectId.value);
      return project?.name || '';
    });

    const isOnProjectBoard = computed(() => {
      return route.path.includes('/projects/') && route.path.includes('/boards/');
    });

    const fetchProjects = async () => {
      try {
        const response = await getProjects();
        projects.value = response.data;
        
        // Set selected project from route or default to first project
        if (route.params.projectId) {
          selectedProjectId.value = Number(route.params.projectId);
        } else if (projects.value.length > 0 && !selectedProjectId.value) {
          selectedProjectId.value = projects.value[0]!.id;
        }
      } catch (error) {
        console.error('Failed to fetch projects:', error);
      }
    };

    const handleProjectChange = async () => {
      if (!selectedProjectId.value) return;

      try {
        // Fetch the project to get its boards
        const project = projects.value.find(p => p.id === selectedProjectId.value);
        
        if (project && project.boards && project.boards.length > 0) {
          // Navigate to the first board
          const firstBoard = project.boards[0];
          router.push(`/projects/${selectedProjectId.value}/boards/${firstBoard.id}`);
        } else {
          // If no boards exist, stay on current page or go to summary
          console.warn('No boards found for this project');
          alert('This project has no boards. Please create a board first.');
        }
      } catch (error) {
        console.error('Failed to navigate to project:', error);
      }
    };

    const handleCreateProject = async () => {
      if (!newProjectName.value.trim()) {
        alert('Project name is required');
        return;
      }

      try {
        const response = await createProject({
          name: newProjectName.value,
          description: newProjectDescription.value
        });
        
        newProjectName.value = '';
        newProjectDescription.value = '';
        showAddProjectModal.value = false;
        
        await fetchProjects();
        
        // Navigate to the newly created project's default board
        if (response.data && response.data.boards && response.data.boards.length > 0) {
          selectedProjectId.value = response.data.id;
          const firstBoard = response.data.boards[0];
          router.push(`/projects/${response.data.id}/boards/${firstBoard.id}`);
        }
      } catch (error) {
        console.error('Failed to create project:', error);
        alert('Failed to create project');
      }
    };

    // Watch route changes to update selected project
    watch(() => route.params.projectId, (newProjectId) => {
      if (newProjectId) {
        selectedProjectId.value = Number(newProjectId);
      }
    });

    onMounted(fetchProjects);

    return { 
      route,
      projects,
      selectedProjectId,
      currentProjectName,
      isOnProjectBoard,
      showAddProjectModal,
      newProjectName,
      newProjectDescription,
      handleProjectChange,
      handleCreateProject
    };
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