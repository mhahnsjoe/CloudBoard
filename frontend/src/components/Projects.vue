<template>
  <div>
    <h1>Projects</h1>
    <ul>
      <li v-for="project in projectsList" :key="project.id">
        {{ project.name }} ({{ project.tasks.length }} tasks)
      </li>
    </ul>
  </div>
</template>

<script lang="ts">
import { getProjects } from "../services/api";
import type { Project } from "../types/dbo";
import { defineComponent } from "vue";

export default defineComponent({
  name: "ProjectsList",
  data() {
    return {
      projectsList: [] as Project[],
    };
  },
  async mounted() {
    const response = await getProjects();
    this.projectsList = response.data;
  },
});
</script>
