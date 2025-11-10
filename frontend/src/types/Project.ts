export interface TaskItem {
  id: number;
  title: string;
  status: string;
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
