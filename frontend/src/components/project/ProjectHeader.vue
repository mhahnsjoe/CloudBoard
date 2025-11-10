<template>
  <div class="flex flex-col gap-4 mb-8">
    <div class="flex justify-between items-center">
      <h1 class="text-3xl font-bold text-gray-800">{{ title }}</h1>
      <button
        @click="$emit('create')"
        class="bg-blue-600 text-white px-5 py-2.5 rounded-lg hover:bg-blue-700 transition-all shadow-sm hover:shadow-md flex items-center gap-2"
      >
        <PlusIcon />
        {{ buttonText }}
      </button>
    </div>

    <slot name="filters"></slot>
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { PlusIcon } from '@/components/icons';

export default defineComponent({
  name: 'ProjectHeader',
  components: { PlusIcon },
  props: {
    title: {
      type: String,
      required: true
    },
    buttonText: {
      type: String,
      default: 'New'
    }
  },
  emits: ['create']
});
</script>