// Board Types
export interface Board {
  id: number;
  name: string;
  description?: string;
  type: BoardType;
  createdAt: string;
  projectId: number;
  tasks?: TaskItem[];
}

export interface BoardCreate {
  name: string;
  description?: string;
  type: BoardType;
}

export enum BoardType {
  Kanban = 0,
  Scrum = 1,
  Backlog = 2
}

// Task Types
export interface TaskItem {
  id: number;
  title: string;
  status: string;
  priority: string;
  type: TaskType;
  description?: string;
  createdAt: string;
  dueDate?: string;
  estimatedHours?: number;
  actualHours?: number;
  boardId: number; // Changed from projectId
}

export interface TaskCreate {
  title: string;
  status: string;
  priority: string;
  type: TaskType;
  description?: string;
  dueDate?: string;
  estimatedHours?: number;
  boardId: number; // Changed from projectId
}

export interface TaskEdit {
  id: number;
  title: string;
  status: string;
  priority: string;
  type: TaskType;
  description?: string;
  dueDate?: string;
  estimatedHours?: number;
  actualHours?: number;
  boardId: number; // Changed from projectId
}

export enum TaskType {
  Task = 0,
  Bug = 1,
  Feature = 2,
  Epic = 3
}

// Project Types
export interface Project {
  id: number;
  name: string;
  description: string;
  createdAt: string;
  boards?: Board[]; // Changed from tasks
}

export interface ProjectCreate {
  name: string;
  description: string;
}

export interface ProjectUpdate {
  name: string;
  description?: string;
}

// Constants
export const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const;
export const STATUSES = ['To Do', 'In Progress', 'Done'] as const;
export const TASK_TYPES = ['Task', 'Bug', 'Feature', 'Epic'] as const;
export const BOARD_TYPES = ['Kanban', 'Scrum', 'Backlog'] as const;

export type Priority = typeof PRIORITIES[number];
export type Status = typeof STATUSES[number];
export type TaskTypeName = typeof TASK_TYPES[number];
export type BoardTypeName = typeof BOARD_TYPES[number];