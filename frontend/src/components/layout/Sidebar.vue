<template>
  <div class="fixed left-0 top-0 h-full w-64 bg-gray-800 text-gray-100 flex flex-col shadow-lg z-40">
     <!-- Logo/Header -->
    <div class="p-3 border-b border-gray-700 bg-gray-900">
      <div class="flex items-center gap-3 px-2">
        <div class="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
          <span class="text-white font-bold text-sm">CB</span>
        </div>
        <h1 class="text-lg font-bold text-white">CloudBoard</h1>
      </div>
    </div>

    <!-- Project Selector -->
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
          class="absolute top-full left-0 right-0 mt-2 bg-gray-700 rounded-lg shadow-xl border border-gray-600 py-2 z-50"
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
              <span v-if="project.id === selectedProjectId" class="text-xs font-medium text-blue-400 flex-shrink-0">
                Current
              </span>
            </button>
          </div>

          <!-- Add New Project -->
          <div class="border-t border-gray-600 mt-2">
            <button
              @click="openCreateProjectModal"
              class="w-full text-left px-4 py-3 hover:bg-gray-600 transition-colors flex items-center gap-3 font-medium text-blue-400"
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
        <!-- Dashboard/Summary -->
        <router-link
          to="/"
          class="nav-item"
          :class="{ 'active': route.path === '/' }"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
          </svg>
          <span>Dashboard</span>
        </router-link>

        <!-- Backlog -->
        <router-link
          :to="`/projects/${selectedProjectId}/backlog`"
          class="nav-item"
          :class="{ 'active': route.path.includes('/backlog') }"
        >
          <ClipboardIcon className="w-5 h-5" />
          <span>Backlog</span>
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
              <span class="text-xs bg-gray-600 px-1.5 py-0.5 rounded">{{ boardStore.boards.length }}</span>
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
              v-for="board in boardStore.boards"
              :key="board.id"
              :to="`/projects/${selectedProjectId}/boards/${board.id}`"
              class="nav-item-nested"
              :class="{ 'active': route.params.boardId === String(board.id) }"
            >
              <span class="text-sm truncate">{{ board.name }}</span>
              <span
                class="text-xs px-2 py-0.5 rounded flex-shrink-0"
                :class="getBoardTypeBadgeClass(board.type)"
              >
                {{ board.type }}
              </span>
            </router-link>

            <!-- Create New Board -->
            <button
              @click="openCreateBoardModal"
              class="w-full flex items-center gap-2 px-4 py-2 text-sm text-blue-400 hover:bg-gray-700 rounded-lg transition-colors"
            >
              <PlusIcon className="w-4 h-4" />
              <span>New Board</span>
            </button>
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
          class="absolute bottom-full left-0 right-0 mb-2 rounded-lg shadow-xl border border-gray-600 py-2 z-50 bg-gray-700"
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
        class="w-full px-3 py-2 mb-4 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
        @keyup.enter="handleCreateProject"
      />
      <textarea
        v-model="newProjectDescription"
        placeholder="Project description (optional)"
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none min-h-[100px]"
      />
    </Modal>

    <!-- Add Board Modal -->
    <Modal
      :show="showAddBoardModal"
      title="Create New Board"
      submitText="Create"
      @close="showAddBoardModal = false"
      @submit="handleCreateBoard"
    >
      <input
        v-model="newBoardName"
        type="text"
        placeholder="Board name"
        class="w-full px-3 py-2 mb-4 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
        @keyup.enter="handleCreateBoard"
      />
      <select
        v-model="newBoardType"
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
      >
        <option value="Kanban">Kanban</option>
        <option value="Scrum">Scrum</option>
        <option value="Backlog">Backlog</option>
      </select>
    </Modal>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { FolderIcon, PlusIcon, ClipboardIcon } from '@/components/icons'
import Modal from '@/components/common/Modal.vue'
import type { Project } from '@/types/Project'
import { useAuthStore } from '@/stores/auth'
import { useBoardStore } from '@/stores/boards'
import { useProjectStore } from '@/stores/projects'
import { storeToRefs } from 'pinia'
import { useDropdown } from '@/composables/useDropdown'
import { useClickOutside } from '@/composables/useClickOutside'

