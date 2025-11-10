<template>
  <div class="text-center text-gray-500 mt-20">
    <component :is="icon" className="w-16 h-16 mx-auto mb-4 text-gray-400" />
    <p>{{ message }}</p>
  </div>
</template>

<script lang="ts">
import { defineComponent, type Component, type PropType } from 'vue';

export default defineComponent({
  name: 'EmptyState',
  props: {
    icon: {
      type: Object as PropType<Component>,
      required: true
    },
    message: {
      type: String,
      required: true
    }
  }
});
</script>