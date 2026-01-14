export interface WorkItem {
  id: number;
  title: string;
  status: string;
  priority: string;
  type: WorkItemType;
  description?: string;
  createdAt: string;
  dueDate?: string;
  estimatedHours?: number;
  actualHours?: number;
  boardId: number;
  sprintId?: number | null;
  backlogOrder?: number | null;

  // Hierarchy
  parentId?: number;
  parent?: WorkItem;
  children?: WorkItem[];
  level?: number;
  canHaveChildren?: boolean;
  hasChildren?: boolean;
  totalEstimatedHours?: number;
  totalActualHours?: number;
  completionPercentage?: number;
}

export interface WorkItemCreate {
  title: string;
  status: string;
  priority: string;
  type: string;
  description?: string;
  dueDate?: string;
  estimatedHours?: number;
  boardId?: number | null; 
  projectId?: number;       //for backlog items
  parentId?: number;
}

export interface WorkItemEdit {
  id: number;
  title: string;
  status: string;
  priority: string;
  type: string;
  description?: string;
  dueDate?: string;
  estimatedHours?: number;
  actualHours?: number;
  boardId: number;
  parentId?: number;  // Added for hierarchy support
  sprintId?: number | null;
}

export type WorkItemType = 'Task' | 'Bug' | 'PBI' | 'Feature' | 'Epic';

export const WORKITEM_TYPES: readonly WorkItemType[] = ['Epic', 'Feature', 'PBI', 'Task', 'Bug'] as const;

// Hierarchy rules (mirrors backend)
export const TYPE_HIERARCHY: Record<WorkItemType, {
  level: number;
  canHaveChildren: WorkItemType[];
  displayName: string;
  iconClass: string;
  color: string;
}> = {
  'Epic': {
    level: 0,
    canHaveChildren: ['Feature', 'Bug'],
    displayName: 'Epic',
    iconClass: 'epic-icon',
    color: 'orange'
  },
  'Feature': {
    level: 1,
    canHaveChildren: ['PBI', 'Bug'],
    displayName: 'Feature',
    iconClass: 'feature-icon',
    color: 'purple'
  },
  'PBI': {
    level: 2,
    canHaveChildren: ['Task', 'Bug'],
    displayName: 'Product Backlog Item',
    iconClass: 'pbi-icon',
    color: 'blue'
  },
  'Task': {
    level: 3,
    canHaveChildren: [],
    displayName: 'Task',
    iconClass: 'task-icon',
    color: 'yellow'
  },
  'Bug': {
    level: -1, // Flexible
    canHaveChildren: [],
    displayName: 'Bug',
    iconClass: 'bug-icon',
    color: 'red'
  }
};

// Helper functions
export function canHaveChildren(type: WorkItemType): boolean {
  return TYPE_HIERARCHY[type].canHaveChildren.length > 0;
}

export function getAllowedChildTypes(parentType: WorkItemType): WorkItemType[] {
  return TYPE_HIERARCHY[parentType].canHaveChildren;
}

export function getDisplayName(type: WorkItemType): string {
  return TYPE_HIERARCHY[type].displayName;
}

export function getIconClass(type: WorkItemType): string {
  return TYPE_HIERARCHY[type].iconClass;
}

export function getTypeColor(type: WorkItemType): string {
  return TYPE_HIERARCHY[type].color;
}