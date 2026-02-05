<template>
  <Teleport to="body">
    <div class="fixed inset-0 z-[60] flex items-center justify-center p-4">
      <!-- Backdrop -->
      <transition
        appear
        enter-active-class="transition-opacity duration-300 ease-out"
        enter-from-class="opacity-0"
        enter-to-class="opacity-100"
        leave-active-class="transition-opacity duration-200 ease-in"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
      >
        <div 
          class="fixed inset-0 bg-gray-900/60 backdrop-blur-sm" 
          @click="$emit('close')"
        ></div>
      </transition>

      <!-- Modal Content -->
      <transition
        appear
        enter-active-class="transition-all duration-300 ease-out"
        enter-from-class="opacity-0 scale-95 translate-y-4"
        enter-to-class="opacity-100 scale-100 translate-y-0"
        leave-active-class="transition-all duration-200 ease-in"
        leave-from-class="opacity-100 scale-100 translate-y-0"
        leave-to-class="opacity-0 scale-95 translate-y-4"
      >
        <div class="relative w-full max-w-lg bg-white rounded-2xl shadow-2xl overflow-hidden flex flex-col">
          <!-- Header with Stepper -->
          <div class="px-5 py-3 border-b border-gray-100 flex items-center justify-between bg-white sticky top-0 z-10">
            <h3 class="text-base font-bold text-gray-900 leading-tight">Create Board Item</h3>
            <button 
              @click="$emit('close')"
              class="p-1.5 rounded-full text-gray-400 hover:text-gray-900 hover:bg-gray-100 transition-all focus:outline-none"
            >
              <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          <!-- Body with Transition -->
          <div class="p-6 max-h-[70vh] overflow-y-auto custom-scrollbar">
            <transition name="fade-slide" mode="out-in">
              <!-- Step 1: Type Selection -->
              <div v-if="currentStep === 1" key="step1" class="space-y-4">
                <div class="space-y-0.5">
                  <h4 class="text-sm font-semibold text-gray-800 text-center">Select Type</h4>
                  <p class="text-[10px] text-gray-400 text-center">What are you working on today?</p>
                </div>
                
                <div class="grid grid-cols-3 gap-3">
                  <button
                    v-for="type in types"
                    :key="type.name"
                    type="button"
                    @click="selectType(type.id)"
                    class="group relative flex flex-col items-center justify-center p-5 rounded-2xl border-2 transition-all duration-200 text-center active:scale-[0.95]"
                    :class="[
                      itemType === type.id 
                        ? 'border-blue-500 bg-blue-50/40 shadow-lg shadow-blue-500/10' 
                        : 'border-gray-50 hover:border-gray-200 hover:bg-gray-50'
                    ]"
                  >
                    <div class="transition-all duration-300 mb-2">
                       <WorkItemTypeBadge 
                        :type="type.id" 
                        class="scale-125 !px-3 !py-1.5" 
                        :class="{'!bg-blue-600 !text-white shadow-md': itemType === type.id && type.id === 'PBI', '!bg-red-600 !text-white shadow-md': itemType === type.id && type.id === 'Bug', '!bg-yellow-600 !text-white shadow-md': itemType === type.id && type.id === 'Task'}"
                       />
                    </div>
                    
                    <!-- Selection Indicator -->
                    <div v-if="itemType === type.id" class="absolute -top-1 -right-1">
                      <div class="bg-blue-600 rounded-full p-0.5 shadow-lg border-2 border-white">
                        <svg class="w-2.5 h-2.5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="4" d="M5 13l4 4L19 7" /></svg>
                      </div>
                    </div>
                  </button>
                </div>
              </div>

              <!-- Step 2: Parent Selection (for Tasks) -->
              <div v-else-if="currentStep === 2" key="step2" class="space-y-4">
                <div class="space-y-0.5">
                  <h4 class="text-sm font-semibold text-gray-800">Link to Parent</h4>
                  <p class="text-xs text-gray-500">Tasks must be linked to a PBI or Bug.</p>
                </div>

                <div class="relative group">
                  <div class="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                    <svg class="w-4 h-4 text-gray-400 group-focus-within:text-blue-500 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg>
                  </div>
                  <input 
                    v-model="parentSearch"
                    type="text"
                    placeholder="Search Parent..."
                    class="w-full pl-9 pr-4 py-2.5 bg-gray-50 border-2 border-transparent focus:border-blue-500 focus:bg-white rounded-xl outline-none transition-all text-xs font-medium"
                  />
                </div>

                <div class="bg-gray-50 rounded-xl overflow-hidden min-h-[100px] max-h-52 flex flex-col">
                  <div class="flex-1 overflow-y-auto custom-scrollbar divide-y divide-gray-100">
                    <div v-if="filteredParents.length === 0" class="p-8 text-center">
                      <p class="text-xs font-medium text-gray-400">No matching items found</p>
                    </div>
                    <button
                      v-for="parent in filteredParents"
                      :key="parent.id"
                      type="button"
                      @click="parentId = parent.id"
                      class="w-full text-left p-3 hover:bg-white transition-all flex items-center gap-3 group/item"
                    >
                      <WorkItemTypeBadge :type="parent.type" size="xs" />
                      <div class="min-w-0 flex-1">
                        <p class="text-xs font-bold text-gray-900 truncate">{{ parent.title }}</p>
                        <p class="text-[10px] text-gray-400">#{{ parent.id }} • {{ parent.status }}</p>
                      </div>
                    </button>
                  </div>
                </div>
              </div>

              <!-- Step 3: Item Details -->
              <div v-else key="step3" class="space-y-6">
                <div class="flex items-center justify-between p-3 bg-gray-50 rounded-xl border border-gray-100">
                   <div class="flex items-center gap-2">
                      <WorkItemTypeBadge :type="itemType as any" />
                      <span class="text-[10px] font-bold text-gray-400 uppercase tracking-widest">New Item</span>
                   </div>
                   <div v-if="parentId" class="text-right">
                      <span class="text-[10px] font-bold text-gray-400 uppercase tracking-widest mr-1">Parent:</span>
                      <span class="text-xs font-bold text-blue-600">#{{ parentId }}</span>
                   </div>
                </div>

                <div class="space-y-4">
                  <!-- Title -->
                  <div class="space-y-1.5">
                    <label class="text-[10px] font-bold text-gray-400 uppercase tracking-widest px-0.5">Title <span class="text-red-500">*</span></label>
                    <input
                      v-model="form.title"
                      type="text"
                      required
                      autofocus
                      class="w-full px-4 py-3 bg-white border-2 border-gray-100 focus:border-blue-500 rounded-xl outline-none transition-all text-sm font-bold placeholder:font-normal placeholder:text-gray-300"
                      placeholder="Enter title..."
                    />
                  </div>

                  <!-- Assignee Selection -->
                  <div class="space-y-1.5">
                    <label class="text-[10px] font-bold text-gray-400 uppercase tracking-widest px-0.5">Assigned To</label>
                    <AssigneeSelector
                      v-model="form.assignedToId"
                      :members="teamMembers"
                      class="!w-full"
                    />
                  </div>

                  <!-- Description -->
                  <div class="space-y-1.5">
                    <label class="text-[10px] font-bold text-gray-400 uppercase tracking-widest px-0.5">Description</label>
                    <textarea
                      v-model="form.description"
                      rows="3"
                      class="w-full px-4 py-3 bg-white border-2 border-gray-100 focus:border-blue-500 rounded-xl outline-none transition-all text-xs font-medium placeholder:font-normal placeholder:text-gray-300 resize-none"
                      placeholder="Context or details..."
                    ></textarea>
                  </div>

                  <!-- Grid -->
                  <div class="grid grid-cols-2 gap-4">
                    <div class="space-y-1.5">
                      <label class="text-[10px] font-bold text-gray-400 uppercase tracking-widest px-0.5">Status</label>
                      <div class="relative">
                        <select
                          v-model="form.status"
                          class="w-full appearance-none px-4 py-2.5 bg-gray-50 border-2 border-transparent focus:border-blue-500 focus:bg-white rounded-xl outline-none transition-all text-xs font-bold"
                        >
                          <option v-for="status in availableStatuses" :key="status" :value="status">{{ status }}</option>
                        </select>
                        <div class="absolute inset-y-0 right-3 flex items-center pointer-events-none text-gray-400">
                          <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
                        </div>
                      </div>
                    </div>

                    <div class="space-y-1.5">
                      <label class="text-[10px] font-bold text-gray-400 uppercase tracking-widest px-0.5">Priority</label>
                      <div class="relative">
                        <select
                          v-model="form.priority"
                          class="w-full appearance-none px-4 py-2.5 bg-gray-50 border-2 border-transparent focus:border-blue-500 focus:bg-white rounded-xl outline-none transition-all text-xs font-bold"
                        >
                          <option value="Low">Low</option>
                          <option value="Medium">Medium</option>
                          <option value="High">High</option>
                          <option value="Critical">Critical</option>
                        </select>
                        <div class="absolute inset-y-0 right-3 flex items-center pointer-events-none text-gray-400">
                          <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Hours -->
                  <div class="space-y-1.5">
                    <label class="text-[10px] font-bold text-gray-400 uppercase tracking-widest px-0.5">Estimated Hours</label>
                    <input
                      v-model.number="form.estimatedHours"
                      type="number"
                      min="0"
                      step="0.5"
                      class="w-full px-4 py-2.5 bg-gray-50 border-2 border-transparent focus:border-blue-500 focus:bg-white rounded-xl outline-none transition-all text-sm font-bold placeholder:font-normal placeholder:text-gray-300"
                      placeholder="0.0"
                    />
                  </div>
                </div>
              </div>
            </transition>
          </div>

          <!-- Footer/Actions -->
          <div class="px-6 py-4 bg-gray-50 flex items-center gap-3">
            <button 
              type="button" 
              @click="handleBack"
              class="px-5 py-2.5 rounded-xl text-xs font-bold text-gray-500 hover:text-gray-900 hover:bg-white transition-all active:scale-[0.98]"
            >
              {{ currentStep === 1 ? 'Cancel' : 'Back' }}
            </button>
            
            <button
              v-if="currentStep === 3"
              type="button"
              @click="handleSubmit"
              class="flex-1 px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-xl shadow-lg shadow-blue-500/10 text-xs font-bold transition-all flex items-center justify-center gap-2 active:scale-[0.98] disabled:opacity-50"
              :disabled="loading || !form.title"
            >
              <svg v-if="loading" class="animate-spin h-3.5 w-3.5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              <span>{{ loading ? 'Creating...' : 'Create Item' }}</span>
            </button>
            <div v-else class="flex-1"></div>
          </div>
        </div>
      </transition>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, defineComponent, h } from 'vue'
