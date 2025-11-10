import { ref, computed, type Ref } from 'vue';

export function useSearch<T>(items: Ref<T[]>, searchFields: (keyof T)[]) {
  const searchQuery = ref('');

  const filteredItems = computed(() => {
    if (!searchQuery.value.trim()) {
      return items.value;
    }

    const query = searchQuery.value.toLowerCase();
    return items.value.filter(item => {
      return searchFields.some(field => {
        const value = item[field];
        return value && String(value).toLowerCase().includes(query);
      });
    });
  });

  return {
    searchQuery,
    filteredItems
  };
}