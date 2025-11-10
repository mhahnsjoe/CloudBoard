<template>
  <div id="app" class="flex h-screen">
    <Sidebar />
    <div class="flex-1 ml-64 overflow-auto">
      <router-view />
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import Sidebar from "@/components/layout/Sidebar.vue";

export default defineComponent({
  name: "App",
  components: {
    Sidebar
  }
});
</script>

<style>
html, body {
  margin: 0;
  padding: 0;
  height: 100%;
}

#app {
  height: 100%;
}
</style>