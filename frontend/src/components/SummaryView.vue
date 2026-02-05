<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <!-- Header -->
    <div class="mb-8 flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-bold text-gray-800 mb-2">Dashboard</h1>
        <p class="text-gray-600">Overview of your team, projects and activity</p>
      </div>
      <div class="flex items-center gap-3">
        <span v-if="currentTeam" class="text-sm text-gray-500">
          Team: <strong>{{ currentTeam.name }}</strong>
        </span>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center gap-2 mt-8">
      <LoadingIcon className="h-5 w-5 text-blue-600" />
      <span>Loading dashboard...</span>
    </div>

    <div v-else>
      <!-- Stats Row -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <StatCard title="Total Projects" :value="projects.length" icon="folder" color="blue" />
        <StatCard title="Total Boards" :value="totalBoards" icon="clipboard" color="green" />
        <StatCard title="Work Items" :value="totalWorkItems" icon="task" color="purple" />
        <StatCard title="Team Members" :value="currentTeam?.members?.length || 0" icon="users" color="orange" />
      </div>

      <!-- Two Column Layout -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- Left Column: Projects & Boards (2/3 width) -->
        <div class="lg:col-span-2 space-y-8">
          <!-- Projects Section -->
          <section class="bg-white rounded-lg shadow-sm border border-gray-200">
            <div class="p-6 border-b border-gray-200 flex items-center justify-between">
              <h2 class="text-xl font-bold text-gray-800">Projects</h2>
              <button
                v-if="isAdmin"
                @click="showProjectModal = true"
                class="text-sm bg-blue-600 text-white px-3 py-1.5 rounded-lg hover:bg-blue-700 flex items-center gap-1"
              >
                <PlusIcon className="w-4 h-4" />
                New Project
              </button>
            </div>

            <div v-if="projects.length === 0" class="p-8 text-center">
              <FolderIcon className="w-12 h-12 text-gray-300 mx-auto mb-3" />
              <p class="text-gray-500">No projects yet. Create your first project to get started!</p>
            </div>

            <div v-else class="divide-y divide-gray-100">
              <div
                v-for="project in projects"
                :key="project.id"
                class="p-4 hover:bg-gray-50 transition-colors"
              >
                <div class="flex items-center justify-between">
                  <div class="flex items-center gap-3 min-w-0 flex-1">
                    <FolderIcon className="w-5 h-5 text-blue-500 flex-shrink-0" />
                    <div class="min-w-0">
                      <router-link
                        :to="`/projects/${project.id}/backlog`"
                        class="font-medium text-gray-900 hover:text-blue-600"
                      >
                        {{ project.name }}
                      </router-link>
                      <p class="text-sm text-gray-500">
                        {{ project.boards?.length || 0 }} boards
                      </p>
                    </div>
                  </div>
                  <!-- Admin Actions -->
                  <div v-if="isAdmin" class="flex items-center gap-2">
                    <button
                      @click="editProject(project)"
                      class="p-1.5 text-gray-400 hover:text-blue-600 rounded"
                      title="Edit project"
                    >
                      <EditIcon className="w-4 h-4" />
                    </button>
                    <button
                      @click="confirmDeleteProject(project)"
                      class="p-1.5 text-gray-400 hover:text-red-600 rounded"
                      title="Delete project"
                    >
                      <DeleteIcon className="w-4 h-4" />
                    </button>
                  </div>
                </div>

                <!-- Boards under project (collapsible) -->
                <div v-if="project.boards?.length" class="mt-3 ml-8 space-y-2">
                  <div
                    v-for="board in project.boards"
                    :key="board.id"
                    class="flex items-center justify-between text-sm p-2 rounded hover:bg-gray-100"
                  >
                    <router-link
                      :to="`/projects/${project.id}/boards/${board.id}`"
                      class="flex items-center gap-2 text-gray-700 hover:text-blue-600"
                    >
                      <ClipboardIcon className="w-4 h-4" />
                      {{ board.name }}
                      <span class="text-xs text-gray-400">({{ board.type }})</span>
                    </router-link>
                    <!-- Board admin actions -->
                    <div v-if="isAdmin" class="flex items-center gap-1">
                      <button
                        @click="editBoard(project.id, board)"
                        class="p-1 text-gray-400 hover:text-blue-600 rounded"
                        title="Edit board"
                      >
                        <EditIcon className="w-3 h-3" />
                      </button>
                      <button
                        @click="confirmDeleteBoard(project.id, board)"
                        class="p-1 text-gray-400 hover:text-red-600 rounded"
                        title="Delete board"
                      >
                        <DeleteIcon className="w-3 h-3" />
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </section>
        </div>

        <!-- Right Column: Team Management (1/3 width) -->
        <div class="space-y-6">
          <!-- Team Members Card -->
          <section class="bg-white rounded-lg shadow-sm border border-gray-200">
            <div class="p-4 border-b border-gray-200 flex items-center justify-between">
              <h3 class="font-semibold text-gray-800">Team Members</h3>
              <button
                v-if="isAdmin"
                @click="showInviteModal = true"
                class="text-sm text-blue-600 hover:text-blue-700 flex items-center gap-1"
              >
                <PlusIcon className="w-4 h-4" />
                Invite
              </button>
            </div>

            <div class="p-4 space-y-3 max-h-64 overflow-y-auto">
              <div
                v-for="member in currentTeam?.members"
                :key="member.userId"
                class="flex items-center justify-between"
              >
                <div class="flex items-center gap-2">
                  <UserAvatar :name="member.name" :userId="member.userId" size="sm" />
                  <div class="min-w-0">
                    <div class="text-sm font-medium text-gray-900 truncate">{{ member.name }}</div>
                    <div class="text-xs text-gray-500">{{ member.role }}</div>
                  </div>
                </div>
                <RoleBadge :role="member.role" />
              </div>
            </div>

            <!-- Pending Invitations -->
            <div v-if="currentTeam?.pendingInvitations?.length && isAdmin" class="border-t border-gray-100 p-4">
              <h4 class="text-sm font-medium text-gray-700 mb-2">Pending Invitations</h4>
              <div class="space-y-2">
                <div
                  v-for="invite in currentTeam.pendingInvitations"
                  :key="invite.id"
                  class="flex items-center justify-between text-sm"
                >
                  <span class="text-gray-600">{{ invite.email }}</span>
                  <button
                    @click="cancelInvitation(invite.id)"
                    class="text-red-500 hover:text-red-700"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            </div>
          </section>

          <!-- Team Settings (Admin Only) -->
          <section v-if="isAdmin" class="bg-white rounded-lg shadow-sm border border-gray-200">
            <div class="p-4 border-b border-gray-200">
              <h3 class="font-semibold text-gray-800">Team Settings</h3>
            </div>
            <div class="p-4 space-y-3">
              <button
                @click="showEditTeamModal = true"
                class="w-full text-left px-3 py-2 text-sm rounded hover:bg-gray-50 flex items-center gap-2"
              >
                <EditIcon className="w-4 h-4 text-gray-400" />
                Edit Team Name & Description
              </button>
              <button
                v-if="isOwner"
                @click="confirmDeleteTeam"
                class="w-full text-left px-3 py-2 text-sm text-red-600 rounded hover:bg-red-50 flex items-center gap-2"
              >
                <DeleteIcon className="w-4 h-4" />
                Delete Team
              </button>
            </div>
          </section>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <InviteMemberModal
      v-if="showInviteModal && currentTeam"
      :team-id="currentTeam.id"
      @close="showInviteModal = false"
      @invited="refreshTeam"
    />

    <EditTeamModal
      v-if="showEditTeamModal && currentTeam"
      :team="currentTeam"
      @close="showEditTeamModal = false"
      @updated="refreshTeam"
    />

    <!-- Project/Board modals -->
    <BoardCreateEditModal
      :show="showBoardModal"
      :board="selectedBoard"
      @close="closeBoardModal"
      @submit="handleSaveBoard"
    />

    <ProjectCreateEditModal
      :show="showProjectModal"
      :project="selectedProject"
      @close="closeProjectModal"
      @submit="handleSaveProject"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useProjectStore } from '@/stores/projects'
