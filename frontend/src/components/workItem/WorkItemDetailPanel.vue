<template>
  <div class="fixed inset-0 z-40 overflow-hidden" aria-labelledby="slide-over-title" role="dialog" aria-modal="true">
    <div class="absolute inset-0 overflow-hidden">
      <!-- Backdrop -->
      <div 
        class="absolute inset-0 bg-gray-500 bg-opacity-75 transition-opacity" 
        aria-hidden="true"
        @click="$emit('close')"
      ></div>

      <div class="fixed inset-y-0 right-0 max-w-full flex pl-10 sm:pl-16">
        <div class="w-screen max-w-2xl transform transition-all ease-in-out duration-500 sm:duration-700 bg-white shadow-xl flex flex-col h-full">
          <!-- Loading State -->
          <div v-if="loading" class="h-full flex items-center justify-center">
            <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          </div>

          <!-- Content -->
          <template v-else-if="details">
            <!-- Header -->
            <div class="px-4 py-6 sm:px-6 border-b border-gray-200 bg-gray-50">
              <div class="flex items-start justify-between space-x-3">
                <div class="space-y-1">
                  <!-- Breadcrumbs -->
                  <div class="flex items-center text-xs text-gray-500 space-x-1 mb-2">
                     <span v-for="(ancestor, index) in details.ancestors" :key="ancestor.id" class="flex items-center">
                        <button 
                          @click="$emit('navigate', ancestor.id)"
                          class="hover:text-blue-600 hover:underline"
                        >
                          {{ ancestor.title }}
                        </button>
                        <span class="mx-1">/</span>
                     </span>
                     <span class="font-medium text-gray-700">Current</span>
                  </div>

                  <div class="flex items-center space-x-2">
                    <span 
                      class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                      :class="getTypeColor(details.type)"
                    >
                      {{ details.type }}
                    </span>
                    <span class="text-sm text-gray-500">#{{ details.id }}</span>
                  </div>
                  <h2 class="text-xl font-bold text-gray-900 leading-snug">{{ details.title }}</h2>
                </div>
                <div class="h-7 flex items-center">
                  <button 
                    @click="$emit('close')" 
                    class="bg-white rounded-md text-gray-400 hover:text-gray-500 focus:outline-none"
                  >
                    <span class="sr-only">Close panel</span>
                    <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                  </button>
                </div>
              </div>
            </div>

            <!-- Body -->
            <div class="flex-1 overflow-y-auto px-4 py-6 sm:px-6">
              <div class="grid grid-cols-1 gap-y-8 gap-x-6 sm:grid-cols-2">
                
                <!-- Status & Priority -->
                <div class="sm:col-span-2 flex gap-4">
                  <div class="w-1/2">
                    <dt class="text-xs font-medium text-gray-500 uppercase tracking-wider">Status</dt>
                    <dd class="mt-1 text-sm text-gray-900 font-medium">{{ details.status }}</dd>
                  </div>
                  <div class="w-1/2">
                    <dt class="text-xs font-medium text-gray-500 uppercase tracking-wider">Priority</dt>
                    <dd class="mt-1">
                      <span 
                        class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium"
                        :class="getPriorityColor(details.priority)"
                      >
                        {{ details.priority }}
                      </span>
                    </dd>
                  </div>
                </div>

                <!-- Description -->
                <div class="sm:col-span-2">
                  <dt class="text-xs font-medium text-gray-500 uppercase tracking-wider">Description</dt>
                  <dd class="mt-2 text-sm text-gray-900 whitespace-pre-wrap bg-gray-50 p-3 rounded-md border border-gray-100 min-h-[4rem]">
                    {{ details.description || 'No description provided.' }}
                  </dd>
                </div>

                <!-- Progress (for items with children) -->
                <div class="sm:col-span-2" v-if="details.children.length > 0">
                   <dt class="text-xs font-medium text-gray-500 uppercase tracking-wider mb-2">Progress</dt>
                   <div class="relative pt-1">
                      <div class="overflow-hidden h-2.5 mb-2 text-xs flex rounded bg-gray-200">
                        <div 
                          style="width: 0%" 
                          :style="{ width: `${details.progressPercentage}%` }"
                          class="shadow-none flex flex-col text-center whitespace-nowrap text-white justify-center bg-blue-500 transition-all duration-500"
                        ></div>
                      </div>
                      <div class="flex justify-between text-xs text-gray-500">
                        <span>{{ details.completedChildCount }} of {{ details.totalChildCount }} tasks completed</span>
                        <span>{{ details.completedHours }} / {{ details.totalEstimatedHours }} hours</span>
                      </div>
                    </div>
                </div>

                <!-- Children List -->
                <div class="sm:col-span-2">
                  <div class="flex items-center justify-between mb-2">
                    <dt class="text-xs font-medium text-gray-500 uppercase tracking-wider">Child Items</dt>
                    <button 
                      v-if="details.type === 'PBI' || details.type === 'Bug'"
                      @click="$emit('add-child', details)"
                      class="text-xs text-blue-600 hover:text-blue-800 font-medium flex items-center"
                    >
                      <svg class="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg>
                      Add Task
                    </button>
                  </div>
                  
                  <div v-if="details.children.length === 0" class="text-sm text-gray-400 italic border border-dashed border-gray-200 rounded p-4 text-center">
                    No child items
                  </div>
                  <ul v-else class="border border-gray-200 rounded-md divide-y divide-gray-200">
                    <li v-for="child in details.children" :key="child.id" class="pl-3 pr-4 py-3 flex items-center justify-between text-sm hover:bg-gray-50">
                      <div class="w-0 flex-1 flex items-center">
                         <span 
                            class="w-2 h-2 rounded-full mr-2"
                            :class="child.status === 'Done' ? 'bg-green-500' : 'bg-gray-300'"
                          ></span>
                         <span class="truncate font-medium text-gray-900">{{ child.title }}</span>
                      </div>
                      <div class="ml-4 flex-shrink-0 flex items-center space-x-4">
                        <span class="text-gray-500">{{ child.status }}</span>
                        <div v-if="child.assignedToName" class="flex items-center text-xs text-gray-500 bg-gray-100 px-2 py-0.5 rounded-full">
                           {{ getInitials(child.assignedToName) }}
                        </div>
                        <button 
                          @click="$emit('navigate', child.id)"
                          class="font-medium text-blue-600 hover:text-blue-500"
                        >
                          View
                        </button>
                      </div>
                    </li>
                  </ul>
                </div>

                <!-- Metadata -->
                <div class="sm:col-span-2 grid grid-cols-2 gap-4 pt-4 border-t border-gray-100">
                  <div>
                    <dt class="text-xs font-medium text-gray-500 uppercase tracking-wider">Assignee</dt>
                    <dd class="mt-1 text-sm text-gray-900 flex items-center">
                       <span class="text-xs bg-gray-200 rounded-full h-6 w-6 flex items-center justify-center mr-2">
                          {{ details.assignedToName ? getInitials(details.assignedToName) : '?' }}
                       </span>
                       {{ details.assignedToName || 'Unassigned' }}
                    </dd>
                  </div>
                  <div>
                    <dt class="text-xs font-medium text-gray-500 uppercase tracking-wider">Created By</dt>
                    <dd class="mt-1 text-sm text-gray-900">{{ details.createdByName }}</dd>
                  </div>
                  <div>
                    <dt class="text-xs font-medium text-gray-500 uppercase tracking-wider">Sprint</dt>
                    <dd class="mt-1 text-sm text-gray-900">{{ details.sprintName || 'No active sprint' }}</dd>
                  </div>
                   <div>
                    <dt class="text-xs font-medium text-gray-500 uppercase tracking-wider">Effort</dt>
                    <dd class="mt-1 text-sm text-gray-900">
                      {{ details.remainingHours ?? 0 }}h remaining / {{ details.estimatedHours ?? 0 }}h est
                    </dd>
                  </div>
                </div>

              </div>
            </div>

            <!-- Footer -->
            <div class="flex-shrink-0 px-4 py-4 flex justify-between items-center bg-gray-50 border-t border-gray-200">
              <div class="flex gap-2">
                <button
                  type="button"
                  @click="$emit('return-to-backlog', details)"
                  class="bg-white py-2 px-4 border border-purple-200 rounded-md shadow-sm text-sm font-medium text-purple-700 hover:bg-purple-50 focus:outline-none flex items-center gap-1.5"
                  title="Return this item to the backlog"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 17l-5-5m0 0l5-5m-5 5h12"/>
                  </svg>
                  Move to Backlog
                </button>
                <button
                  type="button"
                  @click="$emit('delete', details.id)"
                  class="bg-white py-2 px-4 border border-red-200 rounded-md shadow-sm text-sm font-medium text-red-600 hover:bg-red-50 focus:outline-none flex items-center gap-1.5"
                  title="Delete this item"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                  </svg>
                  Delete
                </button>
              </div>
              <button
                @click="$emit('edit', details)"
                class="bg-blue-600 py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white hover:bg-blue-700 focus:outline-none"
              >
                Edit
              </button>
            </div>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { WorkItemDetailDto, WorkItemType } from '@/types/WorkItem'

