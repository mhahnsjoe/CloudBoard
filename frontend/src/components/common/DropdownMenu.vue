<template>
  <div class="relative" ref="menuRef">
    <button
      @click="toggleMenu"
      class="p-2 hover:bg-gray-100 rounded-lg transition-colors"
      :class="buttonClass"
    >
      <MenuIcon :className="iconSize" />
    </button>

    <Teleport to="body">
      <div
        v-if="isOpen"
        :style="menuStyle"
        class="fixed bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-50 min-w-[160px]"
      >
        <slot></slot>
      </div>
    </Teleport>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted, onUnmounted, computed } from 'vue';
import { MenuIcon } from '@/components/icons';

export default defineComponent({
  name: 'DropdownMenu',
  components: { MenuIcon },
  props: {
    buttonClass: {
      type: String,
      default: ''
    },
    iconSize: {
      type: String,
      default: 'w-5 h-5'
    }
  },
  setup() {
    const isOpen = ref(false);
    const menuRef = ref<HTMLElement | null>(null);
    const menuPosition = ref({ top: 0, left: 0 });

    const toggleMenu = (event: Event) => {
      event.stopPropagation();
      
      if (!isOpen.value && menuRef.value) {
        const rect = menuRef.value.getBoundingClientRect();
        menuPosition.value = {
          top: rect.bottom + 4,
          left: rect.right - 160 // Align to right edge
        };
      }
      
      isOpen.value = !isOpen.value;
    };

    const closeMenu = () => {
      isOpen.value = false;
    };

    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.value && !menuRef.value.contains(event.target as Node)) {
        closeMenu();
      }
    };

    const menuStyle = computed(() => ({
      top: `${menuPosition.value.top}px`,
      left: `${menuPosition.value.left}px`
    }));

    onMounted(() => {
      document.addEventListener('click', handleClickOutside);
    });

    onUnmounted(() => {
      document.removeEventListener('click', handleClickOutside);
    });

    return {
      isOpen,
      menuRef,
      menuStyle,
      toggleMenu,
      closeMenu
    };
  }
});
</script>