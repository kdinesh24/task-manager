<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import type { WorkflowTask, CreateTaskDto } from '@/services/api';
import { taskApi } from '@/services/api';
import TaskCard from '@/components/TaskCard.vue';
import AddTaskDialog from '@/components/AddTaskDialog.vue';
import { Button } from '@/components/ui/button';

const tasks = ref<WorkflowTask[]>([]);
const loading = ref(true);
const dialogOpen = ref(false);

const immediateTasks = computed(() => 
  tasks.value.filter(t => t.taskType === 'immediate' && !t.isCompleted)
);

const scheduledTasks = computed(() => 
  tasks.value.filter(t => t.taskType === 'scheduled' && !t.isCompleted)
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

const handleCompleteTask = async (id: number) => {
  try {
    const updatedTask = await taskApi.completeTask(id);
    const index = tasks.value.findIndex(t => t.id === id);
    if (index !== -1) {
      tasks.value[index] = updatedTask;
    }
  } catch (error) {
    console.error('Failed to complete task:', error);
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
  <div class="min-h-screen bg-background p-6">
    <div class="max-w-6xl mx-auto">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <h1 class="text-2xl font-bold">Workflow</h1>
        <div class="flex gap-2">
          <Button variant="outline" @click="fetchTasks" :disabled="loading">
            Refresh
          </Button>
          <AddTaskDialog v-model:open="dialogOpen" @create="handleCreateTask">
            <Button>Add Task</Button>
          </AddTaskDialog>
        </div>
      </div>

      <!-- Task Columns -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <!-- Immediate Tasks -->
        <div>
          <h2 class="font-semibold mb-4">Immediate ({{ immediateTasks.length }})</h2>
          <div class="space-y-3">
            <TaskCard 
              v-for="task in immediateTasks" 
              :key="task.id"
              :task="task"
              @complete="handleCompleteTask"
              @delete="handleDeleteTask"
            />
            <p v-if="!immediateTasks.length" class="text-muted-foreground text-sm">
              No immediate tasks
            </p>
          </div>
        </div>

        <!-- Scheduled Tasks -->
        <div>
          <h2 class="font-semibold mb-4">Scheduled ({{ scheduledTasks.length }})</h2>
          <div class="space-y-3">
            <TaskCard 
              v-for="task in scheduledTasks" 
              :key="task.id"
              :task="task"
              @complete="handleCompleteTask"
              @delete="handleDeleteTask"
            />
            <p v-if="!scheduledTasks.length" class="text-muted-foreground text-sm">
              No scheduled tasks
            </p>
          </div>
        </div>

        <!-- Completed Tasks -->
        <div>
          <h2 class="font-semibold mb-4">Completed ({{ completedTasks.length }})</h2>
          <div class="space-y-3">
            <TaskCard 
              v-for="task in completedTasks" 
              :key="task.id"
              :task="task"
              @complete="handleCompleteTask"
              @delete="handleDeleteTask"
            />
            <p v-if="!completedTasks.length" class="text-muted-foreground text-sm">
              No completed tasks
            </p>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-8">
        <p class="text-muted-foreground">Loading tasks...</p>
      </div>
    </div>
  </div>
</template>
