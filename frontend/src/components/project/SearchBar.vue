<template>
  <div class="relative max-w-md">
    <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
      <SearchIcon className="w-5 h-5 text-gray-400" />
    </div>
    <input
      :value="modelValue"
      @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
      type="text"
      :placeholder="placeholder"
      class="w-full pl-10 pr-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition"
    />
  </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue';
import { SearchIcon } from '@/components/icons';

export default defineComponent({
  name: 'SearchBar',
  components: { SearchIcon },
  props: {
    modelValue: {
      type: String,
      required: true
    },
    placeholder: {
      type: String,
      default: 'Search...'
    }
  },
  emits: ['update:modelValue']
});
</script>
