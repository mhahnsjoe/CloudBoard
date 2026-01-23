import { config } from '@vue/test-utils'
import { vi } from 'vitest'

// Polyfill DragEvent and DataTransfer for jsdom
class DataTransferPolyfill {
  data: Record<string, string> = {}
  effectAllowed: string = 'all'
  dropEffect: string = 'none'
  files: FileList = [] as any
  items: DataTransferItemList = [] as any
  types: string[] = []

  clearData(format?: string): void {
    if (format) {
      delete this.data[format]
    } else {
      this.data = {}
    }
  }

  getData(format: string): string {
    return this.data[format] || ''
  }

  setData(format: string, data: string): void {
    this.data[format] = data
  }

  setDragImage(): void {}
}

class DragEventPolyfill extends Event {
  dataTransfer: DataTransfer

  constructor(type: string, eventInitDict?: EventInit & { dataTransfer?: DataTransfer }) {
    super(type, eventInitDict)
    this.dataTransfer = eventInitDict?.dataTransfer || new DataTransferPolyfill() as any
  }
}

// Assign polyfills to global
if (!global.DragEvent) {
  global.DragEvent = DragEventPolyfill as any
}
if (!global.DataTransfer) {
  global.DataTransfer = DataTransferPolyfill as any
}

// Global mocks
vi.mock('@/services/api', () => ({
  getBoard: vi.fn(),
  createWorkItem: vi.fn(),
  updateWorkItem: vi.fn(),
  deleteWorkItem: vi.fn()
}))

// Add global components if needed
config.global.stubs = {
  // Stub router-link if used
  'router-link': true,
  'router-view': true
}

// Add global plugins
config.global.plugins = []
