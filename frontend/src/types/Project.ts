export interface TaskItem {
  id: number;
  title: string;
  status: string;
  priority: string;
  description?: string;
  createdAt: string;
  dueDate?: string;
  estimatedHours?: number;
  actualHours?: number;
  projectId: number;
}

export interface TaskCreate {
  title: string;
  status: string;
  priority: string;
  description?: string;
  dueDate?: string;
  estimatedHours?: number;
  projectId: number;  
}

export interface TaskEdit {
  id: number;
  title: string;
  status: string;
  priority: string;
  description?: string;
  dueDate?: string;
  estimatedHours?: number;
  actualHours?: number;
  projectId: number;
}


export interface Project {
  id: number;
  name: string;
  description: string;
  tasks?: TaskItem[];
}

export interface ProjectCreate {
  name: string;
  description: string;
}

export const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const;
export const STATUSES = ['To Do', 'In Progress', 'Done'] as const;

export type Priority = typeof PRIORITIES[number];
export type Status = typeof STATUSES[number];