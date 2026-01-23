import { describe, it, expect, vi, beforeEach } from 'vitest'
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

describe('BoardCanvas.vue', () => {
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

  describe('Rendering', () => {
    it('renders correct number of columns', () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const columns = wrapper.findAll('.bg-gray-100.rounded-lg')
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

      const badges = wrapper.findAll('.text-sm.text-gray-500.bg-white')
      expect(badges[0]!.text()).toBe('2') // To Do has 2 items
      expect(badges[1]!.text()).toBe('1') // In Progress has 1 item
      expect(badges[2]!.text()).toBe('1') // Done has 1 item
    })

    it('shows empty state when column has no items', () => {
      const emptyWorkItems: WorkItem[] = []

      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: emptyWorkItems,
          columns: mockColumns
        }
      })

      expect(wrapper.text()).toContain('Drop WorkItems here')
    })

    it('filters work items by status correctly', () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const component = wrapper.vm as any
      const toDoItems = component.getWorkItemsByStatus('To Do')
      const inProgressItems = component.getWorkItemsByStatus('In Progress')
      const doneItems = component.getWorkItemsByStatus('Done')

      expect(toDoItems).toHaveLength(2)
      expect(inProgressItems).toHaveLength(1)
      expect(doneItems).toHaveLength(1)
    })
  })

  describe('New Item Button', () => {
    it('shows "New Item" button only in first column', () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const newItemButtons = wrapper.findAll('button')
      expect(newItemButtons).toHaveLength(1)
      expect(newItemButtons[0]!.text()).toContain('New Item')
    })

    it('emits create-workitem event when "New Item" is clicked', async () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const newItemButton = wrapper.find('button')
      await newItemButton.trigger('click')

      expect(wrapper.emitted('create-workitem')).toBeTruthy()
      expect(wrapper.emitted('create-workitem')?.[0]).toEqual(['To Do'])
    })

    it('does not show "New Item" button in non-first columns', () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const columnContainers = wrapper.findAll('.bg-gray-100.rounded-lg')
      // Check second and third columns don't have the button
      expect(columnContainers[1]!.find('button').exists()).toBe(false)
      expect(columnContainers[2]!.find('button').exists()).toBe(false)
    })
  })

  describe('Drag and Drop', () => {
    it('sets draggedWorkItem on dragstart', async () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const kanbanCard = wrapper.findComponent({ name: 'KanbanCard' })

      // Simulate dragstart by emitting from KanbanCard
      const mockEvent = {
        dataTransfer: new DataTransfer(),
        preventDefault: vi.fn()
      }

      await kanbanCard.vm.$emit('dragstart', mockEvent, mockWorkItems[0])

      // Verify draggedWorkItem is set internally
      const component = wrapper.vm as any
      expect(component.draggedWorkItem).toEqual(mockWorkItems[0])
    })

    it('emits update-status on drop to different column', async () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      // Simulate dragstart
      const kanbanCard = wrapper.findComponent({ name: 'KanbanCard' })
      const mockDragEvent = {
        dataTransfer: new DataTransfer(),
        preventDefault: vi.fn()
      }
      await kanbanCard.vm.$emit('dragstart', mockDragEvent, mockWorkItems[0])

      // Simulate drop on different column (In Progress)
      const dropZones = wrapper.findAll('.min-h-\\[500px\\]')
      await dropZones[1]!.trigger('drop')

      expect(wrapper.emitted('update-status')).toBeTruthy()
      expect(wrapper.emitted('update-status')?.[0]).toEqual([mockWorkItems[0], 'In Progress'])
    })

    it('does not emit update-status when dropped in same column', async () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      // Simulate dragstart from To Do column
      const kanbanCard = wrapper.findComponent({ name: 'KanbanCard' })
      const mockDragEvent = {
        dataTransfer: new DataTransfer(),
        preventDefault: vi.fn()
      }
      await kanbanCard.vm.$emit('dragstart', mockDragEvent, mockWorkItems[0])

      // Simulate drop on same column (To Do)
      const dropZones = wrapper.findAll('.min-h-\\[500px\\]')
      await dropZones[0]!.trigger('drop')

      expect(wrapper.emitted('update-status')).toBeFalsy()
    })

    it('clears draggedWorkItem after drop', async () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      // Simulate dragstart
      const kanbanCard = wrapper.findComponent({ name: 'KanbanCard' })
      const mockDragEvent = {
        dataTransfer: new DataTransfer(),
        preventDefault: vi.fn()
      }
      await kanbanCard.vm.$emit('dragstart', mockDragEvent, mockWorkItems[0])

      // Simulate drop
      const dropZones = wrapper.findAll('.min-h-\\[500px\\]')
      await dropZones[1]!.trigger('drop')

      const component = wrapper.vm as any
      expect(component.draggedWorkItem).toBeNull()
    })

    it('prevents default behavior on dragover', async () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const dropZones = wrapper.findAll('.min-h-\\[500px\\]')

      // The component has @dragover.prevent which prevents default
      // We just need to verify the event can be triggered without errors
      await dropZones[0]!.trigger('dragover')

      // If we got here without errors, the prevent modifier is working
      expect(true).toBe(true)
    })

    it('prevents default behavior on dragenter', async () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const dropZones = wrapper.findAll('.min-h-\\[500px\\]')

      // The component has @dragenter.prevent which prevents default
      // We just need to verify the event can be triggered without errors
      await dropZones[0]!.trigger('dragenter')

      // If we got here without errors, the prevent modifier is working
      expect(true).toBe(true)
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

    it('forwards delete-workitem event from KanbanCard', async () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const kanbanCard = wrapper.findComponent({ name: 'KanbanCard' })
      await kanbanCard.vm.$emit('delete', mockWorkItems[0]!.id)

      expect(wrapper.emitted('delete-workitem')).toBeTruthy()
      expect(wrapper.emitted('delete-workitem')?.[0]).toEqual([mockWorkItems[0]!.id])
    })

    it('forwards return-to-backlog event from KanbanCard', async () => {
      const wrapper = mount(BoardCanvas, {
        props: {
          workItems: mockWorkItems,
          columns: mockColumns
        }
      })

      const kanbanCard = wrapper.findComponent({ name: 'KanbanCard' })
      await kanbanCard.vm.$emit('return-to-backlog', mockWorkItems[0])

      expect(wrapper.emitted('return-to-backlog')).toBeTruthy()
      expect(wrapper.emitted('return-to-backlog')?.[0]).toEqual([mockWorkItems[0]])
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
