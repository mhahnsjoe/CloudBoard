import axios from "axios";
import type { Project, TaskItem } from "../types/dbo";


const API_URL = "http://localhost:5154/api";


// Projects
export const getProjects = () => axios.get<Project[]>(`${API_URL}/projects`);
export const getProject = (id: number) => axios.get<Project>(`${API_URL}/projects/${id}`);
export const createProject = (project: Project) => axios.post<Project>(`${API_URL}/projects`, project);
export const updateProject = (id: number, project: Project) => axios.put(`${API_URL}/projects/${id}`, project);
export const deleteProject = (id: number) => axios.delete(`${API_URL}/projects/${id}`);

// Tasks
export const getTasks = () => axios.get<TaskItem[]>(`${API_URL}/taskitem`);
export const getTask = (id: number) => axios.get<TaskItem>(`${API_URL}/taskitem/${id}`);
export const createTask = (task: TaskItem) => axios.post<TaskItem>(`${API_URL}/taskitem`, task);
export const updateTask = (id: number, task: TaskItem) => axios.put(`${API_URL}/taskitem/${id}`, task);
export const deleteTask = (id: number) => axios.delete(`${API_URL}/taskitem/${id}`);
