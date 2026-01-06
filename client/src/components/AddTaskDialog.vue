<script setup lang="ts">
import { ref, computed } from 'vue';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';

const props = defineProps<{
  open: boolean;
}>();

const emit = defineEmits<{
  'update:open': [value: boolean];
  create: [task: { title: string; description?: string; scheduledFor?: string }];
}>();

const isOpen = computed({
  get: () => props.open,
  set: (value) => emit('update:open', value),
});

const title = ref('');
const description = ref('');
const scheduledDate = ref('');

const resetForm = () => {
  title.value = '';
  description.value = '';
  scheduledDate.value = '';
};

const handleSubmit = () => {
  if (!title.value.trim()) return;

  emit('create', {
    title: title.value.trim(),
    description: description.value.trim() || undefined,
    scheduledFor: scheduledDate.value ? new Date(scheduledDate.value).toISOString() : undefined,
  });

  resetForm();
  isOpen.value = false;
};
</script>

<template>
  <Dialog :open="isOpen" @update:open="(val) => isOpen = val">
    <DialogTrigger as-child>
      <slot />
    </DialogTrigger>
    
    <DialogContent class="sm:max-w-md">
      <DialogHeader>
        <DialogTitle>New Task</DialogTitle>
      </DialogHeader>
      
      <form @submit.prevent="handleSubmit" class="space-y-4 mt-4">
        <div>
          <input 
            v-model="title" 
            type="text"
            placeholder="Task title"
            class="w-full px-3 py-2 border rounded-lg bg-background text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring"
            autofocus
          />
        </div>
        
        <div>
          <textarea 
            v-model="description" 
            placeholder="Description (optional)"
            rows="3"
            class="w-full px-3 py-2 border rounded-lg bg-background text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring resize-none"
          />
        </div>
        
        <div>
          <label class="text-sm text-muted-foreground">Schedule for (optional)</label>
          <input 
            v-model="scheduledDate" 
            type="datetime-local"
            class="w-full mt-1 px-3 py-2 border rounded-lg bg-background text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </div>
        
        <div class="flex gap-3 pt-2">
          <button 
            type="button"
            @click="isOpen = false" 
            class="flex-1 px-4 py-2 border rounded-lg text-sm font-medium hover:bg-secondary"
          >
            Cancel
          </button>
          <button 
            type="submit"
            :disabled="!title.trim()"
            class="flex-1 px-4 py-2 bg-foreground text-background rounded-lg text-sm font-medium hover:bg-foreground/90 disabled:opacity-50"
          >
            Add Task
          </button>
        </div>
      </form>
    </DialogContent>
  </Dialog>
</template>
