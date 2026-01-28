<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-gray-800 mb-2">Dashboard</h1>
      <p class="text-gray-600">Overview of your projects and recent activity</p>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center gap-2 mt-8">
      <LoadingIcon className="h-5 w-5 text-blue-600" />
      <span>Loading dashboard...</span>
    </div>

    <!-- Projects Overview -->
    <div v-else>
      <!-- Stats Cards -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        <div class="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-600 mb-1">Total Projects</p>
              <p class="text-3xl font-bold text-gray-800">{{ projects.length }}</p>
            </div>
            <FolderIcon className="w-12 h-12 text-blue-500" />
          </div>
        </div>

        <div class="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-600 mb-1">Total Boards</p>
              <p class="text-3xl font-bold text-gray-800">{{ totalBoards }}</p>
            </div>
            <ClipboardIcon className="w-12 h-12 text-green-500" />
          </div>
        </div>

        <div class="bg-white rounded-lg shadow-sm p-6 border border-gray-200">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm text-gray-600 mb-1">Total WorkItems</p>
              <p class="text-3xl font-bold text-gray-800">{{ totalWorkItems }}</p>
            </div>
            <svg class="w-12 h-12 text-purple-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/>
            </svg>
          </div>
        </div>
      </div>

      <!-- Projects List -->
      <div class="bg-white rounded-lg shadow-sm border border-gray-200">
        <div class="p-6 border-b border-gray-200">
          <h2 class="text-xl font-bold text-gray-800">Your Projects</h2>
        </div>

        <div v-if="projects.length === 0" class="p-8">
          <EmptyState
            :icon="FolderIcon"
            message="No projects yet. Create your first project from the sidebar!"
          />
        </div>

        <div v-else class="divide-y divide-gray-200">
          <div
            v-for="project in projects"
            :key="project.id"
            @click="navigateToProject(project.id)"
            class="p-6 hover:bg-gray-50 cursor-pointer transition-all group"
          >
            <div class="flex items-start justify-between">
              <div class="flex-1">
                <div class="flex items-center gap-3 mb-2">
                  <h3 class="text-lg font-semibold text-gray-800 group-hover:text-blue-600 transition">
                    {{ project.name }}
                  </h3>
                  <span class="text-xs bg-blue-100 text-blue-700 px-2 py-1 rounded-full">
                    {{ project.boards?.length || 0 }} boards
                  </span>
                </div>
                <p v-if="project.description" class="text-gray-600 text-sm mb-3">
                  {{ project.description }}
                </p>
                <div class="flex items-center gap-4 text-sm text-gray-500">
                  <span class="flex items-center gap-1">
                    <ClipboardIcon className="w-4 h-4" />
                    {{ getProjectWorkItemCount(project) }} WorkItems
                  </span>
                </div>
              </div>
              <button
                class="opacity-0 group-hover:opacity-100 transition-opacity px-4 py-2 bg-blue-50 text-blue-700 rounded-lg hover:bg-blue-100 font-medium"
              >
                View Boards →
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import type { Project } from '@/types/Project';
import { LoadingIcon, FolderIcon, ClipboardIcon } from '@/components/icons';
import EmptyState from '@/components/common/EmptyState.vue';
import { useProjectStore } from '@/stores/projects';
import { storeToRefs } from 'pinia';

export default defineComponent({
  name: 'SummaryView',
  components: {
    LoadingIcon,
    FolderIcon,
    ClipboardIcon,
    EmptyState
  },
  setup() {
    const router = useRouter();
    const projectStore = useProjectStore();
    const { projects, loading } = storeToRefs(projectStore);

    const totalBoards = computed(() => {
      return projects.value.reduce((sum, project) => sum + (project.boards?.length || 0), 0);
    });

    const totalWorkItems = computed(() => {
      return projects.value.reduce((sum, project) => {
        const projectWorkItems = project.boards?.reduce((boardSum, board) => {
          return boardSum + (board.workItems?.length || 0);
        }, 0) || 0;
        return sum + projectWorkItems;
      }, 0);
    });

    const getProjectWorkItemCount = (project: Project) => {
      return project.boards?.reduce((sum, board) => {
        return sum + (board.workItems?.length || 0);
      }, 0) || 0;
    };

    const navigateToProject = (projectId: number) => {
      const project = projects.value.find(p => p.id === projectId);

      if (project && project.boards && project.boards.length > 0) {
        // Navigate to the first board of the project
        const firstBoard = project.boards[0];
        router.push(`/projects/${projectId}/boards/${firstBoard!.id}`);
      } else {
        alert('This project has no boards. Please create a board first.');
      }
    };

    onMounted(async () => {
      await projectStore.fetchProjects();
    });

    return {
      projects,
      loading,
      totalBoards,
      totalWorkItems,
      getProjectWorkItemCount,
      navigateToProject,
      FolderIcon
    };
  }
});
</script>