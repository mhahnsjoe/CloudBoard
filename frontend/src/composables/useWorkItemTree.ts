import { ref, computed, type Ref } from 'vue'
import type { WorkItem, WorkItemType } from '@/types/WorkItem'

// Hierarchy rules matching backend
const HIERARCHY_RULES: Record<WorkItemType, WorkItemType[]> = {
  'Epic': ['Feature', 'Bug'],
  'Feature': ['PBI', 'Bug'],
  'PBI': ['Task', 'Bug'],
  'Task': [],
  'Bug': []
}

const TYPE_LEVELS: Record<WorkItemType, number> = {
  'Epic': 0,
  'Feature': 1,
  'PBI': 2,
  'Task': 3,
  'Bug': -1 // Flexible
}

export interface TreeNode extends WorkItem {
  children: TreeNode[]
  expanded: boolean
  depth: number
}

export interface TreeFilters {
  types: WorkItemType[]
  statuses: string[]
  searchQuery: string
}

export function useWorkItemTree(workItems: Ref<WorkItem[]>) {
  const expandedIds = ref<Set<number>>(new Set())
  
  const filters = ref<TreeFilters>({
    types: [],
    statuses: [],
    searchQuery: ''
  })

  // Build tree structure from flat list
  const buildTree = (items: WorkItem[]): TreeNode[] => {
    const itemMap = new Map<number, TreeNode>()
    const roots: TreeNode[] = []

    // First pass: create all nodes
    items.forEach(item => {
      itemMap.set(item.id, {
        ...item,
        children: [],
        expanded: expandedIds.value.has(item.id),
        depth: 0
      })
    })

    // Second pass: build hierarchy
    items.forEach(item => {
      const node = itemMap.get(item.id)!
      if (item.parentId && itemMap.has(item.parentId)) {
        const parent = itemMap.get(item.parentId)!
        node.depth = parent.depth + 1
        parent.children.push(node)
      } else {
        roots.push(node)
      }
    })

    // Sort children by type level, then by id
    const sortChildren = (nodes: TreeNode[]) => {
      nodes.sort((a, b) => {
        const levelA = TYPE_LEVELS[a.type] === -1 ? 99 : TYPE_LEVELS[a.type]
        const levelB = TYPE_LEVELS[b.type] === -1 ? 99 : TYPE_LEVELS[b.type]
        if (levelA !== levelB) return levelA - levelB
        return a.id - b.id
      })
      nodes.forEach(node => sortChildren(node.children))
    }

    sortChildren(roots)
    return roots
  }

  // Apply filters
  const filteredItems = computed(() => {
    let items = workItems.value

    // Filter by type
    if (filters.value.types.length > 0) {
      items = items.filter(item => filters.value.types.includes(item.type))
    }

    // Filter by status
    if (filters.value.statuses.length > 0) {
      items = items.filter(item => filters.value.statuses.includes(item.status))
    }

    // Filter by search query
    if (filters.value.searchQuery.trim()) {
      const query = filters.value.searchQuery.toLowerCase()
      items = items.filter(item => 
        item.title.toLowerCase().includes(query) ||
        item.description?.toLowerCase().includes(query) ||
        `#${item.id}`.includes(query)
      )
    }

    return items
  })

  // Build filtered tree
  const tree = computed(() => buildTree(filteredItems.value))

  // Flatten tree for rendering (respects expanded state)
  const flattenedTree = computed(() => {
    const result: TreeNode[] = []
    
    const flatten = (nodes: TreeNode[]) => {
      nodes.forEach(node => {
        result.push(node)
        if (node.expanded && node.children.length > 0) {
          flatten(node.children)
        }
      })
    }
    
    flatten(tree.value)
    return result
  })

  // Toggle expansion
  const toggleExpanded = (id: number) => {
    if (expandedIds.value.has(id)) {
      expandedIds.value.delete(id)
    } else {
      expandedIds.value.add(id)
    }
    // Force reactivity
    expandedIds.value = new Set(expandedIds.value)
  }

  // Expand all
  const expandAll = () => {
    workItems.value.forEach(item => {
      if (canHaveChildren(item.type)) {
        expandedIds.value.add(item.id)
      }
    })
    expandedIds.value = new Set(expandedIds.value)
  }

  // Collapse all
  const collapseAll = () => {
    expandedIds.value.clear()
    expandedIds.value = new Set(expandedIds.value)
  }

  // Check if type can have children
  const canHaveChildren = (type: WorkItemType): boolean => {
    return HIERARCHY_RULES[type].length > 0
  }

  // Get allowed child types for a parent
  const getAllowedChildTypes = (parentType: WorkItemType): WorkItemType[] => {
    return HIERARCHY_RULES[parentType]
  }

  // Get valid parents for a given type (for parent selector)
  const getValidParents = (childType: WorkItemType, excludeId?: number): WorkItem[] => {
    return workItems.value.filter(item => {
      // Can't be own parent
      if (excludeId && item.id === excludeId) return false
      // Check if this type can have the child type
      return HIERARCHY_RULES[item.type].includes(childType)
    })
  }

  // Get breadcrumb path for an item
  const getBreadcrumbPath = (itemId: number): WorkItem[] => {
    const path: WorkItem[] = []
    let current = workItems.value.find(i => i.id === itemId)
    
    while (current) {
      path.unshift(current)
      current = current.parentId 
        ? workItems.value.find(i => i.id === current!.parentId)
        : undefined
    }
    
    return path
  }

  // Update filters
  const setTypeFilter = (types: WorkItemType[]) => {
    filters.value.types = types
  }

  const setStatusFilter = (statuses: string[]) => {
    filters.value.statuses = statuses
  }

  const setSearchQuery = (query: string) => {
    filters.value.searchQuery = query
  }

  const clearFilters = () => {
    filters.value = {
      types: [],
      statuses: [],
      searchQuery: ''
    }
  }

  return {
    tree,
    flattenedTree,
    filters,
    filteredItems,
    toggleExpanded,
    expandAll,
    collapseAll,
    canHaveChildren,
    getAllowedChildTypes,
    getValidParents,
    getBreadcrumbPath,
    setTypeFilter,
    setStatusFilter,
    setSearchQuery,
    clearFilters,
    expandedIds
  }
}

// Export hierarchy helpers for use elsewhere
export { HIERARCHY_RULES, TYPE_LEVELS }