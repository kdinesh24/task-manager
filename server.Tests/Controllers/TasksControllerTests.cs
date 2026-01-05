using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using server.Controllers;
using server.Data;
using server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace server.Tests.Controllers
{
    public class TasksControllerTests : IDisposable
    {
        private readonly WorkflowDbContext _context;
        private readonly TasksController _controller;

        public TasksControllerTests()
        {
            var options = new DbContextOptionsBuilder<WorkflowDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new WorkflowDbContext(options);
            _controller = new TasksController(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetTasks_ReturnsEmptyList_WhenNoTasksExist()
        {
            // Act
            var result = await _controller.GetTasks();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<WorkflowTask>>>(result);
            var tasks = Assert.IsType<List<WorkflowTask>>(okResult.Value);
            Assert.Empty(tasks);
        }

        [Fact]
        public async Task GetTasks_ReturnsTasksOrderedByCreatedAtDescending()
        {
            // Arrange
            var task1 = new WorkflowTask { Title = "Task 1", CreatedAt = DateTime.UtcNow.AddMinutes(-10) };
            var task2 = new WorkflowTask { Title = "Task 2", CreatedAt = DateTime.UtcNow.AddMinutes(-5) };
            var task3 = new WorkflowTask { Title = "Task 3", CreatedAt = DateTime.UtcNow };

            _context.Tasks.AddRange(task1, task2, task3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetTasks();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<WorkflowTask>>>(result);
            var tasks = Assert.IsType<List<WorkflowTask>>(okResult.Value);
            Assert.Equal(3, tasks.Count);
            Assert.Equal("Task 3", tasks[0].Title);
            Assert.Equal("Task 2", tasks[1].Title);
            Assert.Equal("Task 1", tasks[2].Title);
        }

        [Fact]
        public async Task GetTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Act
            var result = await _controller.GetTask(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetTask_ReturnsTask_WhenTaskExists()
        {
            // Arrange
            var task = new WorkflowTask { Title = "Test Task" };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetTask(task.Id);

            // Assert
            var okResult = Assert.IsType<ActionResult<WorkflowTask>>(result);
            var returnedTask = Assert.IsType<WorkflowTask>(okResult.Value);
            Assert.Equal(task.Id, returnedTask.Id);
            Assert.Equal("Test Task", returnedTask.Title);
        }

        [Fact]
        public async Task GetImmediateTasks_ReturnsOnlyImmediateTasks()
        {
            // Arrange
            var immediateTask1 = new WorkflowTask { Title = "Immediate 1", TaskType = "immediate" };
            var immediateTask2 = new WorkflowTask { Title = "Immediate 2", TaskType = "immediate" };
            var scheduledTask = new WorkflowTask { Title = "Scheduled", TaskType = "scheduled" };

            _context.Tasks.AddRange(immediateTask1, immediateTask2, scheduledTask);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetImmediateTasks();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<WorkflowTask>>>(result);
            var tasks = Assert.IsType<List<WorkflowTask>>(okResult.Value);
            Assert.Equal(2, tasks.Count);
            Assert.All(tasks, t => Assert.Equal("immediate", t.TaskType));
        }

        [Fact]
        public async Task GetScheduledTasks_ReturnsOnlyScheduledTasks()
        {
            // Arrange
            var immediateTask = new WorkflowTask { Title = "Immediate", TaskType = "immediate" };
            var scheduledTask1 = new WorkflowTask { Title = "Scheduled 1", TaskType = "scheduled", ScheduledFor = DateTime.UtcNow.AddDays(1) };
            var scheduledTask2 = new WorkflowTask { Title = "Scheduled 2", TaskType = "scheduled", ScheduledFor = DateTime.UtcNow.AddDays(2) };

            _context.Tasks.AddRange(immediateTask, scheduledTask1, scheduledTask2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetScheduledTasks();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<WorkflowTask>>>(result);
            var tasks = Assert.IsType<List<WorkflowTask>>(okResult.Value);
            Assert.Equal(2, tasks.Count);
            Assert.All(tasks, t => Assert.Equal("scheduled", t.TaskType));
        }

        [Fact]
        public async Task CreateTask_CreatesImmediateTask_WhenNoScheduledForProvided()
        {
            // Arrange
            var dto = new CreateTaskDto { Title = "New Immediate Task", Description = "Description" };

            // Act
            var result = await _controller.CreateTask(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var task = Assert.IsType<WorkflowTask>(createdAtActionResult.Value);
            Assert.Equal("New Immediate Task", task.Title);
            Assert.Equal("immediate", task.TaskType);
            Assert.Equal("pending", task.Status);
            Assert.Null(task.ScheduledFor);
        }

        [Fact]
        public async Task CreateTask_CreatesScheduledTask_WhenScheduledForProvided()
        {
            // Arrange
            var scheduledTime = DateTime.UtcNow.AddDays(1);
            var dto = new CreateTaskDto { Title = "New Scheduled Task", ScheduledFor = scheduledTime };

            // Act
            var result = await _controller.CreateTask(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var task = Assert.IsType<WorkflowTask>(createdAtActionResult.Value);
            Assert.Equal("New Scheduled Task", task.Title);
            Assert.Equal("scheduled", task.TaskType);
            Assert.Equal(scheduledTime, task.ScheduledFor);
        }

        [Fact]
        public async Task UpdateTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var dto = new UpdateTaskDto { Title = "Updated Title" };

            // Act
            var result = await _controller.UpdateTask(999, dto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateTask_UpdatesTask_WhenTaskExists()
        {
            // Arrange
            var task = new WorkflowTask { Title = "Original Title", Status = "pending" };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var dto = new UpdateTaskDto { Title = "Updated Title", Status = "completed" };

            // Act
            var result = await _controller.UpdateTask(task.Id, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedTask = Assert.IsType<WorkflowTask>(okResult.Value);
            Assert.Equal("Updated Title", updatedTask.Title);
            Assert.Equal("completed", updatedTask.Status);
            Assert.True(updatedTask.IsCompleted);
            Assert.NotNull(updatedTask.CompletedAt);
        }

        [Fact]
        public async Task CompleteTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Act
            var result = await _controller.CompleteTask(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CompleteTask_CompletesTask_WhenTaskExists()
        {
            // Arrange
            var task = new WorkflowTask { Title = "Task to Complete", Status = "pending" };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.CompleteTask(task.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var completedTask = Assert.IsType<WorkflowTask>(okResult.Value);
            Assert.True(completedTask.IsCompleted);
            Assert.Equal("completed", completedTask.Status);
            Assert.NotNull(completedTask.CompletedAt);
        }

        [Fact]
        public async Task DeleteTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteTask(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTask_DeletesTask_WhenTaskExists()
        {
            // Arrange
            var task = new WorkflowTask { Title = "Task to Delete" };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteTask(task.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var deletedTask = await _context.Tasks.FindAsync(task.Id);
            Assert.Null(deletedTask);
        }

        [Fact]
        public async Task UpdateTask_UpdatesOnlyTitle_WhenOnlyTitleProvided()
        {
            // Arrange
            var task = new WorkflowTask { Title = "Original", Description = "Original Desc", Status = "pending" };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var dto = new UpdateTaskDto { Title = "Updated Title" };

            // Act
            var result = await _controller.UpdateTask(task.Id, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedTask = Assert.IsType<WorkflowTask>(okResult.Value);
            Assert.Equal("Updated Title", updatedTask.Title);
            Assert.Equal("Original Desc", updatedTask.Description);
            Assert.Equal("pending", updatedTask.Status);
        }

        [Fact]
        public async Task UpdateTask_UpdatesScheduledFor_WhenScheduledForProvided()
        {
            // Arrange
            var task = new WorkflowTask { Title = "Task", TaskType = "immediate" };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var newScheduledTime = DateTime.UtcNow.AddDays(3);
            var dto = new UpdateTaskDto { ScheduledFor = newScheduledTime };

            // Act
            var result = await _controller.UpdateTask(task.Id, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedTask = Assert.IsType<WorkflowTask>(okResult.Value);
            Assert.Equal(newScheduledTime, updatedTask.ScheduledFor);
            Assert.Equal("scheduled", updatedTask.TaskType);
        }

        [Fact]
        public async Task UpdateTask_SetsIsCompletedAndCompletedAt_WhenStatusIsCompleted()
        {
            // Arrange
            var task = new WorkflowTask { Title = "Task", Status = "pending", IsCompleted = false };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var dto = new UpdateTaskDto { Status = "completed" };

            // Act
            var result = await _controller.UpdateTask(task.Id, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedTask = Assert.IsType<WorkflowTask>(okResult.Value);
            Assert.Equal("completed", updatedTask.Status);
            Assert.True(updatedTask.IsCompleted);
            Assert.NotNull(updatedTask.CompletedAt);
        }

        [Fact]
        public async Task UpdateTask_UpdatesDescription_WhenDescriptionProvided()
        {
            // Arrange
            var task = new WorkflowTask { Title = "Task", Description = "Original" };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var dto = new UpdateTaskDto { Description = "Updated Description" };

            // Act
            var result = await _controller.UpdateTask(task.Id, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedTask = Assert.IsType<WorkflowTask>(okResult.Value);
            Assert.Equal("Updated Description", updatedTask.Description);
        }

        [Fact]
        public async Task GetScheduledTasks_OrdersByScheduledFor()
        {
            // Arrange
            var task1 = new WorkflowTask { Title = "Task 1", TaskType = "scheduled", ScheduledFor = DateTime.UtcNow.AddDays(3) };
            var task2 = new WorkflowTask { Title = "Task 2", TaskType = "scheduled", ScheduledFor = DateTime.UtcNow.AddDays(1) };
            var task3 = new WorkflowTask { Title = "Task 3", TaskType = "scheduled", ScheduledFor = DateTime.UtcNow.AddDays(2) };

            _context.Tasks.AddRange(task1, task2, task3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetScheduledTasks();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<WorkflowTask>>>(result);
            var tasks = Assert.IsType<List<WorkflowTask>>(okResult.Value);
            Assert.Equal(3, tasks.Count);
            Assert.Equal("Task 2", tasks[0].Title); // Earliest
            Assert.Equal("Task 3", tasks[1].Title);
            Assert.Equal("Task 1", tasks[2].Title); // Latest
        }

        [Fact]
        public async Task CreateTask_SetsTaskTypeToScheduled_WhenScheduledForProvided()
        {
            // Arrange
            var scheduledTime = DateTime.UtcNow.AddDays(1);
            var dto = new CreateTaskDto { Title = "Scheduled Task", ScheduledFor = scheduledTime };

            // Act
            var result = await _controller.CreateTask(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var task = Assert.IsType<WorkflowTask>(createdAtActionResult.Value);
            Assert.Equal("scheduled", task.TaskType);
            Assert.Equal(scheduledTime, task.ScheduledFor);
        }

        [Fact]
        public async Task CreateTask_SetsCreatedAtToUtcNow()
        {
            // Arrange
            var dto = new CreateTaskDto { Title = "New Task" };
            var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

            // Act
            var result = await _controller.CreateTask(dto);
            var afterCreation = DateTime.UtcNow.AddSeconds(1);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var task = Assert.IsType<WorkflowTask>(createdAtActionResult.Value);
            Assert.True(task.CreatedAt >= beforeCreation && task.CreatedAt <= afterCreation);
        }

        [Fact]
        public async Task CompleteTask_SetsCompletedAtToUtcNow()
        {
            // Arrange
            var task = new WorkflowTask { Title = "Task" };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var beforeCompletion = DateTime.UtcNow.AddSeconds(-1);

            // Act
            var result = await _controller.CompleteTask(task.Id);
            var afterCompletion = DateTime.UtcNow.AddSeconds(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var completedTask = Assert.IsType<WorkflowTask>(okResult.Value);
            Assert.NotNull(completedTask.CompletedAt);
            Assert.True(completedTask.CompletedAt >= beforeCompletion && completedTask.CompletedAt <= afterCompletion);
        }

        [Fact]
        public async Task GetImmediateTasks_OrdersByCreatedAtDescending()
        {
            // Arrange
            var task1 = new WorkflowTask { Title = "Task 1", TaskType = "immediate", CreatedAt = DateTime.UtcNow.AddMinutes(-30) };
            var task2 = new WorkflowTask { Title = "Task 2", TaskType = "immediate", CreatedAt = DateTime.UtcNow.AddMinutes(-10) };
            var task3 = new WorkflowTask { Title = "Task 3", TaskType = "immediate", CreatedAt = DateTime.UtcNow.AddMinutes(-20) };

            _context.Tasks.AddRange(task1, task2, task3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetImmediateTasks();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<WorkflowTask>>>(result);
            var tasks = Assert.IsType<List<WorkflowTask>>(okResult.Value);
            Assert.Equal(3, tasks.Count);
            Assert.Equal("Task 2", tasks[0].Title); // Most recent
            Assert.Equal("Task 3", tasks[1].Title);
            Assert.Equal("Task 1", tasks[2].Title); // Oldest
        }

        [Fact]
        public async Task CreateTask_WithDescription_CreatesTaskSuccessfully()
        {
            // Arrange
            var dto = new CreateTaskDto 
            { 
                Title = "Task with Description", 
                Description = "This is a detailed description" 
            };

            // Act
            var result = await _controller.CreateTask(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var task = Assert.IsType<WorkflowTask>(createdAtActionResult.Value);
            Assert.Equal("Task with Description", task.Title);
            Assert.Equal("This is a detailed description", task.Description);
        }

        [Fact]
        public async Task UpdateTask_DoesNotChangeUnspecifiedFields()
        {
            // Arrange
            var originalDate = DateTime.UtcNow.AddDays(-5);
            var task = new WorkflowTask 
            { 
                Title = "Original Title",
                Description = "Original Description",
                Status = "pending",
                CreatedAt = originalDate
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var dto = new UpdateTaskDto { Title = "New Title" };

            // Act
            var result = await _controller.UpdateTask(task.Id, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedTask = Assert.IsType<WorkflowTask>(okResult.Value);
            Assert.Equal("New Title", updatedTask.Title);
            Assert.Equal("Original Description", updatedTask.Description);
            Assert.Equal("pending", updatedTask.Status);
            Assert.Equal(originalDate, updatedTask.CreatedAt);
        }
    }
}
