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
  <Card :class="{ 'opacity-60': task.isCompleted }">
    <CardHeader class="pb-2">
      <div class="flex items-start gap-3">
        <Checkbox 
          :checked="task.isCompleted"
          @update:checked="emit('complete', task.id)"
          class="mt-1"
        />
        <div class="flex-1">
          <CardTitle 
            class="text-sm"
            :class="{ 'line-through': task.isCompleted }"
          >
            {{ task.title }}
          </CardTitle>
          <p v-if="task.description" class="text-xs text-muted-foreground mt-1">
            {{ task.description }}
          </p>
        </div>
      </div>
    </CardHeader>
    <CardContent>
      <div class="flex items-center justify-between">
        <div class="flex gap-2">
          <Badge variant="outline">{{ task.status }}</Badge>
          <Badge v-if="task.taskType === 'scheduled'" variant="secondary">
            Scheduled
          </Badge>
        </div>
        <div class="flex items-center gap-2">
          <span class="text-xs text-muted-foreground">{{ formattedDate }}</span>
          <Button variant="ghost" size="sm" @click="emit('delete', task.id)">
            Delete
          </Button>
        </div>
      </div>
    </CardContent>
  </Card>
</template>
