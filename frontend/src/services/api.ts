import axios from "axios";
import type { WorkItem, WorkItemCreate, WorkItemEdit } from "../types/WorkItem";
import type { Project, ProjectCreate, Board, BoardCreate } from "../types/Project";

// Projects
export const getProjects = () => axios.get<Project[]>('/api/projects');
export const getProject = (id: number) => axios.get<Project>(`/api/projects/${id}`);
export const createProject = (project: ProjectCreate) => axios.post<Project>('/api/projects', project);
export const updateProject = (id: number, project: ProjectCreate) => axios.put(`/api/projects/${id}`, project);
export const deleteProject = (id: number) => axios.delete(`/api/projects/${id}`);

// Boards
export const getBoards = (projectId: number) => axios.get<Board[]>(`/api/projects/${projectId}/boards`);
export const getBoard = (projectId: number, boardId: number) => axios.get<Board>(`/api/projects/${projectId}/boards/${boardId}`);
export const createBoard = (projectId: number, board: BoardCreate) => axios.post<Board>(`/api/projects/${projectId}/boards`, board);
export const updateBoard = (projectId: number, boardId: number, board: BoardCreate) => axios.put(`/api/projects/${projectId}/boards/${boardId}`, board);
export const deleteBoard = (projectId: number, boardId: number) => axios.delete(`/api/projects/${projectId}/boards/${boardId}`);

// WorkItems
export const getWorkItems = (boardId: number) => axios.get<WorkItem[]>(`/api/boards/${boardId}/WorkItems`);
export const getAllWorkItems = () => axios.get<WorkItem[]>('/api/WorkItems');
export const getWorkItem = (boardId: number, workItemId: number) => axios.get<WorkItem>(`/api/boards/${boardId}/WorkItems/${workItemId}`);
export const createWorkItem = (boardId: number, workItem: WorkItemCreate) => axios.post<WorkItem>(`/api/boards/${boardId}/WorkItems`, workItem);
export const updateWorkItem = (boardId: number, workItemId: number, workItem: WorkItemEdit) => axios.put(`/api/boards/${boardId}/WorkItems/${workItemId}`, workItem);
export const deleteWorkItem = (boardId: number, workItemId: number) => axios.delete(`/api/boards/${boardId}/WorkItems/${workItemId}`);