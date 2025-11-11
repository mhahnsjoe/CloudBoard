<template>
  <div class="min-h-screen bg-gray-50 p-8">
    <ProjectHeader 
      title="Projects" 
      buttonText="New Project"
      @create="openCreateModal"
    >
      <template #filters>
        <SearchBar 
          v-model="searchQuery" 
          placeholder="Search projects..."
        />
      </template>
    </ProjectHeader>

    <!-- Projects grid -->
    <div v-if="filteredProjects.length" class="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <ProjectCard
        v-for="project in filteredProjects"
        :key="project.id"
        :project="project"
        @view="viewProject"
        @kanban="viewKanban"
        @edit="editProject"
        @delete="handleDelete"
      />
    </div>

    <!-- Empty states -->
    <EmptyState
      v-else-if="!loading && searchQuery"
      :icon="SearchIcon"
      :message="`No projects found matching '${searchQuery}'`"
    />
    
    <EmptyState
      v-else-if="!loading"
      :icon="FolderIcon"
      message="No projects yet. Create your first one!"
    />

    <!-- Modal -->
    <Modal
      :show="modal.isOpen.value"
      :title="modal.isEditing.value ? 'Edit Project' : 'Create Project'"
      :submitText="modal.isEditing.value ? 'Update' : 'Create'"
      @close="modal.close"
      @submit="submitForm"
    >
      <input
        v-model="form.name"
        type="text"
        placeholder="Project name"
        class="input"
      />
      <textarea
        v-model="form.description"
        placeholder="Project description (optional)"
        class="input min-h-[100px]"
      />
    </Modal>
  </div>
</template>

<script lang="ts">
import { ref, onMounted, defineComponent } from "vue";
import { useRouter } from "vue-router";
import { useProjects } from "@/composables/useProjects";
import { useSearch } from "@/composables/useSearch";
import { useModal } from "@/composables/useModal";
import { useConfirm } from "@/composables/useConfirm";
import type { Project } from "@/types/Project";

import ProjectHeader from "@/components/project/ProjectHeader.vue";
import SearchBar from "@/components/project/SearchBar.vue";
import ProjectCard from "@/components/project/ProjectCard.vue";
import Modal from "@/components/common/Modal.vue";
import EmptyState from "@/components/common/EmptyState.vue";
import { SearchIcon } from "@/components/icons";

// Placeholder for FolderIcon - you can create this
const FolderIcon = SearchIcon; // Replace with actual FolderIcon component

export default defineComponent({
  name: "ProjectsList",
  components: {
    ProjectHeader,
    SearchBar,
    ProjectCard,
    Modal,
    EmptyState
  },
  setup() {
    const router = useRouter();
    const { projects, loading, fetchProjects, addProject, modifyProject, removeProject } = useProjects();
    const { searchQuery, filteredItems: filteredProjects } = useSearch(projects, ['name', 'description']);
    const modal = useModal<Project>();
    const { confirm } = useConfirm();

    const form = ref<{ id?: number; name: string; description: string }>({
      name: "",
      description: ""
    });

    const openCreateModal = () => {
      form.value = { name: "", description: "" };
      modal.open();
    };

    const viewKanban = (id: number) => {
      router.push(`/projects/${id}/kanban`);
    };

    const editProject = (project: Project) => {
      form.value = {
        id: project.id,
        name: project.name,
        description: project.description
      };
      modal.open(project);
    };

    const submitForm = async () => {
      try {
        if (modal.isEditing.value && form.value.id != null) {
          await modifyProject(form.value.id, {
            id: form.value.id,
            name: form.value.name,
            description: form.value.description
          });
        } else {
          await addProject({
            name: form.value.name,
            description: form.value.description
          });
        }
        modal.close();
      } catch (err) {
        console.error('Failed to submit form:', err);
      }
    };

    const handleDelete = async (id: number) => {
      if (confirm("Are you sure you want to delete this project?")) {
        await removeProject(id);
      }
    };

    const viewProject = (id: number) => {
      router.push(`/projects/${id}`);
    };

    onMounted(fetchProjects);

    return {
      filteredProjects,
      loading,
      searchQuery,
      modal,
      form,
      openCreateModal,
      editProject,
      handleDelete,
      submitForm,
      viewProject,
      viewKanban,
      SearchIcon,
      FolderIcon
    };
  }
});
</script>