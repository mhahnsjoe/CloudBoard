<template>
  <Teleport to="body">
    <div v-if="true" class="modal-overlay" @click.self="$emit('close')">
      <div class="modal max-w-2xl">
        <h2 class="text-2xl font-bold mb-6">{{ isEditing ? 'Edit Task' : 'Create Task' }}</h2>
        
        <div class="space-y-4">
          <!-- Title -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Title *</label>
            <input
              v-model="form.title"
              type="text"
              placeholder="Task title"
              class="input"
              required
            />
          </div>

          <!-- Description -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
            <textarea
              v-model="form.description"
              placeholder="Task description..."
              class="input min-h-[100px]"
              rows="4"
            />
          </div>

          <!-- Row: Status, Priority -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <select v-model="form.status" class="input">
                <option v-for="status in STATUSES" :key="status">{{ status }}</option>
              </select>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Priority</label>
              <select v-model="form.priority" class="input">
                <option v-for="priority in PRIORITIES" :key="priority">{{ priority }}</option>
              </select>
            </div>
          </div>

          <!-- Row: Project, Due Date -->
          <div class="grid grid-cols-2 gap-4">
            <div v-if="projects.length">
              <label class="block text-sm font-medium text-gray-700 mb-1">Project</label>
              <select v-model="form.projectId" class="input">
                <option v-for="project in projects" :key="project.id" :value="project.id">
                  {{ project.name }}
                </option>
              </select>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1 flex items-center gap-1">
                <CalendarIcon className="w-4 h-4" />
                Due Date
              </label>
              <input
                v-model="form.dueDate"
                type="date"
                class="input"
              />
            </div>
          </div>

          <!-- Row: Time Tracking -->
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1 flex items-center gap-1">
                <ClockIcon className="w-4 h-4" />
                Estimated Hours
              </label>
              <input
                v-model.number="form.estimatedHours"
                type="number"
                step="0.5"
                min="0"
                placeholder="0.0"
                class="input"
              />
            </div>

            <div v-if="isEditing">
              <label class="block text-sm font-medium text-gray-700 mb-1 flex items-center gap-1">
                <ClockIcon className="w-4 h-4" />
                Actual Hours
              </label>
              <input
                v-model.number="form.actualHours"
                type="number"
                step="0.5"
                min="0"
                placeholder="0.0"
                class="input"
              />
            </div>
          </div>
        </div>

        <!-- Actions -->
        <div class="modal-actions mt-6">
          <button class="btn btn-light" @click="$emit('close')">Cancel</button>
          <button class="btn btn-primary" @click="handleSave">
            {{ isEditing ? 'Update' : 'Create' }}
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script lang="ts">
import { defineComponent, ref, watch, type PropType } from 'vue';
import type { TaskItem, Project } from '@/types/Project';
import { PRIORITIES, STATUSES } from '@/types/Project';
import { ClockIcon, CalendarIcon } from '@/components/icons';

export default defineComponent({
  name: 'TaskModal',
  components: {
    ClockIcon,
    CalendarIcon
  },
  props: {
    task: {
      type: Object as PropType<TaskItem | null>,
      default: null
    },
    projects: {
      type: Array as PropType<Project[]>,
      default: () => []
    },
    defaultProjectId: {
      type: Number,
      default: null
    }
  },
  emits: ['close', 'save'],
  setup(props, { emit }) {
    const isEditing = ref(!!props.task);
    
    const form = ref({
      id: props.task?.id || 0,
      title: props.task?.title || '',
      description: props.task?.description || '',
      status: props.task?.status || 'To Do',
      priority: props.task?.priority || 'Medium',
      dueDate: props.task?.dueDate ? props.task.dueDate.split('T')[0] : '',
      estimatedHours: props.task?.estimatedHours || null,
      actualHours: props.task?.actualHours || null,
      projectId: props.task?.projectId || props.defaultProjectId || (props.projects[0]?.id || 0)
    });

    watch(() => props.task, (newTask) => {
      if (newTask) {
        isEditing.value = true;
        form.value = {
          id: newTask.id,
          title: newTask.title,
          description: newTask.description || '',
          status: newTask.status,
          priority: newTask.priority,
          dueDate: newTask.dueDate ? newTask.dueDate.split('T')[0] : '',
          estimatedHours: newTask.estimatedHours || null,
          actualHours: newTask.actualHours || null,
          projectId: newTask.projectId
        };
      }
    });

    const handleSave = () => {
      if (!form.value.title.trim()) {
        alert('Title is required');
        return;
      }

      const taskData: TaskItem = {
        id: form.value.id,
        title: form.value.title,
        description: form.value.description || undefined,
        status: form.value.status,
        priority: form.value.priority,
        dueDate: form.value.dueDate || undefined,
        estimatedHours: form.value.estimatedHours || undefined,
        actualHours: form.value.actualHours || undefined,
        projectId: form.value.projectId,
        createdAt: props.task?.createdAt || new Date().toISOString()
      };

      emit('save', taskData);
    };

    return {
      isEditing,
      form,
      PRIORITIES,
      STATUSES,
      handleSave
    };
  }
});
</script>