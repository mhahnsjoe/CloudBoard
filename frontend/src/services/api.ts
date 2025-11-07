import axios from "axios";

export const api = axios.create({
  baseURL: "http://localhost:8080/api",
});

export async function getProjects() {
  const res = await api.get("/projects");
  return res.data;
}
