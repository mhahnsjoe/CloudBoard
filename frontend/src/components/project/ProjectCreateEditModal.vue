<template>
  <Modal
    :show="show"
    :title="isEditing ? 'Edit Project' : 'Create Project'"
    :submitText="isEditing ? 'Update' : 'Create'"
    :submitDisabled="!isValid"
    @close="$emit('close')"
    @submit="handleSubmit"
  >
    <div class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Project Name *</label>
        <input
          v-model="form.name"
          type="text"
          placeholder="e.g., My Awesome Project"
          class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
          required
        />
      </div>
       <div>
        <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
        <textarea
          v-model="form.description"
          placeholder="Optional description"
          class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
          rows="3"
        ></textarea>
      </div>
    </div>
  </Modal>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import type { Project } from '@/types/Project'
import Modal from '@/components/common/Modal.vue'

const props = defineProps<{
  show: boolean
  project?: Project | null
}>()

const emit = defineEmits<{
  (e: 'close'): void
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  (e: 'submit', form: any): void
}>()

const form = ref({
  id: undefined as number | undefined,
  name: '',
  description: ''
})

const isEditing = computed(() => !!props.project)
const isValid = computed(() => !!form.value.name.trim())

watch(() => props.show, (newVal) => {
  if (newVal) {
    if (props.project) {
        form.value = {
            id: props.project.id,
            name: props.project.name,
            description: props.project.description || ''
        }
    } else {
        form.value = {
            id: undefined,
            name: '',
            description: ''
        }
    }
  }
})

const handleSubmit = () => {
    emit('submit', form.value)
}
</script>
