<template>
  <Teleport to="body">
    <div v-if="true" class="modal-overlay" @click.self="$emit('close')">
      <div class="modal max-w-2xl">
        <h2 class="text-2xl font-bold mb-6">{{ isEditing ? 'Edit WorkItem' : 'Create WorkItem' }}</h2>
        
        <div class="space-y-4">
          <!-- Title -->
           <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Type</label>
              <select v-model="form.type" class="input">
                <option v-for="workItemTypeName in WORKITEM_TYPES" :key="workItemTypeName">{{ workItemTypeName }}</option>
              </select>
            </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Title *</label>
            <input
              v-model="form.title"
              type="text"
              placeholder="WorkItem title"
              class="input"
              required
            />
          </div>

          <!-- Description -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
            <textarea
              v-model="form.description"
              placeholder="WorkItem description..."
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
            <!-- <div v-if="projects.length">
              <label class="block text-sm font-medium text-gray-700 mb-1">Project</label>
              <select v-model="form.projectId" class="input">
                <option v-for="project in projects" :key="project.id" :value="project.id">
                  {{ project.name }}
                </option>
              </select>
            </div> -->

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
import type { WorkItem } from '@/types/Project';
import { PRIORITIES, STATUSES, WORKITEM_TYPES } from '@/types/Project';
import { ClockIcon, CalendarIcon } from '@/components/icons';

export default defineComponent({
  name: 'WorkItemModal',
  components: {
    ClockIcon,
    CalendarIcon
  },
  props: {
    workItem: {
      type: Object as PropType<WorkItem | null>,
      default: null
    },
    boardId: {
      type: Number,
      required: true
    },
    defaultStatus: {
      type: String,
      default: 'To Do'
    }
  },
  emits: ['close', 'save'],
  setup(props, { emit }) {
    const isEditing = ref(!!props.workItem);
    
    const form = ref({
      id: props.workItem?.id || 0,
      title: props.workItem?.title || '',
      type: props.workItem?.type || 'WorkItem',
      description: props.workItem?.description || '',
      status: props.workItem?.status || props.defaultStatus || 'To Do',
      priority: props.workItem?.priority || 'Medium',
      dueDate: props.workItem?.dueDate ? props.workItem.dueDate.split('T')[0] : '',
      estimatedHours: props.workItem?.estimatedHours || null,
      actualHours: props.workItem?.actualHours || null,
      boardId: props.boardId
    });

    watch(() => props.workItem, (newWorkItem) => {
      if (newWorkItem) {
        isEditing.value = true;
        form.value = {
          id: newWorkItem.id,
          title: newWorkItem.title,
          type: newWorkItem.priority,
          description: newWorkItem.description || '',
          status: newWorkItem.status,
          priority: newWorkItem.priority,
          dueDate: newWorkItem.dueDate ? newWorkItem.dueDate.split('T')[0] : '',
          estimatedHours: newWorkItem.estimatedHours || null,
          actualHours: newWorkItem.actualHours || null,
          boardId: props.boardId
        };
      } else {
        isEditing.value = false;
        form.value.status = props.defaultStatus || 'To Do';
      }
    });

    const handleSave = () => {
      if (!form.value.title.trim()) {
        alert('Title is required');
        return;
      }

      const workItemData: WorkItem = {
        id: form.value.id,
        title: form.value.title,
        description: form.value.description || undefined,
        status: form.value.status,
        priority: form.value.priority,
        type: form.value.type!,
        dueDate: form.value.dueDate || undefined,
        estimatedHours: form.value.estimatedHours || undefined,
        actualHours: form.value.actualHours || undefined,
        boardId: props.boardId,
        createdAt: props.workItem?.createdAt || new Date().toISOString()
      };

      emit('save', workItemData);
    };

    return {
      isEditing,
      form,
      PRIORITIES,
      STATUSES,
      WORKITEM_TYPES,
      handleSave
    };
  }
});
</script>