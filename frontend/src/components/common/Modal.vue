<template>
  <Teleport to="body">
    <div v-if="show" class="modal-overlay" @click.self="$emit('close')">
      <div class="modal">
        <h2 class="text-2xl font-bold mb-4">{{ title }}</h2>
        <slot></slot>
        <div class="modal-actions">
          <button class="btn btn-light" @click="$emit('close')">{{ cancelText }}</button>
          <button class="btn btn-primary" @click="$emit('submit')">{{ submitText }}</button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script lang="ts">
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'ModalComponent',
  props: {
    show: {
      type: Boolean,
      required: true
    },
    title: {
      type: String,
      required: true
    },
    submitText: {
      type: String,
      default: 'Submit'
    },
    cancelText: {
      type: String,
      default: 'Cancel'
    }
  },
  emits: ['close', 'submit']
});
</script>