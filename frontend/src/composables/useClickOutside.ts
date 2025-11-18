import { onMounted, onBeforeUnmount, type Ref } from 'vue'

/**
 * Composable to handle clicks outside of a specified element
 * Automatically sets up and cleans up event listeners
 */
export function useClickOutside(
  elementRef: Ref<HTMLElement | null>,
  callback: () => void
) {
  const handleClickOutside = (event: MouseEvent) => {
    if (elementRef.value && !elementRef.value.contains(event.target as Node)) {
      callback()
    }
  }

  onMounted(() => {
    document.addEventListener('click', handleClickOutside)
  })

  onBeforeUnmount(() => {
    document.removeEventListener('click', handleClickOutside)
  })

  return {
    handleClickOutside
  }
}