export interface TaskboardRow {
    id: number
    title: string
    type: 'PBI' | 'Bug' | 'Unparented'
    status: string
    priority: string
    totalHours: number
    completedHours: number
    remainingHours: number
    progressPercentage: number
    tasks: TaskboardTask[]
    assignedToName?: string | null
    assignedToId?: number | null
}

export interface TaskboardTask {
    id: number
    title: string
    type: 'Task' | 'Bug'
    status: string
    priority: string
    estimatedHours: number | null
    actualHours: number | null
    remainingHours: number | null
    parentId: number
    assignedToId: number | null
    assignedToName: string | null
}

export interface Taskboard {
    sprintId: number
    sprintName: string
    columns: string[]
    rows: TaskboardRow[]
    totalHours: number
    completedHours: number
    totalTasks: number
    completedTasks: number
}
