<template>
  <div class="bg-white rounded-lg shadow-sm p-6">
    <h3 class="text-lg font-semibold mb-4 text-gray-900">Sprint Retrospective</h3>
    
    <div v-if="isEditing">
      <textarea 
        v-model="retrospective"
        class="w-full h-48 p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition-all"
        placeholder="What went well? What could be improved? Action items..."
      />
      <div class="flex justify-end gap-2 mt-4">
        <button 
          @click="cancel" 
          class="px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-md transition-colors"
        >
          Cancel
        </button>
        <button 
          @click="save" 
          class="px-4 py-2 text-sm bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
          :disabled="loading"
        >
          <span v-if="loading">Saving...</span>
          <span v-else>Save</span>
        </button>
      </div>
    </div>
    
    <div v-else>
      <div v-if="sprint.retrospective" class="prose prose-sm max-w-none text-gray-700 whitespace-pre-wrap">
        {{ sprint.retrospective }}
      </div>
      <div v-else class="text-gray-500 italic py-4">
        No retrospective recorded yet.
      </div>
      
      <div class="flex justify-end mt-4">
        <button 
          v-if="sprint.status === 'Completed' || sprint.status === 'Active'"
          @click="startEditing" 
          class="text-sm text-blue-600 hover:text-blue-800 font-medium"
        >
          {{ sprint.retrospective ? 'Edit' : 'Add' }} Retrospective
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { Sprint } from '@/types/Sprint'
import { updateSprintRetrospective } from '@/services/api'

interface Props {
  sprint: Sprint
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update': [sprint: Sprint]
}>()

const isEditing = ref(false)
const loading = ref(false)
const retrospective = ref(props.sprint.retrospective || '')

watch(() => props.sprint.retrospective, (newVal) => {
  retrospective.value = newVal || ''
})

const startEditing = () => {
  retrospective.value = props.sprint.retrospective || ''
  isEditing.value = true
}

const cancel = () => {
  isEditing.value = false
}

const save = async () => {
  loading.value = true
  try {
    await updateSprintRetrospective(props.sprint.id, retrospective.value)
    isEditing.value = false
    emit('update', { ...props.sprint, retrospective: retrospective.value })
  } catch (error) {
    console.error('Failed to save retrospective:', error)
  } finally {
    loading.value = false
  }
}
</script>
