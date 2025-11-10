<template>
  <div id="app">
    <ProjectsList />
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import ProjectsList from "./components/ProjectsList.vue";

export default defineComponent({
  name: "App",
  components: {
    ProjectsList,
  },
});
</script>

<style>
/* optional global styles */
</style>
