import { ref } from 'vue';
import { getProject, createTask, updateTask, deleteTask } from '@/services/api';
import type { Project, TaskItem, TaskCreate } from '@/types/Project';

export function useTasks(projectId: number) {
  const project = ref<Project>();
  const tasks = ref<TaskItem[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const fetchTasks = async () => {
    loading.value = true;
    error.value = null;
    try {
      const res = await getProject(projectId);
      project.value = res.data;
      tasks.value = res.data?.tasks || [];
    } catch (err) {
      error.value = 'Failed to fetch tasks';
      console.error(err);
    } finally {
      loading.value = false;
    }
  };

  const addTask = async (task: TaskCreate) => {
    try {
      await createTask(task);
      await fetchTasks();
    } catch (err) {
      error.value = 'Failed to create task';
      console.error(err);
      throw err;
    }
  };

  const modifyTask = async (id: number, task: TaskItem) => {
    try {
      await updateTask(id, task);
      await fetchTasks();
    } catch (err) {
      error.value = 'Failed to update task';
      console.error(err);
      throw err;
    }
  };

  const removeTask = async (id: number) => {
    try {
      await deleteTask(id);
      await fetchTasks();
    } catch (err) {
      error.value = 'Failed to delete task';
      console.error(err);
      throw err;
    }
  };

  const updateTaskStatus = async (task: TaskItem, newStatus: string) => {
    const updatedTask = { ...task, status: newStatus };
    await modifyTask(task.id, updatedTask);
  };

  return {
    project,
    tasks,
    loading,
    error,
    fetchTasks,
    addTask,
    modifyTask,
    removeTask,
    updateTaskStatus
  };
}