import { ref } from 'vue';

export function useModal<T = Record<string, unknown>>() {
  const isOpen = ref(false);
  const isEditing = ref(false);
  const data = ref<T | null>(null);

  const open = (editData?: T) => {
    if (editData) {
      isEditing.value = true;
      data.value = editData;
    } else {
      isEditing.value = false;
      data.value = null;
    }
    isOpen.value = true;
  };

  const close = () => {
    isOpen.value = false;
    isEditing.value = false;
    data.value = null;
  };

  return {
    isOpen,
    isEditing,
    data,
    open,
    close
  };
}