<!-- <template>
  <div class="min-h-screen bg-gray-50 p-8">
    <div class="flex flex-col gap-4 mb-8">
      <h1 class="text-3xl font-bold text-gray-800">Work Items</h1>
      
      <!-- Filters 
      <div class="flex gap-4 items-center">
        <SearchBar 
          v-model="searchQuery" 
          placeholder="Search tasks..."
          class="flex-1 max-w-md"
        />
        
        <select 
          v-model="statusFilter"
          class="px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
        >
          <option value="">All Statuses</option>
          <option v-for="status in STATUSES" :key="status">{{ status }}</option>
        </select>

        <select 
          v-model="priorityFilter"
          class="px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
        >
          <option value="">All Priorities</option>
          <option v-for="priority in PRIORITIES" :key="priority">{{ priority }}</option>
        </select>
      </div>
    </div>

    <!-- Tasks Table 
    <div v-if="loading" class="flex items-center justify-center gap-2 mt-8">
      <LoadingIcon className="h-5 w-5 text-blue-600" />
      <span>Loading tasks...</span>
    </div>

    <div v-else-if="filteredTasks.length" class="bg-white rounded-lg shadow-sm border border-gray-200">
      <table class="w-full">
        <thead class="bg-gray-50 border-b border-gray-200">
          <tr>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Task
            </th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Project
            </th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Status
            </th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Priority
            </th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Due Date
            </th>
            <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
              Actions
            </th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <tr 
            v-for="task in filteredTasks" 
            :key="task.id"
            class="hover:bg-gray-50 transition cursor-pointer"
            @click="navigateToProject(task.projectId)"
          >
            <td class="px-6 py-4">
              <div class="font-medium text-gray-900">{{ task.title }}</div>
              <div v-if="task.description" class="text-sm text-gray-500 truncate max-w-md">
                {{ task.description }}
              </div>
            </td>
            <td class="px-6 py-4 text-sm text-gray-500">
              {{ getProjectName(task.projectId) }}
            </td>
            <td class="px-6 py-4">
              <span :class="getStatusBadgeClass(task.status)" class="px-2 py-1 text-xs font-medium rounded-full">
                {{ task.status }}
              </span>
            </td>
            <td class="px-6 py-4">
              <span :class="getPriorityBadgeClass(task.priority)" class="px-2 py-1 text-xs font-medium rounded-full">
                {{ task.priority }}
              </span>
            </td>
            <td class="px-6 py-4 text-sm text-gray-500">
              <div v-if="task.dueDate" class="flex items-center gap-1">
                <CalendarIcon className="w-4 h-4" />
                {{ formatDate(task.dueDate) }}
              </div>
              <span v-else class="text-gray-400">-</span>
            </td>
            <td class="px-6 py-4 text-right">
              <button
                @click.stop="editTask(task)"
                class="text-blue-600 hover:text-blue-800 mr-3"
              >
                <EditIcon className="w-4 h-4" />
              </button>
              <button
                @click.stop="handleDelete(task.id)"
                class="text-red-600 hover:text-red-800"
              >
                <DeleteIcon className="w-4 h-4" />
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <EmptyState
      v-else
      :icon="ClipboardIcon"
      message="No tasks found. Create tasks in your projects!"
    />

    <!-- Edit Modal 
    <TaskModal
      v-if="showModal"
      :task="selectedTask"
      :projects="projects"
      @close="showModal = false"
      @save="handleSave"
    />
  </div>
</template> -->

<!-- <script lang="ts">
import { defineComponent, ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { getTasks, getProjects, updateTask, deleteTask } from '@/services/api';
import type { TaskItem, Project } from '@/types/Project';
import { PRIORITIES, STATUSES } from '@/types/Project';

import SearchBar from './project/SearchBar.vue';
import EmptyState from './common/EmptyState.vue';
import TaskModal from './task/TaskModal.vue';

import { LoadingIcon, EditIcon, DeleteIcon, ClipboardIcon, CalendarIcon } from './icons';

export default defineComponent({
  name: 'WorkItemsView',
  components: {
    SearchBar,
    EmptyState,
    TaskModal,
    LoadingIcon,
    EditIcon,
    DeleteIcon,
    ClipboardIcon,
    CalendarIcon
  },
  setup() {
    const router = useRouter();
    const tasks = ref<TaskItem[]>([]);
    const projects = ref<Project[]>([]);
    const loading = ref(false);
    const searchQuery = ref('');
    const statusFilter = ref('');
    const priorityFilter = ref('');
    const showModal = ref(false);
    const selectedTask = ref<TaskItem | null>(null);

    const filteredTasks = computed(() => {
      let result = tasks.value;

      if (searchQuery.value) {
        const query = searchQuery.value.toLowerCase();
        result = result.filter(t => 
          t.title.toLowerCase().includes(query) ||
          t.description?.toLowerCase().includes(query)
        );
      }

      if (statusFilter.value) {
        result = result.filter(t => t.status === statusFilter.value);
      }

      if (priorityFilter.value) {
        result = result.filter(t => t.priority === priorityFilter.value);
      }

      return result;
    });

    const fetchData = async () => {
      loading.value = true;
      try {
        const [tasksRes, projectsRes] = await Promise.all([
          getTasks(),
          getProjects()
        ]);
        tasks.value = tasksRes.data;
        projects.value = projectsRes.data;
      } catch (error) {
        console.error('Failed to fetch data:', error);
      } finally {
        loading.value = false;
      }
    };

    const getProjectName = (projectId: number) => {
      const project = projects.value.find(p => p.id === projectId);
      return project?.name || 'Unknown';
    };

    const getStatusBadgeClass = (status: string) => {
      const classes: Record<string, string> = {
        'To Do': 'bg-gray-100 text-gray-700',
        'In Progress': 'bg-blue-100 text-blue-700',
        'Done': 'bg-green-100 text-green-700'
      };
      return classes[status] || 'bg-gray-100 text-gray-700';
    };

    const getPriorityBadgeClass = (priority: string) => {
      const classes: Record<string, string> = {
        'Low': 'bg-gray-100 text-gray-600',
        'Medium': 'bg-blue-100 text-blue-700',
        'High': 'bg-orange-100 text-orange-700',
        'Critical': 'bg-red-100 text-red-700'
      };
      return classes[priority] || 'bg-gray-100 text-gray-600';
    };

    const formatDate = (dateString: string) => {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
    };

    const navigateToProject = (projectId: number) => {
      router.push(`/projects/${projectId}`);
    };

    const editTask = (task: TaskItem) => {
      selectedTask.value = task;
      showModal.value = true;
    };

    const handleSave = async (updatedTask: TaskItem) => {
      try {
        await updateTask(updatedTask.id, updatedTask);
        await fetchData();
        showModal.value = false;
      } catch (error) {
        console.error('Failed to update task:', error);
      }
    };

    const handleDelete = async (id: number) => {
      if (confirm('Are you sure you want to delete this task?')) {
        try {
          await deleteTask(id);
          await fetchData();
        } catch (error) {
          console.error('Failed to delete task:', error);
        }
      }
    };

    onMounted(fetchData);

    return {
      tasks,
      projects,
      loading,
      searchQuery,
      statusFilter,
      priorityFilter,
      filteredTasks,
      showModal,
      selectedTask,
      PRIORITIES,
      STATUSES,
      getProjectName,
      getStatusBadgeClass,
      getPriorityBadgeClass,
      formatDate,
      navigateToProject,
      editTask,
      handleSave,
      handleDelete,
      ClipboardIcon
    };
  }
});
</script> -->