import { useTeamsStore } from '@/stores/teams'
import { useAuthStore } from '@/stores/auth'
import { useBoardStore } from '@/stores/boards'
import { useToast } from '@/composables/useToast'
import { storeToRefs } from 'pinia'
import type { Project, Board } from '@/types/Project'
import { 
  LoadingIcon, 
  FolderIcon, 
  ClipboardIcon, 
  PlusIcon,
  EditIcon,
  DeleteIcon
} from '@/components/icons'
import StatCard from '@/components/common/StatCard.vue'
import RoleBadge from '@/components/common/RoleBadge.vue'
import UserAvatar from '@/components/common/UserAvatar.vue'
import InviteMemberModal from '@/components/team/InviteMemberModal.vue'
import EditTeamModal from '@/components/team/EditTeamModal.vue'
import BoardCreateEditModal from '@/components/board/BoardCreateEditModal.vue'
import ProjectCreateEditModal from '@/components/project/ProjectCreateEditModal.vue'

const router = useRouter()
const projectStore = useProjectStore()
const teamsStore = useTeamsStore()
const authStore = useAuthStore()
const boardStore = useBoardStore()
const { success, error: toastError } = useToast()

const { projects, loading: projectsLoading } = storeToRefs(projectStore)
const { currentTeam, loading: teamsLoading } = storeToRefs(teamsStore)

const loading = computed(() => projectsLoading.value || teamsLoading.value)

const showInviteModal = ref(false)
const showEditTeamModal = ref(false)
const showProjectModal = ref(false)
const showBoardModal = ref(false)

const selectedProject = ref<Project | null>(null)
const selectedBoard = ref<Board | null>(null)
// When creating a board, we need to know for which project
const activeProjectId = ref<number | null>(null)

