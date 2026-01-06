<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import type { WorkflowTask, CreateTaskDto } from '@/services/api';
import { taskApi } from '@/services/api';
import TaskCard from '@/components/TaskCard.vue';
import AddTaskDialog from '@/components/AddTaskDialog.vue';

const tasks = ref<WorkflowTask[]>([]);
const loading = ref(true);
const dialogOpen = ref(false);

const pendingTasks = computed(() => 
  tasks.value.filter(t => !t.isCompleted)
);

const completedTasks = computed(() => 
  tasks.value.filter(t => t.isCompleted)
);

const fetchTasks = async () => {
  loading.value = true;
  try {
    tasks.value = await taskApi.getAllTasks();
  } catch (error) {
    console.error('Failed to fetch tasks:', error);
  } finally {
    loading.value = false;
  }
};

const handleCreateTask = async (task: CreateTaskDto) => {
  try {
    const newTask = await taskApi.createTask(task);
    tasks.value.unshift(newTask);
  } catch (error) {
    console.error('Failed to create task:', error);
  }
};

const handleToggleTask = async (id: number, isCurrentlyCompleted: boolean) => {
  try {
    let updatedTask: WorkflowTask;
    if (isCurrentlyCompleted) {
      // Task is completed, so uncomplete it
      updatedTask = await taskApi.uncompleteTask(id);
    } else {
      // Task is pending, so complete it
      updatedTask = await taskApi.completeTask(id);
    }
    const index = tasks.value.findIndex(t => t.id === id);
    if (index !== -1) {
      tasks.value.splice(index, 1, updatedTask);
    }
  } catch (error) {
    console.error('Failed to toggle task:', error);
  }
};

const handleDeleteTask = async (id: number) => {
  try {
    await taskApi.deleteTask(id);
    tasks.value = tasks.value.filter(t => t.id !== id);
  } catch (error) {
    console.error('Failed to delete task:', error);
  }
};

onMounted(() => {
  fetchTasks();
});
</script>

<template>
  <div class="min-h-screen bg-background">
    <div class="max-w-2xl mx-auto p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-8">
        <div>
          <h1 class="text-2xl font-bold text-foreground">Tasks</h1>
          <p class="text-sm text-muted-foreground mt-1">
            {{ pendingTasks.length }} pending, {{ completedTasks.length }} completed
          </p>
        </div>
        <AddTaskDialog v-model:open="dialogOpen" @create="handleCreateTask">
          <button class="px-4 py-2 bg-foreground text-background rounded-lg font-medium text-sm hover:bg-foreground/90">
            + Add Task
          </button>
        </AddTaskDialog>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="text-center py-12 text-muted-foreground">
        Loading...
      </div>

      <!-- Tasks -->
      <div v-else class="space-y-8">
        <!-- Pending Tasks -->
        <div v-if="pendingTasks.length > 0">
          <h2 class="text-sm font-medium text-muted-foreground mb-3">TO DO</h2>
          <div class="space-y-2">
            <TaskCard 
              v-for="task in pendingTasks" 
              :key="task.id"
              :task="task"
              @toggle="handleToggleTask"
              @delete="handleDeleteTask"
            />
          </div>
        </div>

        <!-- Completed Tasks -->
        <div v-if="completedTasks.length > 0">
          <h2 class="text-sm font-medium text-muted-foreground mb-3">COMPLETED</h2>
          <div class="space-y-2">
            <TaskCard 
              v-for="task in completedTasks" 
              :key="task.id"
              :task="task"
              @toggle="handleToggleTask"
              @delete="handleDeleteTask"
            />
          </div>
        </div>

        <!-- Empty State -->
        <div v-if="tasks.length === 0" class="text-center py-12">
          <p class="text-muted-foreground">No tasks yet</p>
          <p class="text-sm text-muted-foreground mt-1">Add a task to get started</p>
        </div>
      </div>
    </div>
  </div>
</template>
