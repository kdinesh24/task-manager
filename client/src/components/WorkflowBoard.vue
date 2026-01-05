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
  <div class="min-h-screen bg-gradient-to-br from-indigo-50 via-white to-purple-50 dark:from-slate-950 dark:via-slate-900 dark:to-indigo-950 p-4 md:p-8 relative overflow-hidden">
    <!-- Animated background elements -->
    <div class="absolute inset-0 overflow-hidden pointer-events-none">
      <div class="absolute top-20 left-10 w-72 h-72 bg-purple-300 dark:bg-purple-900 rounded-full mix-blend-multiply dark:mix-blend-soft-light filter blur-3xl opacity-20 animate-blob"></div>
      <div class="absolute top-40 right-10 w-72 h-72 bg-indigo-300 dark:bg-indigo-900 rounded-full mix-blend-multiply dark:mix-blend-soft-light filter blur-3xl opacity-20 animate-blob animation-delay-2000"></div>
      <div class="absolute -bottom-8 left-20 w-72 h-72 bg-pink-300 dark:bg-pink-900 rounded-full mix-blend-multiply dark:mix-blend-soft-light filter blur-3xl opacity-20 animate-blob animation-delay-4000"></div>
    </div>

    <div class="max-w-7xl mx-auto relative z-10">
      <!-- Header -->
      <div class="mb-8 animate-fade-in-down">
        <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-6">
          <div>
            <h1 class="text-5xl font-extrabold bg-gradient-to-r from-indigo-600 via-purple-600 to-pink-600 dark:from-indigo-400 dark:via-purple-400 dark:to-pink-400 bg-clip-text text-transparent mb-2">
              Workflow Manager
            </h1>
            <p class="text-sm text-muted-foreground flex items-center gap-2">
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-purple-500">
                <path d="M9 11 12 14 22 4"/>
                <path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11"/>
              </svg>
              Organize and track your tasks efficiently
            </p>
          </div>
          <div class="flex gap-3">
            <Button 
              variant="outline" 
              @click="fetchTasks" 
              :disabled="loading"
              class="gap-2 backdrop-blur-sm bg-white/80 dark:bg-slate-800/80 hover:bg-white dark:hover:bg-slate-800 transition-all duration-200 shadow-sm hover:shadow-md"
            >
              <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" :class="{ 'animate-spin': loading }">
                <path d="M21.5 2v6h-6M2.5 22v-6h6M2 11.5a10 10 0 0 1 18.8-4.3M22 12.5a10 10 0 0 1-18.8 4.2"/>
              </svg>
              Refresh
            </Button>
            <AddTaskDialog v-model:open="dialogOpen" @create="handleCreateTask">
              <Button class="gap-2 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 shadow-md hover:shadow-xl transition-all duration-200">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                  <path d="M5 12h14M12 5v14"/>
                </svg>
                Add Task
              </Button>
            </AddTaskDialog>
          </div>
        </div>

        <!-- Stats Overview -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 animate-fade-in-up">
          <div class="group bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-blue-950/30 dark:to-indigo-950/30 backdrop-blur-sm rounded-xl p-5 border border-blue-100 dark:border-blue-900/50 hover:shadow-lg hover:scale-[1.02] transition-all duration-300 cursor-pointer">
            <div class="flex items-center justify-between mb-2">
              <div class="p-2.5 bg-blue-100 dark:bg-blue-900/50 rounded-lg group-hover:scale-110 transition-transform duration-300">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-blue-600 dark:text-blue-400">
                  <circle cx="12" cy="12" r="10"/>
                  <polyline points="12 6 12 12 16 14"/>
                </svg>
              </div>
              <div class="text-3xl font-bold text-blue-600 dark:text-blue-400">{{ immediateTasks.length }}</div>
            </div>
            <div class="text-sm font-medium text-blue-700 dark:text-blue-300">Immediate Tasks</div>
            <div class="text-xs text-muted-foreground mt-1">Pending action</div>
          </div>
          <div class="group bg-gradient-to-br from-purple-50 to-pink-50 dark:from-purple-950/30 dark:to-pink-950/30 backdrop-blur-sm rounded-xl p-5 border border-purple-100 dark:border-purple-900/50 hover:shadow-lg hover:scale-[1.02] transition-all duration-300 cursor-pointer">
            <div class="flex items-center justify-between mb-2">
              <div class="p-2.5 bg-purple-100 dark:bg-purple-900/50 rounded-lg group-hover:scale-110 transition-transform duration-300">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-purple-600 dark:text-purple-400">
                  <rect width="18" height="18" x="3" y="4" rx="2" ry="2"/>
                  <line x1="16" x2="16" y1="2" y2="6"/>
                  <line x1="8" x2="8" y1="2" y2="6"/>
                  <line x1="3" x2="21" y1="10" y2="10"/>
                </svg>
              </div>
              <div class="text-3xl font-bold text-purple-600 dark:text-purple-400">{{ scheduledTasks.length }}</div>
            </div>
            <div class="text-sm font-medium text-purple-700 dark:text-purple-300">Scheduled Tasks</div>
            <div class="text-xs text-muted-foreground mt-1">Future planned</div>
          </div>
          <div class="group bg-gradient-to-br from-green-50 to-emerald-50 dark:from-green-950/30 dark:to-emerald-950/30 backdrop-blur-sm rounded-xl p-5 border border-green-100 dark:border-green-900/50 hover:shadow-lg hover:scale-[1.02] transition-all duration-300 cursor-pointer">
            <div class="flex items-center justify-between mb-2">
              <div class="p-2.5 bg-green-100 dark:bg-green-900/50 rounded-lg group-hover:scale-110 transition-transform duration-300">
                <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-green-600 dark:text-green-400">
                  <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                  <path d="m9 11 3 3L22 4"/>
                </svg>
              </div>
              <div class="text-3xl font-bold text-green-600 dark:text-green-400">{{ completedTasks.length }}</div>
            </div>
            <div class="text-sm font-medium text-green-700 dark:text-green-300">Completed Tasks</div>
            <div class="text-xs text-muted-foreground mt-1">Well done!</div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="flex items-center justify-center py-20">
        <div class="text-center">
          <div class="relative w-16 h-16 mx-auto mb-4">
            <div class="absolute inset-0 border-4 border-indigo-200 dark:border-indigo-900 rounded-full"></div>
            <div class="absolute inset-0 border-4 border-transparent border-t-indigo-600 dark:border-t-indigo-400 rounded-full animate-spin"></div>
          </div>
          <p class="text-muted-foreground font-medium">Loading your tasks...</p>
        </div>
      </div>

      <!-- Task Columns -->
      <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Immediate Tasks -->
        <div class="space-y-4 animate-fade-in-up animation-delay-100">
          <div class="flex items-center gap-3 mb-4 px-1">
            <div class="w-1 h-8 rounded-full bg-gradient-to-b from-blue-500 to-indigo-500"></div>
            <div>
              <h2 class="font-bold text-lg text-slate-900 dark:text-slate-100">Immediate</h2>
              <p class="text-xs text-muted-foreground">{{ immediateTasks.length }} task{{ immediateTasks.length !== 1 ? 's' : '' }}</p>
            </div>
          </div>
          <div class="space-y-3 min-h-[300px]">
            <TransitionGroup name="task-list">
              <TaskCard 
                v-for="task in immediateTasks" 
                :key="task.id"
                :task="task"
                @complete="handleCompleteTask"
                @delete="handleDeleteTask"
              />
            </TransitionGroup>
            <div v-if="!immediateTasks.length" class="group bg-white/60 dark:bg-slate-800/60 backdrop-blur-sm border-2 border-dashed border-slate-300 dark:border-slate-700 hover:border-blue-400 dark:hover:border-blue-600 rounded-xl p-10 text-center transition-all duration-300 hover:bg-white/80 dark:hover:bg-slate-800/80">
              <svg xmlns="http://www.w3.org/2000/svg" width="56" height="56" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" class="mx-auto mb-3 text-slate-400 dark:text-slate-600 group-hover:text-blue-400 transition-colors">
                <circle cx="12" cy="12" r="10"/>
                <polyline points="12 6 12 12 16 14"/>
              </svg>
              <p class="text-muted-foreground text-sm font-medium">No immediate tasks</p>
              <p class="text-xs text-muted-foreground/70 mt-1">Start adding tasks to get organized</p>
            </div>
          </div>
        </div>

        <!-- Scheduled Tasks -->
        <div class="space-y-4 animate-fade-in-up animation-delay-200">
          <div class="flex items-center gap-3 mb-4 px-1">
            <div class="w-1 h-8 rounded-full bg-gradient-to-b from-purple-500 to-pink-500"></div>
            <div>
              <h2 class="font-bold text-lg text-slate-900 dark:text-slate-100">Scheduled</h2>
              <p class="text-xs text-muted-foreground">{{ scheduledTasks.length }} task{{ scheduledTasks.length !== 1 ? 's' : '' }}</p>
            </div>
          </div>
          <div class="space-y-3 min-h-[300px]">
            <TransitionGroup name="task-list">
              <TaskCard 
                v-for="task in scheduledTasks" 
                :key="task.id"
                :task="task"
                @complete="handleCompleteTask"
                @delete="handleDeleteTask"
              />
            </TransitionGroup>
            <div v-if="!scheduledTasks.length" class="group bg-white/60 dark:bg-slate-800/60 backdrop-blur-sm border-2 border-dashed border-slate-300 dark:border-slate-700 hover:border-purple-400 dark:hover:border-purple-600 rounded-xl p-10 text-center transition-all duration-300 hover:bg-white/80 dark:hover:bg-slate-800/80">
              <svg xmlns="http://www.w3.org/2000/svg" width="56" height="56" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" class="mx-auto mb-3 text-slate-400 dark:text-slate-600 group-hover:text-purple-400 transition-colors">
                <rect width="18" height="18" x="3" y="4" rx="2" ry="2"/>
                <line x1="16" x2="16" y1="2" y2="6"/>
                <line x1="8" x2="8" y1="2" y2="6"/>
                <line x1="3" x2="21" y1="10" y2="10"/>
              </svg>
              <p class="text-muted-foreground text-sm font-medium">No scheduled tasks</p>
              <p class="text-xs text-muted-foreground/70 mt-1">Plan ahead for upcoming work</p>
            </div>
          </div>
        </div>

        <!-- Completed Tasks -->
        <div class="space-y-4 animate-fade-in-up animation-delay-300">
          <div class="flex items-center gap-3 mb-4 px-1">
            <div class="w-1 h-8 rounded-full bg-gradient-to-b from-green-500 to-emerald-500"></div>
            <div>
              <h2 class="font-bold text-lg text-slate-900 dark:text-slate-100">Completed</h2>
              <p class="text-xs text-muted-foreground">{{ completedTasks.length }} task{{ completedTasks.length !== 1 ? 's' : '' }}</p>
            </div>
          </div>
          <div class="space-y-3 min-h-[300px]">
            <TransitionGroup name="task-list">
              <TaskCard 
                v-for="task in completedTasks" 
                :key="task.id"
                :task="task"
                @complete="handleCompleteTask"
                @delete="handleDeleteTask"
              />
            </TransitionGroup>
            <div v-if="!completedTasks.length" class="group bg-white/60 dark:bg-slate-800/60 backdrop-blur-sm border-2 border-dashed border-slate-300 dark:border-slate-700 hover:border-green-400 dark:hover:border-green-600 rounded-xl p-10 text-center transition-all duration-300 hover:bg-white/80 dark:hover:bg-slate-800/80">
              <svg xmlns="http://www.w3.org/2000/svg" width="56" height="56" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" class="mx-auto mb-3 text-slate-400 dark:text-slate-600 group-hover:text-green-400 transition-colors">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                <path d="m9 11 3 3L22 4"/>
              </svg>
              <p class="text-muted-foreground text-sm font-medium">No completed tasks</p>
              <p class="text-xs text-muted-foreground/70 mt-1">Complete tasks to see them here</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