const isAdmin = computed(() => {
  return currentTeam.value?.currentUserRole === 'Admin' || currentTeam.value?.currentUserRole === 'Owner'
})

const isOwner = computed(() => {
  return currentTeam.value?.currentUserRole === 'Owner'
})

const totalBoards = computed(() => {
  return projects.value.reduce((sum, project) => sum + (project.boards?.length || 0), 0)
})

const totalWorkItems = computed(() => {
  return projects.value.reduce((sum, project) => {
    const projectWorkItems = project.boards?.reduce((boardSum, board) => {
      return boardSum + (board.workItems?.length || 0)
    }, 0) || 0
    return sum + projectWorkItems
  }, 0)
})

const refreshTeam = async () => {
    if (currentTeam.value) {
        await teamsStore.fetchTeam(currentTeam.value.id)
    }
}

const cancelInvitation = async (invitationId: number) => {
    if (confirm('Are you sure you want to cancel this invitation?') && currentTeam.value) {
        try {
            await teamsStore.cancelInvitation(currentTeam.value.id, invitationId)
            success('Invitation cancelled')
        } catch {
            toastError('Failed to cancel invitation')
        }
    }
}

const confirmDeleteTeam = async () => {
    if (confirm('Are you sure you want to delete this team? This action cannot be undone.') && currentTeam.value) {
        try {
            await teamsStore.deleteTeam(currentTeam.value.id)
            router.push('/teams') 
            success('Team deleted')
        } catch {
            toastError('Failed to delete team')
        }
    }
}

// Project Management


const editProject = (project: Project) => {
    selectedProject.value = project
    showProjectModal.value = true
}

const closeProjectModal = () => {
    showProjectModal.value = false
    selectedProject.value = null
}

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
const handleSaveProject = async (formData: any) => {
    try {
        if (formData.id) {
            await projectStore.updateProject(formData.id, formData)
            success('Project updated')
        } else {
             // Assuming createProject handles team association implicitly or we might need teamId
             // For now api.createProject takes name/desc. Backend might infer team from context or input.
             // Looking at api.createProject(project: ProjectCreate), ProjectCreate has teamId?
             // types/Project.ts check needed. Usually assumes current user/team context.
             // If ProjectCreate requires teamId, I should provide it.
             const projectData = { ...formData, teamId: currentTeam.value?.id }
             await projectStore.createProject(projectData)
             success('Project created')
        }
        await projectStore.fetchProjects()
        closeProjectModal()
    } catch {
        toastError('Failed to save project')
    }
}

const confirmDeleteProject = async (project: Project) => {
     if (confirm(`Are you sure you want to delete project "${project.name}"?`)) {
        try {
            await projectStore.deleteProject(project.id)
            await projectStore.fetchProjects()
            success('Project deleted')
        } catch {
            toastError('Failed to delete project')
        }
     }
}

// Board Management
// Note: We need a way to Create a board for a specific project.
// The UI in plan didn't explicitly show "Create Board" button per project, 
// but usually it's good to have inside the project card or general "New Board" which asks for project.
// In the template, I see "New Project" button but not "New Board" button explicitly, 
// likely "New Board" is absent in SummaryView plan, but Edit/Delete are present.
// However BoardDetailView allows creating boards.
// If I can't create board from SummaryView, that's fine for now, or I can add it.
// I'll stick to Edit/Delete as requested.

const editBoard = (projectId: number, board: Board) => {
    activeProjectId.value = projectId
    selectedBoard.value = board
    showBoardModal.value = true
}

const closeBoardModal = () => {
    showBoardModal.value = false
    selectedBoard.value = null
    activeProjectId.value = null
}

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
const handleSaveBoard = async (formData: any) => {
    try {
        if (formData.id && activeProjectId.value) {
            await boardStore.updateBoard(activeProjectId.value, formData.id, {
                name: formData.name,
                type: formData.type,
                projectId: activeProjectId.value,
                columns: formData.columns
            })
            success('Board updated')
            await projectStore.fetchProjects() // Refresh to update board list
        }
        closeBoardModal()
    } catch {
        // Check if current board was edited, might need refresh if passing props
        toastError('Failed to save board')
    }
}

const confirmDeleteBoard = async (projectId: number, board: Board) => {
    if (confirm(`Are you sure you want to delete board "${board.name}"?`)) {
        try {
            await boardStore.deleteBoard(projectId, board.id)
            success('Board deleted')
            await projectStore.fetchProjects()
        } catch {
            toastError('Failed to delete board')
        }
    }
}

onMounted(async () => {
    await projectStore.fetchProjects()
    if (teamsStore.selectedTeamId) {
        await teamsStore.fetchTeam(teamsStore.selectedTeamId)
    } else if (authStore.user) {
        // Try to fetch teams and select one?
        await teamsStore.fetchTeams()
        if (teamsStore.teams.length > 0) {
             teamsStore.selectTeam(teamsStore.teams[0]!.id)
        }
    }
})
</script>