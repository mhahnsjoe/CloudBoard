import axios from "axios";
import type { Project, ProjectCreate, Board, BoardCreate, TaskItem, TaskCreate, TaskEdit } from "../types/Project";

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

// Tasks
export const getTasks = (boardId: number) => axios.get<TaskItem[]>(`/api/boards/${boardId}/tasks`);
export const getAllTasks = () => axios.get<TaskItem[]>('/api/tasks');
export const getTask = (boardId: number, taskId: number) => axios.get<TaskItem>(`/api/boards/${boardId}/tasks/${taskId}`);
export const createTask = (boardId: number, task: TaskCreate) => axios.post<TaskItem>(`/api/boards/${boardId}/tasks`, task);
export const updateTask = (boardId: number, taskId: number, task: TaskEdit) => axios.put(`/api/boards/${boardId}/tasks/${taskId}`, task);
export const deleteTask = (boardId: number, taskId: number) => axios.delete(`/api/boards/${boardId}/tasks/${taskId}`);