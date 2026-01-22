import type { WorkItem } from "./WorkItem";

export interface BoardColumn {
  id: number;
  name: string;
  order: number;
  category: 'To Do' | 'In Progress' | 'Done';
}

export interface BoardColumnCreate {
  name: string;
  order: number;
  category: 'To Do' | 'In Progress' | 'Done';
}

export interface Board {
  id: number;
  name: string;
  type: string; // Kanban, Scrum, Backlog
  projectId: number;
  workItems?: WorkItem[];
  columns?: BoardColumn[];
}

export interface BoardCreate {
  name: string;
  type: string;
  projectId: number;
  columns?: BoardColumnCreate[];
}

export interface BoardEdit {
  id: number;
  name: string;
  type: string;
  projectId: number;
  columns?: BoardColumn[];
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

// DEPRECATED: Use board.columns instead
// export const STATUSES = ['To Do', 'In Progress', 'Done'] as const;

export const DEFAULT_STATUSES = ['To Do', 'In Progress', 'Done'] as const;
export const BOARD_TYPES = ['Kanban', 'Scrum', 'Backlog'] as const;

export type Priority = typeof PRIORITIES[number];
export type Status = typeof DEFAULT_STATUSES[number];
export type BoardType = typeof BOARD_TYPES[number];

/**
 * Gets status names from a board's columns.
 * Falls back to default statuses if board has no columns configured.
 *
 * @param board - The board to get statuses from
 * @returns Array of status names in order
 */
export function getStatusesFromBoard(board: Board | null | undefined): string[] {
  if (board?.columns && board.columns.length > 0) {
    return board.columns
      .sort((a, b) => a.order - b.order)
      .map(c => c.name);
  }
  return [...DEFAULT_STATUSES];
}