import type { WorkItem, WorkItemCreate, WorkItemType } from '@/types/WorkItem'
import { onMounted } from 'vue'
import WorkItemTypeBadge from './WorkItemTypeBadge.vue'
import AssigneeSelector from './AssigneeSelector.vue'
import type { TeamMember } from '@/types/Team'

// Custom Functional Icon Components for consistency (Used in Step 1)
const IconPBI = defineComponent({
  render() {
    return h('svg', { class: 'w-full h-full', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
      h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2.5', d: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z' })
    ])
  }
})

const IconBug = defineComponent({
  render() {
    return h('svg', { class: 'w-full h-full', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
      h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2.5', d: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z' })
    ])
  }
})

const IconTask = defineComponent({
  render() {
    return h('svg', { class: 'w-full h-full', fill: 'none', stroke: 'currentColor', viewBox: '0 0 24 24' }, [
      h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'stroke-width': '2.5', d: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4' })
    ])
  }
})

const props = defineProps<{
  boardId?: number
  defaultStatus?: string
  workItems: WorkItem[]
  availableStatuses: string[]
  sprintId?: number | null
  parentPreselected?: number
  teamMembers: TeamMember[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'create', data: WorkItemCreate): void
}>()

const itemType = ref<WorkItemType | null>(null)
const parentId = ref<number | undefined>(props.parentPreselected)
const parentSearch = ref('')
const loading = ref(false)

