const API_BASE_URL = 'http://localhost:5000/api';

export interface WorkflowTask {
  id: number;
  title: string;
  description?: string;
  createdAt: string;
  scheduledFor?: string;
  isCompleted: boolean;
  completedAt?: string;
  taskType: 'immediate' | 'scheduled';
  status: 'pending' | 'in_progress' | 'completed';
}

export interface CreateTaskDto {
  title: string;
  description?: string;
  scheduledFor?: string;
}

export interface UpdateTaskDto {
  title?: string;
  description?: string;
  status?: string;
  scheduledFor?: string;
}

export const taskApi = {
  async getAllTasks(): Promise<WorkflowTask[]> {
    const response = await fetch(`${API_BASE_URL}/tasks`);
    if (!response.ok) throw new Error('Failed to fetch tasks');
    return response.json();
  },

  async createTask(task: CreateTaskDto): Promise<WorkflowTask> {
    const response = await fetch(`${API_BASE_URL}/tasks`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(task),
    });
    if (!response.ok) throw new Error('Failed to create task');
    return response.json();
  },

  async updateTask(id: number, task: UpdateTaskDto): Promise<WorkflowTask> {
    const response = await fetch(`${API_BASE_URL}/tasks/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(task),
    });
    if (!response.ok) throw new Error('Failed to update task');
    return response.json();
  },

  async completeTask(id: number): Promise<WorkflowTask> {
    const response = await fetch(`${API_BASE_URL}/tasks/${id}/complete`, {
      method: 'PUT',
    });
    if (!response.ok) throw new Error('Failed to complete task');
    return response.json();
  },

  async deleteTask(id: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/tasks/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete task');
  },
};
