import axios from "axios";
import type { WorkItem, WorkItemCreate, WorkItemEdit } from "../types/WorkItem";
import type { Project, ProjectCreate, Board, BoardCreate } from "../types/Project";
import type { AuthResponse, User } from "../stores/auth";
import type {
  Sprint,
  CreateSprintDto,
  UpdateSprintDto,
  SprintStats,
  BurndownPoint,
  SprintPlanningContext,
  BulkOperationResult,
  BoardVelocity,

  SprintCapacity
} from "../types/Sprint";
import type { Taskboard } from "../types/Taskboard";
import type { WorkItemDetailDto } from "../types/WorkItem";

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

// Configure axios instance with versioned API
const API_VERSION = 'v1'
const api = axios.create({
  baseURL: `${import.meta.env.VITE_API_URL || ''}/api/${API_VERSION}`,
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
  api.post<AuthResponse>('/auth/login', credentials);
export const register = (data: RegisterData) =>
  api.post<AuthResponse>('/auth/register', data);
export const getCurrentUser = () =>
  api.get<User>('/auth/me');

// Projects
export const getProjects = () => api.get<Project[]>('/projects');
export const getProject = (id: number) => api.get<Project>(`/projects/${id}`);
export const createProject = (project: ProjectCreate) => api.post<Project>('/projects', project);
export const updateProject = (id: number, project: ProjectCreate) => api.put(`/projects/${id}`, project);
export const deleteProject = (id: number) => api.delete(`/projects/${id}`);

// Boards
export const getBoards = (projectId: number) => api.get<Board[]>(`/projects/${projectId}/boards`);
export const getBoard = (projectId: number, boardId: number) => api.get<Board>(`/projects/${projectId}/boards/${boardId}`);
export const createBoard = (projectId: number, board: BoardCreate) => api.post<Board>(`/projects/${projectId}/boards`, board);
export const updateBoard = (projectId: number, boardId: number, board: BoardCreate) => api.put(`/projects/${projectId}/boards/${boardId}`, board);
export const deleteBoard = (projectId: number, boardId: number) => api.delete(`/projects/${projectId}/boards/${boardId}`);

// WorkItems
export const getWorkItems = (boardId: number) => api.get<WorkItem[]>(`/boards/${boardId}/WorkItems`);
export const getAllWorkItems = () => api.get<WorkItem[]>('/WorkItems');
export const getWorkItem = (boardId: number, workItemId: number) => api.get<WorkItem>(`/boards/${boardId}/WorkItems/${workItemId}`);
export const createWorkItem = (boardId: number, workItem: WorkItemCreate) => api.post<WorkItem>(`/boards/${boardId}/WorkItems`, workItem);
export const updateWorkItem = (boardId: number, workItemId: number, workItem: WorkItemEdit) => api.put(`/boards/${boardId}/WorkItems/${workItemId}`, workItem);
export const deleteWorkItem = (boardId: number, workItemId: number) => api.delete(`/boards/${boardId}/WorkItems/${workItemId}`);

// Sprints
export const getSprints = (boardId: number) => api.get<Sprint[]>(`/boards/${boardId}/sprints`);
export const getSprint = (sprintId: number) => api.get<Sprint>(`/sprints/${sprintId}`);
export const createSprint = (boardId: number, data: CreateSprintDto) => api.post<Sprint>(`/boards/${boardId}/sprints`, data);
export const updateSprint = (sprintId: number, data: UpdateSprintDto) => api.put(`/sprints/${sprintId}`, data);
export const startSprint = (sprintId: number) => api.patch(`/sprints/${sprintId}/start`);
export const completeSprint = (sprintId: number) => api.patch<{ movedToBacklog: number }>(`/sprints/${sprintId}/complete`);
export const deleteSprint = (sprintId: number) => api.delete(`/sprints/${sprintId}`);
export const getSprintStats = (sprintId: number) => api.get<SprintStats>(`/sprints/${sprintId}/stats`);
export const getSprintBurndown = (sprintId: number) => api.get<BurndownPoint[]>(`/sprints/${sprintId}/burndown`);
export const getSprintTaskboard = (sprintId: number) => api.get<Taskboard>(`/sprints/${sprintId}/taskboard`);
export const assignWorkItemToSprint = (workItemId: number, sprintId: number | null) => api.patch(`/workitems/${workItemId}/assign-sprint`, { sprintId });

// Sprint Planning
export const getSprintPlanningContext = (boardId: number) =>
  api.get<SprintPlanningContext>(`/boards/${boardId}/sprint-planning`)

export const bulkAssignToSprint = (sprintId: number, workItemIds: number[]) =>
  api.post<BulkOperationResult>(`/sprints/${sprintId}/items/assign`, { workItemIds })

export const bulkUnassignFromSprint = (sprintId: number, workItemIds: number[]) =>
  api.post<BulkOperationResult>(`/sprints/${sprintId}/items/unassign`, { workItemIds })

// Velocity & Capacity
export const getBoardVelocity = (boardId: number, count: number = 6) =>
  api.get<BoardVelocity>(`/boards/${boardId}/velocity`, { params: { count } })

export const getSprintCapacity = (sprintId: number) =>
  api.get<SprintCapacity>(`/sprints/${sprintId}/capacity`)

export const setSprintCapacity = (sprintId: number, capacityHours: number) =>
  api.put(`/sprints/${sprintId}/capacity`, { capacityHours })

// Retrospective
export const updateSprintRetrospective = (sprintId: number, retrospective: string) =>
  api.put(`/sprints/${sprintId}/retrospective`, { retrospective })

// Backlog endpoints
export const getProjectBacklog = (projectId: number) => api.get<WorkItem[]>(`/projects/${projectId}/backlog`);
export const createBacklogItem = (projectId: number, workItem: WorkItemCreate) => api.post<WorkItem>(`/projects/${projectId}/backlog`, workItem);
export const moveToBoard = (workItemId: number, boardId: number | null, sprintId?: number | null) => api.patch(`/workitems/${workItemId}/move-to-board`, { boardId, sprintId });
export const returnWorkItemToBacklog = (workItemId: number) => api.patch(`/workitems/${workItemId}/return-to-backlog`);
export const reorderBacklogItems = (projectId: number, itemOrders: Array<{ itemId: number; order: number }>) =>
  api.patch(`/projects/${projectId}/backlog/reorder`, { itemOrders });

export const getWorkItemDetails = (workItemId: number) =>
  api.get<WorkItemDetailDto>(`/workitems/${workItemId}/details`);