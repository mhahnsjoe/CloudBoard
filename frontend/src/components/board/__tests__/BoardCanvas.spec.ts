import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import BoardCanvas from '../BoardCanvas.vue'
import type { WorkItem } from '@/types/WorkItem'
import type { BoardColumn } from '@/types/Project'

// Mock dependencies
vi.mock('@/utils/badges', () => ({
  getStatusIconClass: vi.fn(() => 'text-blue-500')
}))

// Mock child components
vi.mock('../../kanban/KanbanCard.vue', () => ({
  default: {
    name: 'KanbanCard',
    template: '<div class="kanban-card-mock" data-testid="kanban-card">{{ workItem.title }}</div>',
    props: ['workItem', 'columns']
  }
}))

vi.mock('../../icons', () => ({
  PlusIcon: {
    name: 'PlusIcon',
    template: '<span data-testid="plus-icon">+</span>',
    props: ['className']
  }
}))

const mockColumns: BoardColumn[] = [
  { id: 1, name: 'To Do', order: 0, category: 'To Do', boardId: 1 },
  { id: 2, name: 'In Progress', order: 1, category: 'In Progress', boardId: 1 },
  { id: 3, name: 'Done', order: 2, category: 'Done', boardId: 1 }
]

const mockWorkItems: WorkItem[] = [
  {
    id: 1,
    title: 'Task 1',
    status: 'To Do',
    priority: 'High',
    type: 'Task',
    createdAt: '2024-01-01',
    boardId: 1
  },
  {
    id: 2,
    title: 'Task 2',
    status: 'To Do',
    priority: 'Medium',
    type: 'Task',
    createdAt: '2024-01-02',
    boardId: 1
  },
  {
    id: 3,
    title: 'Task 3',
    status: 'In Progress',
    priority: 'High',
    type: 'Bug',
    createdAt: '2024-01-03',
    boardId: 1
  },
  {
    id: 4,
    title: 'Task 4',
    status: 'Done',
    priority: 'Low',
    type: 'PBI',
    createdAt: '2024-01-04',
    boardId: 1
  }
]

describe('BoardCanvas.vue', () => {

  describe('Rendering', () => {
    it('renders correct number of columns', () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const columns = wrapper.findAll('[data-testid="board-column"]')
      expect(columns).toHaveLength(3)
    })

    it('displays column names correctly', () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      expect(wrapper.text()).toContain('To Do')
      expect(wrapper.text()).toContain('In Progress')
      expect(wrapper.text()).toContain('Done')
    })

    it('renders columns in correct order', () => {
      // Shuffle columns to test ordering
      const shuffledColumns: BoardColumn[] = [
        { id: 3, name: 'Done', order: 2, category: 'Done', boardId: 1 },
        { id: 1, name: 'To Do', order: 0, category: 'To Do', boardId: 1 },
        { id: 2, name: 'In Progress', order: 1, category: 'In Progress', boardId: 1 }
      ]

      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: shuffledColumns
        }
      })

      const columnHeaders = wrapper.findAll('h2')
      expect(columnHeaders[0]!.text()).toContain('To Do')
      expect(columnHeaders[1]!.text()).toContain('In Progress')
      expect(columnHeaders[2]!.text()).toContain('Done')
    })
  })

  describe('Work Items', () => {
    it('shows work items in correct columns', () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const kanbanCards = wrapper.findAllComponents({ name: 'KanbanCard' })
      expect(kanbanCards).toHaveLength(4)
    })

    it('displays correct item count badges', () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const badges = wrapper.findAll('[data-testid="column-count-badge"]')
      expect(badges[0]!.text()).toBe('2') // To Do has 2 items
      expect(badges[1]!.text()).toBe('1') // In Progress has 1 item
      expect(badges[2]!.text()).toBe('1') // Done has 1 item
    })

    it('shows empty state when column has no items', async () => {
      const emptyWorkItems: WorkItem[] = []

      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: emptyWorkItems,
          columns: mockColumns
        }
      })

      // Empty state only shows when dragging
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const component = wrapper.vm as any
      component.isDragging = true
      await wrapper.vm.$nextTick()

      // Update selector or check text content directly
      expect(wrapper.text()).toContain('Drop WorkItems here')
    })

    it('filters work items by status correctly', () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const component = wrapper.vm as any
      const toDoItems = component.getWorkItemsByStatus('To Do')
      const inProgressItems = component.getWorkItemsByStatus('In Progress')
      const doneItems = component.getWorkItemsByStatus('Done')

      expect(toDoItems).toHaveLength(2)
      expect(inProgressItems).toHaveLength(1)
      expect(doneItems).toHaveLength(1)
    })
  })



  describe('Event Emissions', () => {
    it('forwards edit-workitem event from KanbanCard', async () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const kanbanCard = wrapper.findComponent({ name: 'KanbanCard' })
      await kanbanCard.vm.$emit('edit', mockWorkItems[0])

      expect(wrapper.emitted('edit-workitem')).toBeTruthy()
      expect(wrapper.emitted('edit-workitem')?.[0]).toEqual([mockWorkItems[0]])
    })
  })

  describe('Computed Properties', () => {
    it('computes orderedColumns correctly', () => {
      const unorderedColumns: BoardColumn[] = [
        { id: 2, name: 'In Progress', order: 1, category: 'In Progress', boardId: 1 },
        { id: 3, name: 'Done', order: 2, category: 'Done', boardId: 1 },
        { id: 1, name: 'To Do', order: 0, category: 'To Do', boardId: 1 }
      ]

      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: unorderedColumns
        }
      })

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const component = wrapper.vm as any
      const ordered = component.orderedColumns

      expect(ordered[0].name).toBe('To Do')
      expect(ordered[1].name).toBe('In Progress')
      expect(ordered[2].name).toBe('Done')
    })
  })

  describe('Grid Layout', () => {
    it('sets correct grid template columns based on column count', () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const gridContainer = wrapper.find('.grid')
      const style = gridContainer.attributes('style')

      expect(style).toContain('grid-template-columns: repeat(3, minmax(0, 1fr))')
    })

    it('adapts grid layout for different column counts', () => {
      const twoColumns: BoardColumn[] = [
        { id: 1, name: 'To Do', order: 0, category: 'To Do', boardId: 1 },
        { id: 2, name: 'Done', order: 1, category: 'Done', boardId: 1 }
      ]

      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: twoColumns
        }
      })

      const gridContainer = wrapper.find('.grid')
      const style = gridContainer.attributes('style')

      expect(style).toContain('grid-template-columns: repeat(2, minmax(0, 1fr))')
    })
  })
})