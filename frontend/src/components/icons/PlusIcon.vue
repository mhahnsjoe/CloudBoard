<template>
  <svg :class="className" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
  </svg>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'PlusIcon',
  props: {
    className: {
      type: String,
      default: 'w-5 h-5'
    }
  }
});
</script>