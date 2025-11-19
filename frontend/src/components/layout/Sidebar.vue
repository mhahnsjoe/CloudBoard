In the meantime, here's the updated Sidebar with a lighter theme using your brand colors and a more integrated project selector:

Updated Sidebar.vue
Replace the entire template section:

<template>
  <div class="fixed left-0 top-0 h-full w-64 bg-gray-800 text-gray-100 flex flex-col shadow-lg">
     <!-- Logo/Header -->
    <div class="p-3 border-b border-gray-700 bg-gray-900">
      <div class="bg-white rounded-lg overflow-hidden" style="height: 60px;">
        <img src="@/assets/logo.png" alt="CloudBoard" class="object-cover" />
        <h1>CloudBoard</h1>
      </div>
    </div>

    <!-- Project Selector (integrated style) -->
    <div class="px-4 pt-4 pb-2">
      <div class="relative" ref="projectSelectorRef">
        <button
          @click="projectDropdown.toggle"
          class="w-full text-left px-4 py-3 rounded-lg hover:bg-gray-700 transition-all flex items-center justify-between group"
          :class="{ 'bg-gray-700': showProjectDropdown }"
        >
          <div class="flex items-center gap-3 flex-1 min-w-0">
            <FolderIcon className="w-5 h-5 text-gray-400" />
            <span class="font-semibold text-base truncate text-white">{{ currentProjectName || 'Select Project' }}</span>
          </div>
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
          class="absolute top-full left-0 right-0 mt-2 bg-gray-750 rounded-lg shadow-xl border border-gray-600 py-2 z-50"
          style="background-color: #374151;"
        >
          <!-- Project List -->
          <div class="max-h-64 overflow-y-auto">
            <button
              v-for="project in projects"
              :key="project.id"
              @click="handleProjectChange(project.id)"
              class="w-full text-left px-4 py-3 hover:bg-gray-600 transition-colors flex items-center justify-between group"
              :class="{ 'bg-gray-600': project.id === selectedProjectId }"
            >
              <div class="flex items-center gap-3 flex-1 min-w-0">
                <FolderIcon className="w-4 h-4 text-gray-400 flex-shrink-0" />
                <div class="min-w-0 flex-1">
                  <div class="font-medium text-white truncate">
                    {{ project.name }}
                  </div>
                  <div class="text-xs text-gray-400">
                    {{ project.boards?.length || 0 }} boards
                  </div>
                </div>
              </div>
              <span v-if="project.id === selectedProjectId" class="text-xs font-medium flex-shrink-0" style="color: #25AAE1;">
                Current
              </span>
            </button>
          </div>

          <!-- Add New Project -->
          <div class="border-t border-gray-600 mt-2">
            <button
              @click="openCreateProjectModal"
              class="w-full text-left px-4 py-3 hover:bg-gray-600 transition-colors flex items-center gap-3 font-medium"
              style="color: #25AAE1;"
            >
              <PlusIcon className="w-4 h-4" />
              <span>Create New Project</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 px-4 py-2 overflow-y-auto">
      <!-- Project-specific navigation -->
      <div v-if="selectedProjectId">
        <!-- Summary -->
        <router-link
          :to="`/projects/${selectedProjectId}/summary`"
          class="nav-item"
          :class="{ 'active': route.path.includes('/summary') }"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
          </svg>
          <span>Summary</span>
        </router-link>

        <!-- Boards Section (Expandable) -->
        <div>
          <button
            @click="toggleBoards"
            class="w-full flex items-center justify-between px-4 py-3 rounded-lg text-gray-300 hover:bg-gray-700 transition-all mb-2"
            :class="{ 'bg-gray-700 text-white': isBoardsExpanded || isOnProjectBoard }"
          >
            <div class="flex items-center gap-3">
              <FolderIcon className="w-5 h-5" />
              <span>Boards</span>
            </div>
            <svg
              class="w-4 h-4 transition-transform"
              :class="{ 'rotate-90': isBoardsExpanded }"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </button>

          <!-- Nested Board Items -->
          <div v-if="isBoardsExpanded" class="ml-4 space-y-1">
            <router-link
              v-for="board in projectBoards"
              :key="board.id"
              :to="`/projects/${selectedProjectId}/boards/${board.id}`"
              class="nav-item-nested"
              :class="{ 'active': route.params.boardId === String(board.id) }"
            >
              <span class="text-sm">{{ board.name }}</span>
              <span
                class="text-xs px-2 py-0.5 rounded"
                :class="getBoardTypeBadgeClass(board.type)"
              >
                {{ board.type }}
              </span>
            </router-link>

            <!-- Backlog Tab -->
            <router-link
              :to="`/projects/${selectedProjectId}/backlog`"
              class="nav-item-nested"
              :class="{ 'active': route.path.includes('/backlog') }"
            >
              <span class="text-sm">Backlog</span>
              <span class="text-xs px-2 py-0.5 rounded bg-gray-600 text-gray-200">
                All Items
              </span>
            </router-link>
          </div>
        </div>
      </div>

      <!-- No Project Selected State -->
      <div v-else class="text-center py-8 px-4">
        <svg class="w-12 h-12 mx-auto mb-3 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
        </svg>
        <p class="text-sm text-gray-400">Select a project above</p>
      </div>
    </nav>

     <!-- User Footer -->
    <div class="p-4 border-t border-gray-700">
      <div class="relative" ref="userMenuRef">
        <button
          @click="userMenuDropdown.toggle"
          class="w-full flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-gray-700 transition-colors"
        >
          <div class="w-8 h-8 rounded-full bg-gray-600 flex items-center justify-center text-white font-semibold flex-shrink-0">
            {{ userInitials }}
          </div>
          <div class="flex-1 text-left min-w-0">
            <div class="text-sm font-medium text-white truncate">{{ authStore.user?.name || 'User' }}</div>
            <div class="text-xs text-gray-400 truncate">{{ authStore.user?.email }}</div>
          </div>
          <svg
            class="w-4 h-4 text-gray-400 flex-shrink-0"
            :class="{ 'rotate-180': showUserMenu }"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
          </svg>
        </button>

        <!-- User Dropdown Menu -->
        <div
          v-if="showUserMenu"
          class="absolute bottom-full left-0 right-0 mb-2 rounded-lg shadow-xl border border-gray-600 py-2 z-50"
          style="background-color: #374151;"
        >
          <button
            @click="handleLogout"
            class="w-full text-left px-4 py-2 hover:bg-gray-600 transition-colors flex items-center gap-3 text-red-400"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
            </svg>
            <span>Logout</span>
          </button>
        </div>
      </div>
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
import { defineComponent, ref, computed, watch, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { FolderIcon, PlusIcon } from '@/components/icons';
import Modal from '@/components/common/Modal.vue';
import { getProjects, createProject } from '@/services/api';
import type { Project } from '@/types/Project';
import { useAuthStore } from '@/stores/auth';
import { useDropdown } from '@/composables/useDropdown';
import { useClickOutside } from '@/composables/useClickOutside';
import type { Board } from '@/types/Project';
import { getBoards } from '@/services/api';


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
    const authStore = useAuthStore();
    const selectedProjectId = ref<number | null>(null);
    const showAddProjectModal = ref(false);
    const newProjectName = ref('');
    const newProjectDescription = ref('');
    const isBoardsExpanded = ref(false);
    const projectBoards = ref<Board[]>([]);

    // Dropdown management
    const projectSelectorRef = ref<HTMLElement | null>(null);
    const userMenuRef = ref<HTMLElement | null>(null);
    const projectDropdown = useDropdown();
    const userMenuDropdown = useDropdown();

    useClickOutside(projectSelectorRef, projectDropdown.close);
    useClickOutside(userMenuRef, userMenuDropdown.close);

    const currentProjectName = computed(() => {
      const project = projects.value.find(p => p.id === selectedProjectId.value);
      return project?.name || '';
    });

    const isOnProjectBoard = computed(() => {
      return route.path.includes('/projects/') && route.path.includes('/boards/');
    });

    const userInitials = computed(() => {
      const name = authStore.user?.name || '';
      return name
        .split(' ')
        .map(n => n[0])
        .join('')
        .toUpperCase()
        .slice(0, 2) || 'U';
    });

    const handleLogout = () => {
      authStore.logout();
      router.push('/login');
    };

    const toggleBoards = () => {
      isBoardsExpanded.value = !isBoardsExpanded.value;
      if (isBoardsExpanded.value && projectBoards.value.length === 0) {
        fetchProjectBoards();
      }
    };
    const fetchProjectBoards = async () => {
      if (!selectedProjectId.value) return;
      try {
        const response = await getBoards(selectedProjectId.value);
        projectBoards.value = response.data;
      } catch (error) {
        console.error('Failed to fetch boards:', error);
      }
    };
    const getBoardTypeBadgeClass = (type: string) => {
      const classes: Record<string, string> = {
        'Scrum': 'bg-blue-900 text-blue-200',
        'Kanban': 'bg-green-900 text-green-200',
        'Backlog': 'bg-gray-600 text-gray-200'
      };
      return classes[type] || 'bg-gray-600 text-gray-300';
    };

    const fetchProjects = async () => {
      try {
        const response = await getProjects();
        projects.value = response.data;
        
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
      projectDropdown.close();
      
      if (projectId === selectedProjectId.value) {
        return;
      }

      selectedProjectId.value = projectId;

      try {
        const project = projects.value.find(p => p.id === projectId);
        
        if (project && project.boards && project.boards.length > 0) {
          const firstBoard = project.boards[0];
          router.push(`/projects/${projectId}/boards/${firstBoard!.id}`);
        } else {
          router.push(`/projects/${projectId}/boards/0`);
        }
      } catch (error) {
        console.error('Failed to navigate to project:', error);
      }
    };

    const openCreateProjectModal = () => {
      projectDropdown.close();
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

    watch(() => route.params.projectId, (newProjectId) => {
     if (newProjectId) {
        selectedProjectId.value = Number(newProjectId);
        if (isBoardsExpanded.value) {
          fetchProjectBoards();
        }
      }
    });

    watch(selectedProjectId, (newId) => {
      if (newId && isBoardsExpanded.value) {
        fetchProjectBoards();
      }
    });

    onMounted(() => {
      fetchProjects();
    });

    return {
      route,
      authStore,
      projects,
      selectedProjectId,
      currentProjectName,
      isOnProjectBoard,
      userInitials,
      showAddProjectModal,
      showProjectDropdown: projectDropdown.isOpen,
      showUserMenu: userMenuDropdown.isOpen,
      projectSelectorRef,
      userMenuRef,
      newProjectName,
      newProjectDescription,
      projectDropdown,
      userMenuDropdown,
      isBoardsExpanded,
      projectBoards,
      toggleBoards,
      fetchProjectBoards,
      getBoardTypeBadgeClass,
      handleProjectChange,
      handleLogout,
      openCreateProjectModal,
      handleCreateProject,
      fetchProjects
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