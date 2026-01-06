<script setup lang="ts">
import { computed } from 'vue';
import type { WorkflowTask } from '@/services/api';

const props = defineProps<{
  task: WorkflowTask;
}>();

const emit = defineEmits<{
  toggle: [id: number, isCompleted: boolean];
  delete: [id: number];
}>();

const formattedDate = computed(() => {
  const date = props.task.scheduledFor 
    ? new Date(props.task.scheduledFor) 
    : new Date(props.task.createdAt);
  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
  });
});

const handleToggle = () => {
  emit('toggle', props.task.id, props.task.isCompleted);
};
</script>

<template>
  <div 
    class="group flex items-start gap-3 p-4 bg-card border rounded-lg"
    :class="{ 'opacity-50': task.isCompleted }"
  >
    <!-- Toggle Button -->
    <button
      @click="handleToggle"
      class="mt-0.5 w-5 h-5 rounded-full border-2 flex items-center justify-center flex-shrink-0 hover:scale-110 transition-transform"
      :class="task.isCompleted 
        ? 'bg-foreground border-foreground' 
        : 'border-muted-foreground hover:border-foreground'"
    >
      <svg 
        v-if="task.isCompleted" 
        class="w-3 h-3 text-background" 
        fill="none" 
        stroke="currentColor" 
        viewBox="0 0 24 24"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M5 13l4 4L19 7"/>
      </svg>
    </button>

    <!-- Content -->
    <div class="flex-1 min-w-0">
      <h3 
        class="font-medium text-foreground"
        :class="{ 'line-through text-muted-foreground': task.isCompleted }"
      >
        {{ task.title }}
      </h3>
      <p v-if="task.description" class="text-sm text-muted-foreground mt-1 truncate">
        {{ task.description }}
      </p>
      <div class="flex items-center gap-3 mt-2 text-xs text-muted-foreground">
        <span>{{ formattedDate }}</span>
        <span v-if="task.taskType === 'scheduled'" class="px-1.5 py-0.5 bg-secondary rounded text-xs">
          Scheduled
        </span>
      </div>
    </div>

    <!-- Delete Button -->
    <button
      @click="emit('delete', task.id)"
      class="opacity-0 group-hover:opacity-100 p-1 text-muted-foreground hover:text-foreground transition-opacity"
    >
      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
      </svg>
    </button>
  </div>
</template>
