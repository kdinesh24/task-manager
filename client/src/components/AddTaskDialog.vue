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
    
    <DialogContent>
      <DialogHeader>
        <DialogTitle>Create New Task</DialogTitle>
        <DialogDescription>
          Add a new task to your workflow.
        </DialogDescription>
      </DialogHeader>
      
      <div class="space-y-4 py-4">
        <div class="flex gap-2">
          <Button 
            :variant="taskType === 'immediate' ? 'default' : 'outline'"
            @click="taskType = 'immediate'"
          >
            Immediate
          </Button>
          <Button 
            :variant="taskType === 'scheduled' ? 'default' : 'outline'"
            @click="taskType = 'scheduled'"
          >
            Scheduled
          </Button>
        </div>
        
        <div>
          <label class="text-sm font-medium">Title</label>
          <Input v-model="title" placeholder="Task title" />
        </div>
        
        <div>
          <label class="text-sm font-medium">Description</label>
          <Textarea v-model="description" placeholder="Description (optional)" />
        </div>
        
        <div v-if="taskType === 'scheduled'" class="flex gap-2">
          <div class="flex-1">
            <label class="text-sm font-medium">Date</label>
            <Input v-model="scheduledDate" type="date" />
          </div>
          <div class="flex-1">
            <label class="text-sm font-medium">Time</label>
            <Input v-model="scheduledTime" type="time" />
          </div>
        </div>
      </div>
      
      <DialogFooter>
        <Button variant="outline" @click="isOpen = false">Cancel</Button>
        <Button @click="handleSubmit" :disabled="!title.trim()">Create</Button>
      </DialogFooter>
    </DialogContent>
  </Dialog>
</template>