defineProps<{
  details: WorkItemDetailDto | null
  loading: boolean
}>()

defineEmits<{
  (e: 'close'): void
  (e: 'navigate', id: number): void
  (e: 'edit', item: WorkItemDetailDto): void
  (e: 'add-child', item: WorkItemDetailDto): void
  (e: 'delete', id: number): void
  (e: 'return-to-backlog', item: WorkItemDetailDto): void
}>()

const getInitials = (name: string) => {
  return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase()
}

const getTypeColor = (type: WorkItemType | string) => {
  switch (type) {
    case 'PBI': return 'bg-blue-100 text-blue-800'
    case 'Bug': return 'bg-red-100 text-red-800'
    case 'Task': return 'bg-yellow-100 text-yellow-800'
    case 'Feature': return 'bg-purple-100 text-purple-800'
    case 'Epic': return 'bg-orange-100 text-orange-800'
    default: return 'bg-gray-100 text-gray-800'
  }
}

const getPriorityColor = (priority: string) => {
  switch (priority) {
    case 'High': return 'bg-red-100 text-red-800'
    case 'Medium': return 'bg-yellow-100 text-yellow-800'
    case 'Low': return 'bg-green-100 text-green-800'
    case 'Critical': return 'bg-red-800 text-white'
    default: return 'bg-gray-100 text-gray-800'
  }
}
</script>
