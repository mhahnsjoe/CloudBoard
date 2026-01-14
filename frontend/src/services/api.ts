import axios from "axios";
import type { WorkItem, WorkItemCreate, WorkItemEdit } from "../types/WorkItem";
import type { Project, ProjectCreate, Board, BoardCreate } from "../types/Project";
import type { AuthResponse, User } from "../stores/auth";
import type { Sprint, CreateSprintDto, UpdateSprintDto, SprintStats, BurndownPoint } from "../types/Sprint";

// Auth types
export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterData {
  email: string;
  password: string;
  name: string;
}

// Configure axios instance
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '',
});

// Request interceptor to add JWT token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle 401 errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Clear auth data
      localStorage.removeItem('auth_token');
      localStorage.removeItem('auth_user');

      // Redirect to login page if not already there
      if (window.location.pathname !== '/login' && window.location.pathname !== '/register') {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

// Auth
export const login = (credentials: LoginCredentials) =>
  api.post<AuthResponse>('/api/auth/login', credentials);
export const register = (data: RegisterData) =>
  api.post<AuthResponse>('/api/auth/register', data);
export const getCurrentUser = () =>
  api.get<User>('/api/auth/me');

// Projects
export const getProjects = () => api.get<Project[]>('/api/projects');
export const getProject = (id: number) => api.get<Project>(`/api/projects/${id}`);
export const createProject = (project: ProjectCreate) => api.post<Project>('/api/projects', project);
export const updateProject = (id: number, project: ProjectCreate) => api.put(`/api/projects/${id}`, project);
export const deleteProject = (id: number) => api.delete(`/api/projects/${id}`);

// Boards
export const getBoards = (projectId: number) => api.get<Board[]>(`/api/projects/${projectId}/boards`);
export const getBoard = (projectId: number, boardId: number) => api.get<Board>(`/api/projects/${projectId}/boards/${boardId}`);
export const createBoard = (projectId: number, board: BoardCreate) => api.post<Board>(`/api/projects/${projectId}/boards`, board);
export const updateBoard = (projectId: number, boardId: number, board: BoardCreate) => api.put(`/api/projects/${projectId}/boards/${boardId}`, board);
export const deleteBoard = (projectId: number, boardId: number) => api.delete(`/api/projects/${projectId}/boards/${boardId}`);

// WorkItems
export const getWorkItems = (boardId: number) => api.get<WorkItem[]>(`/api/boards/${boardId}/WorkItems`);
export const getAllWorkItems = () => api.get<WorkItem[]>('/api/WorkItems');
export const getWorkItem = (boardId: number, workItemId: number) => api.get<WorkItem>(`/api/boards/${boardId}/WorkItems/${workItemId}`);
export const createWorkItem = (boardId: number, workItem: WorkItemCreate) => api.post<WorkItem>(`/api/boards/${boardId}/WorkItems`, workItem);
export const updateWorkItem = (boardId: number, workItemId: number, workItem: WorkItemEdit) => api.put(`/api/boards/${boardId}/WorkItems/${workItemId}`, workItem);
export const deleteWorkItem = (boardId: number, workItemId: number) => api.delete(`/api/boards/${boardId}/WorkItems/${workItemId}`);

// Sprints
export const getSprints = (boardId: number) => api.get<Sprint[]>(`/api/boards/${boardId}/sprints`);
export const getSprint = (sprintId: number) => api.get<Sprint>(`/api/sprints/${sprintId}`);
export const createSprint = (boardId: number, data: CreateSprintDto) => api.post<Sprint>(`/api/boards/${boardId}/sprints`, data);
export const updateSprint = (sprintId: number, data: UpdateSprintDto) => api.put(`/api/sprints/${sprintId}`, data);
export const startSprint = (sprintId: number) => api.patch(`/api/sprints/${sprintId}/start`);
export const completeSprint = (sprintId: number) => api.patch<{ movedToBacklog: number }>(`/api/sprints/${sprintId}/complete`);
export const deleteSprint = (sprintId: number) => api.delete(`/api/sprints/${sprintId}`);
export const getSprintStats = (sprintId: number) => api.get<SprintStats>(`/api/sprints/${sprintId}/stats`);
export const getSprintBurndown = (sprintId: number) => api.get<BurndownPoint[]>(`/api/sprints/${sprintId}/burndown`);
export const assignWorkItemToSprint = (workItemId: number, sprintId: number | null) => api.patch(`/api/workitems/${workItemId}/assign-sprint`, { sprintId });

// Backlog endpoints
export const getProjectBacklog = (projectId: number) =>  api.get<WorkItem[]>(`/api/projects/${projectId}/backlog`);
export const createBacklogItem = (projectId: number, workItem: WorkItemCreate) => api.post<WorkItem>(`/api/projects/${projectId}/backlog`, workItem);
export const moveToBoard = (workItemId: number, boardId: number | null) => api.patch(`/api/workitems/${workItemId}/move-to-board`, { boardId });
export const returnWorkItemToBacklog = (workItemId: number) => api.patch(`/api/workitems/${workItemId}/return-to-backlog`);
export const reorderBacklogItems = (projectId: number, itemOrders: Array<{ itemId: number; order: number }>) =>
  api.patch(`/api/projects/${projectId}/backlog/reorder`, { itemOrders });