<template>
  <div class="create-project">
    <h2>Create New Project</h2>
    <form @submit.prevent="createNewProject">
      <div>
        <label for="name">Name</label>
        <input
          id="name"
          v-model="newProject.name"
          required
          placeholder="Enter project name"
        />
      </div>

      <div>
        <label for="description">Description</label>
        <textarea
          id="description"
          v-model="newProject.description"
          placeholder="Enter project description"
        ></textarea>
      </div>

      <button type="submit">Create</button>
    </form>

    <p v-if="message" class="success-msg">{{ message }}</p>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref } from "vue";
import { createProject } from "../services/api";
import type { Project } from "../types/Project";

export default defineComponent({
  name: "ProjectCreation",
  emits: ["created"],
  setup(_, { emit }) {
    const newProject = ref<Partial<Project>>({
      name: "",
      description: "",
    });
    const message = ref("");

    const createNewProject = async () => {
      if (!newProject.value.name) return;

      try {
        await createProject({
          name: newProject.value.name!,
          description: newProject.value.description || "",
        });

        message.value = "Project created successfully!";
        newProject.value = { name: "", description: "" };

        emit("created");
      } catch (err) {
        console.error(err);
      }
    };

      return { newProject, message, createNewProject };
    },
  });
</script>

<style scoped>
.create-project {
  margin-bottom: 1.5rem;
}

form {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

input,
textarea {
  padding: 0.4rem 0.6rem;
  border: 1px solid #ccc;
  border-radius: 4px;
}

button {
  padding: 0.4rem 0.8rem;
  background-color: #2d89ef;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

button:hover {
  background-color: #226cd1;
}

.success-msg {
  margin-top: 0.5rem;
  color: green;
}
</style>
