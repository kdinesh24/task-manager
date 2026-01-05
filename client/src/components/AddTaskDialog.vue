<script setup lang="ts">
import { ref, computed } from 'vue';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';

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

const taskType = ref<'immediate' | 'scheduled'>('immediate');
const title = ref('');
const description = ref('');
const scheduledDate = ref('');
const scheduledTime = ref('');

const resetForm = () => {
  taskType.value = 'immediate';
  title.value = '';
  description.value = '';
  scheduledDate.value = '';
  scheduledTime.value = '';
};

const handleSubmit = () => {
  if (!title.value.trim()) return;

  let scheduledFor: string | undefined;
  if (taskType.value === 'scheduled' && scheduledDate.value) {
    const dateTime = scheduledTime.value
      ? `${scheduledDate.value}T${scheduledTime.value}`
      : `${scheduledDate.value}T09:00`;
    scheduledFor = new Date(dateTime).toISOString();
  }

  emit('create', {
    title: title.value.trim(),
    description: description.value.trim() || undefined,
    scheduledFor,
  });

  resetForm();
  isOpen.value = false;
};
</script>

<template>
  <Dialog :open="isOpen" @update:open="(val) => isOpen = val">
    <DialogTrigger as-child>
      <slot>
        <Button>Add Task</Button>
      </slot>
    </DialogTrigger>
    
    <DialogContent class="sm:max-w-[550px] bg-white/95 dark:bg-slate-900/95 backdrop-blur-xl border-slate-200 dark:border-slate-800">
      <DialogHeader>
        <DialogTitle class="text-3xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 dark:from-indigo-400 dark:to-purple-400 bg-clip-text text-transparent">
          Create New Task
        </DialogTitle>
        <DialogDescription class="text-base">
          Add a new task to your workflow. Choose between immediate or scheduled tasks.
        </DialogDescription>
      </DialogHeader>
      
      <div class="space-y-6 py-4">
        <!-- Task Type Selection -->
        <div class="space-y-3">
          <label class="text-sm font-semibold text-slate-700 dark:text-slate-300">Task Type</label>
          <div class="grid grid-cols-2 gap-3">
            <Button 
              :variant="taskType === 'immediate' ? 'default' : 'outline'"
              @click="taskType = 'immediate'"
              :class="[
                'gap-2 h-auto py-4 transition-all duration-200',
                taskType === 'immediate' 
                  ? 'bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 shadow-lg scale-105' 
                  : 'hover:border-blue-400 hover:bg-blue-50 dark:hover:bg-blue-950/30'
              ]"
            >
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="12" cy="12" r="10"/>
                <polyline points="12 6 12 12 16 14"/>
              </svg>
              <span class="font-semibold">Immediate</span>
            </Button>
            <Button 
              :variant="taskType === 'scheduled' ? 'default' : 'outline'"
              @click="taskType = 'scheduled'"
              :class="[
                'gap-2 h-auto py-4 transition-all duration-200',
                taskType === 'scheduled' 
                  ? 'bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 shadow-lg scale-105' 
                  : 'hover:border-purple-400 hover:bg-purple-50 dark:hover:bg-purple-950/30'
              ]"
            >
              <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect width="18" height="18" x="3" y="4" rx="2" ry="2"/>
                <line x1="16" x2="16" y1="2" y2="6"/>
                <line x1="8" x2="8" y1="2" y2="6"/>
                <line x1="3" x2="21" y1="10" y2="10"/>
              </svg>
              <span class="font-semibold">Scheduled</span>
            </Button>
          </div>
        </div>
        
        <!-- Title -->
        <div class="space-y-3">
          <label class="text-sm font-semibold text-slate-700 dark:text-slate-300 flex items-center gap-1">
            Title 
            <span class="text-red-500 text-base">*</span>
          </label>
          <Input 
            v-model="title" 
            placeholder="Enter task title" 
            class="h-12 text-base border-slate-300 dark:border-slate-700 focus:border-indigo-500 focus:ring-indigo-500"
          />
        </div>
        
        <!-- Description -->
        <div class="space-y-3">
          <label class="text-sm font-semibold text-slate-700 dark:text-slate-300">Description</label>
          <Textarea 
            v-model="description" 
            placeholder="Add task description (optional)" 
            rows="4"
            class="resize-none text-base border-slate-300 dark:border-slate-700 focus:border-indigo-500 focus:ring-indigo-500"
          />
        </div>
        
        <!-- Schedule Date/Time -->
        <div v-if="taskType === 'scheduled'" class="space-y-3 p-4 bg-purple-50 dark:bg-purple-950/20 rounded-lg border border-purple-200 dark:border-purple-900/50">
          <label class="text-sm font-semibold text-purple-700 dark:text-purple-300 flex items-center gap-2">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect width="18" height="18" x="3" y="4" rx="2" ry="2"/>
              <line x1="16" x2="16" y1="2" y2="6"/>
              <line x1="8" x2="8" y1="2" y2="6"/>
              <line x1="3" x2="21" y1="10" y2="10"/>
            </svg>
            Schedule For
          </label>
          <div class="grid grid-cols-2 gap-3">
            <div class="space-y-2">
              <Input 
                v-model="scheduledDate" 
                type="date" 
                class="h-12 text-base"
              />
              <p class="text-xs text-muted-foreground font-medium">Date</p>
            </div>
            <div class="space-y-2">
              <Input 
                v-model="scheduledTime" 
                type="time" 
                class="h-12 text-base"
              />
              <p class="text-xs text-muted-foreground font-medium">Time</p>
            </div>
          </div>
        </div>
      </div>
      
      <DialogFooter class="gap-3">
        <Button 
          variant="outline" 
          @click="isOpen = false" 
          class="gap-2 h-11 px-6 hover:bg-slate-100 dark:hover:bg-slate-800"
        >
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M18 6 6 18M6 6l12 12"/>
          </svg>
          Cancel
        </Button>
        <Button 
          @click="handleSubmit" 
          :disabled="!title.trim()"
          class="gap-2 h-11 px-6 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-700 hover:to-purple-700 shadow-lg hover:shadow-xl transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M5 12h14M12 5v14"/>
          </svg>
          Create Task
        </Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
