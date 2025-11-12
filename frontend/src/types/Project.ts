export interface WorkItem {
  id: number;
  title: string;
  status: string;
  priority: string;
  type: string; // WorkItem, Bug, Feature, Epic
  description?: string;
  createdAt: string;
  dueDate?: string;
  estimatedHours?: number;
  actualHours?: number;
  boardId: number; // Changed from projectId to boardId
}

export interface WorkItemCreate {
  title: string;
  status: string;
  priority: string;
  type: string;
  description?: string;
  dueDate?: string;
  estimatedHours?: number;
  boardId: number;
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
}

export interface Board {
  id: number;
  name: string;
  type: string; // Kanban, Scrum, Backlog
  projectId: number;
  workItems?: WorkItem[];
}

export interface BoardCreate {
  name: string;
  type: string;
  projectId: number;
}

export interface BoardEdit {
  id: number;
  name: string;
  type: string;
  projectId: number;
}

export interface Project {
  id: number;
  name: string;
  description: string;
  boards?: Board[];
}

export interface ProjectCreate {
  name: string;
  description: string;
}

export const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const;
export const STATUSES = ['To Do', 'In Progress', 'Done'] as const;
export const WORKITEM_TYPES = ['Task', 'Bug', 'Feature', 'Epic'] as const;
export const BOARD_TYPES = ['Kanban', 'Scrum', 'Backlog'] as const;

export type Priority = typeof PRIORITIES[number];
export type Status = typeof STATUSES[number];
export type WorkItemType = typeof WORKITEM_TYPES[number];
export type BoardType = typeof BOARD_TYPES[number];