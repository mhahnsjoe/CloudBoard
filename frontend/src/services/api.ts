import axios from "axios";
import type { Project, TaskItem, ProjectCreate } from "../types/Project";

// Projects
export const getProjects = () => axios.get<Project[]>('/api/projects');
export const getProject = (id: number) => axios.get<Project>(`/api/projects/${id}`);
export const createProject = (project: ProjectCreate) => axios.post<Project>('/api/projects', project);
export const updateProject = (id: number, project: Project) => axios.put(`/api/projects/${id}`, project);
export const deleteProject = (id: number) => axios.delete(`/api/projects/${id}`);

// Tasks
export const getTasks = () => axios.get<TaskItem[]>('/api/taskitem');
export const getTask = (id: number) => axios.get<TaskItem>(`/api/taskitem/${id}`);
export const createTask = (task: TaskItem) => axios.post<TaskItem>('/api/taskitem', task);
export const updateTask = (id: number, task: TaskItem) => axios.put(`/api/taskitem/${id}`, task);
export const deleteTask = (id: number) => axios.delete(`/api/taskitem/${id}`);