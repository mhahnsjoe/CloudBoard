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
      <div class="relative" ref="projectSelectorRef">
        <button
          @click="showProjectDropdown = !showProjectDropdown"
          class="w-full bg-gray-800 text-white px-3 py-2 rounded-lg border border-gray-700 hover:border-gray-600 focus:border-blue-500 outline-none cursor-pointer transition-all flex items-center justify-between"
        >
          <span class="truncate">{{ currentProjectName || 'Select Project' }}</span>
          <svg 
            class="w-4 h-4 text-gray-400 transition-transform duration-200 flex-shrink-0" 
            :class="{ 'rotate-180': showProjectDropdown }" 
            fill="none" 
            stroke="currentColor" 
            viewBox="0 0 24 24"
          >
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
          </svg>
        </button>

        <!-- Project Dropdown Menu -->
        <div
          v-if="showProjectDropdown"
          class="absolute top-full left-0 right-0 mt-2 bg-gray-800 rounded-lg shadow-xl border border-gray-700 py-2 z-50"
        >
          <!-- <div class="px-3 py-2 text-xs text-gray-400 uppercase tracking-wide border-b border-gray-700">
            Switch Project
          </div> -->
          
          <!-- Project List -->
          <div class="max-h-64 overflow-y-auto">
            <button
              v-for="project in projects"
              :key="project.id"
              @click="handleProjectChange(project.id)"
              class="w-full text-left px-4 py-3 hover:bg-gray-700 transition-colors flex items-center justify-between group"
              :class="{ 'bg-gray-700': project.id === selectedProjectId }"
            >
              <div class="flex items-center gap-3 flex-1 min-w-0">
                <FolderIcon className="w-4 h-4 text-gray-400 flex-shrink-0" />
                <div class="min-w-0 flex-1">
                  <div class="font-medium text-white truncate" :class="{ 'text-blue-400': project.id === selectedProjectId }">
                    {{ project.name }}
                  </div>
                  <div class="text-xs text-gray-400">
                    {{ project.boards?.length || 0 }} boards
                  </div>
                </div>
              </div>
              <span v-if="project.id === selectedProjectId" class="text-blue-400 text-xs font-medium flex-shrink-0">
                Current
              </span>
            </button>
          </div>

          <!-- Add New Project -->
          <div class="border-t border-gray-700 mt-2">
            <button
              @click="openCreateProjectModal"
              class="w-full text-left px-4 py-3 hover:bg-gray-700 transition-colors flex items-center gap-3 text-blue-400 font-medium"
            >
              <PlusIcon className="w-4 h-4" />
              <span>Create New Project</span>
            </button>
          </div>
        </div>
      </div>
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
import { defineComponent, ref, computed, onMounted, watch, onBeforeUnmount } from 'vue';
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
    const showProjectDropdown = ref(false);
    const projectSelectorRef = ref<HTMLElement | null>(null);
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

    const handleProjectChange = async (projectId: number) => {
      showProjectDropdown.value = false;
      
      if (projectId === selectedProjectId.value) {
        return; // Already on this project
      }

      selectedProjectId.value = projectId;

      try {
        // Fetch the project to get its boards
        const project = projects.value.find(p => p.id === projectId);
        
        if (project && project.boards && project.boards.length > 0) {
          // Navigate to the first board
          const firstBoard = project.boards[0];
          router.push(`/projects/${projectId}/boards/${firstBoard.id}`);
        } else {
          // If no boards exist, navigate to the project with boardId 0 (will show empty state)
          router.push(`/projects/${projectId}/boards/0`);
        }
      } catch (error) {
        console.error('Failed to navigate to project:', error);
      }
    };

    const openCreateProjectModal = () => {
      showProjectDropdown.value = false;
      showAddProjectModal.value = true;
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
          router.push(`/projects/${response.data.id}/boards/${firstBoard!.id}`);
        }
      } catch (error) {
        console.error('Failed to create project:', error);
        alert('Failed to create project');
      }
    };

    // Close dropdown when clicking outside
    const handleClickOutside = (event: MouseEvent) => {
      if (projectSelectorRef.value && !projectSelectorRef.value.contains(event.target as Node)) {
        showProjectDropdown.value = false;
      }
    };

    // Watch route changes to update selected project
    watch(() => route.params.projectId, (newProjectId) => {
      if (newProjectId) {
        selectedProjectId.value = Number(newProjectId);
      }
    });

    onMounted(() => {
      fetchProjects();
      document.addEventListener('click', handleClickOutside);
    });

    onBeforeUnmount(() => {
      document.removeEventListener('click', handleClickOutside);
    });

    return { 
      route,
      projects,
      selectedProjectId,
      currentProjectName,
      isOnProjectBoard,
      showAddProjectModal,
      showProjectDropdown,
      projectSelectorRef,
      newProjectName,
      newProjectDescription,
      handleProjectChange,
      openCreateProjectModal,
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

.rotate-180 {
  transform: rotate(180deg);
}
</style>