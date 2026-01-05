<script setup lang="ts">
import { computed } from 'vue';
import type { WorkflowTask } from '@/services/api';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';

const props = defineProps<{
  task: WorkflowTask;
}>();

const emit = defineEmits<{
  complete: [id: number];
  delete: [id: number];
}>();

const formattedDate = computed(() => {
  const date = props.task.scheduledFor 
    ? new Date(props.task.scheduledFor) 
    : new Date(props.task.createdAt);
  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
});
</script>

<template>
  <Card :class="[
    'group transition-all duration-300 hover:shadow-xl hover:-translate-y-1 border-l-4 backdrop-blur-sm',
    task.isCompleted 
      ? 'opacity-75 border-l-green-500 bg-white/70 dark:bg-slate-800/70 hover:opacity-90' 
      : task.taskType === 'scheduled' 
        ? 'border-l-purple-500 bg-white/90 dark:bg-slate-800/90 shadow-md' 
        : 'border-l-blue-500 bg-white/90 dark:bg-slate-800/90 shadow-md'
  ]">
    <CardHeader class="pb-3">
      <div class="flex items-start gap-3">
        <div class="relative mt-1">
          <Checkbox 
            :checked="task.isCompleted"
            @update:checked="emit('complete', task.id)"
            class="transition-all duration-200 hover:scale-110"
          />
          <div v-if="task.isCompleted" class="absolute inset-0 flex items-center justify-center pointer-events-none">
            <div class="w-6 h-6 rounded-full bg-green-500/20 animate-ping"></div>
          </div>
        </div>
        <div class="flex-1 min-w-0">
          <CardTitle 
            class="text-base font-bold transition-all duration-200"
            :class="{ 
              'line-through text-muted-foreground': task.isCompleted,
              'group-hover:text-indigo-600 dark:group-hover:text-indigo-400': !task.isCompleted
            }"
          >
            {{ task.title }}
          </CardTitle>
          <p v-if="task.description" class="text-sm text-muted-foreground mt-2 line-clamp-2 leading-relaxed">
            {{ task.description }}
          </p>
        </div>
      </div>
    </CardHeader>
    <CardContent class="pt-0">
      <div class="flex items-center justify-between flex-wrap gap-3">
        <div class="flex gap-2 items-center flex-wrap">
          <Badge 
            :variant="task.status === 'completed' ? 'default' : 'outline'"
            :class="[
              'text-xs font-medium transition-all duration-200',
              task.status === 'completed' 
                ? 'bg-gradient-to-r from-green-500 to-emerald-500 text-white border-none' 
                : task.status === 'in_progress'
                  ? 'bg-gradient-to-r from-blue-500 to-indigo-500 text-white border-none'
                  : 'border-slate-300 dark:border-slate-600'
            ]"
          >
            <span class="capitalize">{{ task.status.replace('_', ' ') }}</span>
          </Badge>
          <Badge v-if="task.taskType === 'scheduled'" variant="secondary" class="text-xs gap-1.5 font-medium bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300 border-purple-200 dark:border-purple-800">
            <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect width="18" height="18" x="3" y="4" rx="2" ry="2"/>
              <line x1="16" x2="16" y1="2" y2="6"/>
              <line x1="8" x2="8" y1="2" y2="6"/>
              <line x1="3" x2="21" y1="10" y2="10"/>
            </svg>
            {{ formattedDate }}
          </Badge>
          <span v-else class="text-xs text-muted-foreground flex items-center gap-1.5 font-medium">
            <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"/>
              <polyline points="12 6 12 12 16 14"/>
            </svg>
            {{ formattedDate }}
          </span>
        </div>
        <Button 
          variant="ghost" 
          size="sm" 
          @click="emit('delete', task.id)"
          class="text-red-500 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-950/30 h-8 px-3 gap-1.5 transition-all duration-200 hover:scale-105"
        >
          <svg xmlns="http://www.w3.org/2000/svg" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M3 6h18M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/>
          </svg>
          Delete
        </Button>
      </div>
    </CardContent>
  </Card>
</template>
