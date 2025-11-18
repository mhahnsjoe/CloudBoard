import { ref } from 'vue'

/**
 * Composable for managing dropdown state
 * Provides open/close/toggle functionality
 */
export function useDropdown() {
  const isOpen = ref(false)

  const open = () => {
    isOpen.value = true
  }

  const close = () => {
    isOpen.value = false
  }

  const toggle = () => {
    isOpen.value = !isOpen.value
  }

  return {
    isOpen,
    open,
    close,
    toggle
  }
}