onMounted(() => {
  if (props.parentPreselected) {
    itemType.value = 'Task'
  }
})

const types = [
  { id: 'PBI' as WorkItemType, name: 'Backlog Item', desc: 'A user story or requirement.', icon: IconPBI, iconBg: 'bg-blue-500' },
  { id: 'Bug' as WorkItemType, name: 'Fix Bug', desc: 'A defect or issue to fix.', icon: IconBug, iconBg: 'bg-red-500' },
  { id: 'Task' as WorkItemType, name: 'Sub-Task', desc: 'Work linked to a PBI or Bug.', icon: IconTask, iconBg: 'bg-yellow-500' }
]

const form = ref({
  title: '',
  description: '',
  status: props.defaultStatus || 'To Do',
  priority: 'Medium',
  estimatedHours: undefined as number | undefined,
  assignedToId: null as number | null
})

const currentStep = computed(() => {
  if (!itemType.value) return 1
  if (itemType.value === 'Task' && !parentId.value) return 2
  return 3
})



const filteredParents = computed(() => {
  if (!props.workItems) return []
  const search = parentSearch.value.toLowerCase()
  return props.workItems.filter(item => 
    (item.type === 'PBI' || item.type === 'Bug') &&
    (item.title.toLowerCase().includes(search) || item.id.toString().includes(search))
  )
})

const selectType = (type: WorkItemType) => {
  itemType.value = type
  if (type !== 'Task') {
    parentId.value = undefined
  } else if (props.parentPreselected) {
    parentId.value = props.parentPreselected
  }
}

const handleBack = () => {
  if (currentStep.value === 1) {
    emit('close')
  } else if (currentStep.value === 2) {
    itemType.value = null
  } else if (currentStep.value === 3) {
    if (itemType.value === 'Task' && !props.parentPreselected) {
      parentId.value = undefined
    } else {
      itemType.value = null
      parentId.value = undefined
    }
  }
}

const handleSubmit = () => {
  if (!itemType.value || !form.value.title) return

  const data: WorkItemCreate = {
    title: form.value.title,
    description: form.value.description,
    status: form.value.status,
    priority: form.value.priority,
    type: itemType.value as string,
    estimatedHours: form.value.estimatedHours,
    remainingHours: form.value.estimatedHours, // Initialize remaining to estimate
    boardId: props.boardId,
    sprintId: props.sprintId ?? null,
    parentId: parentId.value,
    assignedToId: form.value.assignedToId
  }

  loading.value = true
  emit('create', data)
}
</script>

<style scoped>
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: all 0.2s ease;
}

.fade-slide-enter-from {
  opacity: 0;
  transform: translateX(10px);
}

.fade-slide-leave-to {
  opacity: 0;
  transform: translateX(-10px);
}

.custom-scrollbar::-webkit-scrollbar {
  width: 5px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #f1f1f1;
  border-radius: 10px;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #e5e7eb;
}
</style>