export default defineComponent({
  name: 'SidebarComponent',
  components: {
    FolderIcon,
    PlusIcon,
    ClipboardIcon,
    Modal
  },
  setup() {
    const route = useRoute()
    const router = useRouter()
    const authStore = useAuthStore()
    const boardStore = useBoardStore()
    const projectStore = useProjectStore()
    const { projects } = storeToRefs(projectStore)
    const selectedProjectId = ref<number | null>(null)
    const showAddProjectModal = ref(false)
    const showAddBoardModal = ref(false)
    const newProjectName = ref('')
    const newProjectDescription = ref('')
    const newBoardName = ref('')
    const newBoardType = ref('Kanban')
    const isBoardsExpanded = ref(true)

    // Dropdown management
    const projectSelectorRef = ref<HTMLElement | null>(null)
    const userMenuRef = ref<HTMLElement | null>(null)
    const projectDropdown = useDropdown()
    const userMenuDropdown = useDropdown()

    useClickOutside(projectSelectorRef, projectDropdown.close)
    useClickOutside(userMenuRef, userMenuDropdown.close)

    const currentProjectName = computed(() => {
      const project = projects.value.find(p => p.id === selectedProjectId.value)
      return project?.name || ''
    })

    const isOnProjectBoard = computed(() => {
      return route.path.includes('/projects/') && route.path.includes('/boards/')
    })

    const userInitials = computed(() => {
      const name = authStore.user?.name || ''
      return name
        .split(' ')
        .map(n => n[0])
        .join('')
        .toUpperCase()
        .slice(0, 2) || 'U'
    })

    const handleLogout = () => {
      authStore.logout()
      router.push('/login')
    }

    const toggleBoards = () => {
      isBoardsExpanded.value = !isBoardsExpanded.value
    }

    const getBoardTypeBadgeClass = (type: string) => {
      const classes: Record<string, string> = {
        'Scrum': 'bg-green-900 text-green-200',
        'Kanban': 'bg-blue-900 text-blue-200',
        'Backlog': 'bg-purple-900 text-purple-200'
      }
      return classes[type] || 'bg-gray-600 text-gray-300'
    }

    const fetchProjects = async () => {
      try {
        await projectStore.fetchProjects()

        if (route.params.projectId) {
          selectedProjectId.value = Number(route.params.projectId)
        } else if (projects.value.length > 0 && !selectedProjectId.value) {
          selectedProjectId.value = projects.value[0]!.id
        }
      } catch (error) {
        console.error('Failed to fetch projects:', error)
      }
    }

    const handleProjectChange = async (projectId: number) => {
      projectDropdown.close()
      
      if (projectId === selectedProjectId.value) {
        return
      }

      selectedProjectId.value = projectId

      // Fetch boards for the new project
      await boardStore.fetchBoards(projectId)

      // Navigate to first board or backlog
      if (boardStore.boards.length > 0) {
        const firstBoard = boardStore.boards[0]
        router.push(`/projects/${projectId}/boards/${firstBoard!.id}`)
      } else {
        router.push(`/projects/${projectId}/backlog`)
      }
    }

    const openCreateProjectModal = () => {
      projectDropdown.close()
      showAddProjectModal.value = true
    }

    const openCreateBoardModal = () => {
      showAddBoardModal.value = true
    }

    const handleCreateProject = async () => {
      if (!newProjectName.value.trim()) {
        alert('Project name is required')
        return
      }

      try {
        const newProject = await projectStore.createProject({
          name: newProjectName.value,
          description: newProjectDescription.value
        })

        newProjectName.value = ''
        newProjectDescription.value = ''
        showAddProjectModal.value = false

        if (newProject && newProject.boards && newProject.boards.length > 0) {
          selectedProjectId.value = newProject.id
          await boardStore.fetchBoards(newProject.id)
          const firstBoard = newProject.boards[0]
          router.push(`/projects/${newProject.id}/boards/${firstBoard!.id}`)
        }
      } catch (error) {
        console.error('Failed to create project:', error)
        alert('Failed to create project')
      }
    }

    const handleCreateBoard = async () => {
      if (!newBoardName.value.trim()) {
        alert('Board name is required')
        return
      }

      if (!selectedProjectId.value) {
        alert('Please select a project first')
        return
      }

      try {
        const newBoard = await boardStore.createBoard(selectedProjectId.value, {
          name: newBoardName.value,
          type: newBoardType.value,
          projectId: selectedProjectId.value
        })
        
        newBoardName.value = ''
        newBoardType.value = 'Kanban'
        showAddBoardModal.value = false
        
        // Navigate to the new board
        router.push(`/projects/${selectedProjectId.value}/boards/${newBoard.id}`)
      } catch (error) {
        console.error('Failed to create board:', error)
        alert('Failed to create board')
      }
    }

    watch(() => route.params.projectId, async (newProjectId) => {
      if (newProjectId) {
        selectedProjectId.value = Number(newProjectId)
        await boardStore.fetchBoards(Number(newProjectId))
      }
    }, { immediate: true })

    onMounted(async () => {
      await fetchProjects()
      if (selectedProjectId.value) {
        await boardStore.fetchBoards(selectedProjectId.value)
      }
    })

    return {
      route,
      authStore,
      boardStore,
      projects,
      selectedProjectId,
      currentProjectName,
      isOnProjectBoard,
      userInitials,
      showAddProjectModal,
      showAddBoardModal,
      showProjectDropdown: projectDropdown.isOpen,
      showUserMenu: userMenuDropdown.isOpen,
      projectSelectorRef,
      userMenuRef,
      newProjectName,
      newProjectDescription,
      newBoardName,
      newBoardType,
      projectDropdown,
      userMenuDropdown,
      isBoardsExpanded,
      toggleBoards,
      getBoardTypeBadgeClass,
      handleProjectChange,
      handleLogout,
      openCreateProjectModal,
      openCreateBoardModal,
      handleCreateProject,
      handleCreateBoard,
      fetchProjects
    }
  }
})
</script>

<style scoped>
.nav-item {
  @apply flex items-center gap-3 px-4 py-3 rounded-lg text-gray-300 hover:bg-gray-700 hover:text-white transition-all mb-2 cursor-pointer;
}

.nav-item.active {
  @apply bg-gray-700 text-white;
  border-left: 3px solid #3B82F6;
  padding-left: calc(1rem - 3px);
}

.nav-item-nested {
  @apply flex items-center justify-between px-4 py-2 rounded-lg text-gray-400 hover:bg-gray-700 hover:text-white transition-all cursor-pointer;
}

.nav-item-nested.active {
  @apply bg-gray-700 text-white;
  border-left: 3px solid #22C55E;
  padding-left: calc(1rem - 3px);
}

.rotate-90 {
  transform: rotate(90deg);
}

.rotate-180 {
  transform: rotate(180deg);
}
</style>