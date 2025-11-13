import type { WorkItem } from "./WorkItem"; 

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
export const BOARD_TYPES = ['Kanban', 'Scrum', 'Backlog'] as const;

export type Priority = typeof PRIORITIES[number];
export type Status = typeof STATUSES[number];
export type BoardType = typeof BOARD_TYPES[number];