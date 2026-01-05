using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using System;
using System.Linq;
using Xunit;

namespace server.Tests.Data
{
    public class WorkflowDbContextTests : IDisposable
    {
        private readonly WorkflowDbContext _context;

        public WorkflowDbContextTests()
        {
            var options = new DbContextOptionsBuilder<WorkflowDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new WorkflowDbContext(options);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void CanCreateWorkflowDbContext()
        {
            // Act & Assert
            Assert.NotNull(_context);
            Assert.NotNull(_context.Tasks);
        }

        [Fact]
        public async Task CanAddAndRetrieveTask()
        {
            // Arrange
            var task = new WorkflowTask
            {
                Title = "Test Task",
                Description = "Test Description",
                TaskType = "immediate",
                Status = "pending"
            };

            // Act
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Assert
            var retrievedTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Title == "Test Task");
            Assert.NotNull(retrievedTask);
            Assert.Equal("Test Task", retrievedTask.Title);
            Assert.Equal("Test Description", retrievedTask.Description);
            Assert.Equal("immediate", retrievedTask.TaskType);
            Assert.Equal("pending", retrievedTask.Status);
        }

        [Fact]
        public async Task CanUpdateTask()
        {
            // Arrange
            var task = new WorkflowTask
            {
                Title = "Original Title",
                Status = "pending"
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Act
            task.Title = "Updated Title";
            task.Status = "completed";
            task.IsCompleted = true;
            task.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Assert
            var updatedTask = await _context.Tasks.FindAsync(task.Id);
            Assert.Equal("Updated Title", updatedTask.Title);
            Assert.Equal("completed", updatedTask.Status);
            Assert.True(updatedTask.IsCompleted);
            Assert.NotNull(updatedTask.CompletedAt);
        }

        [Fact]
        public async Task CanDeleteTask()
        {
            // Arrange
            var task = new WorkflowTask { Title = "Task to Delete" };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Act
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            // Assert
            var deletedTask = await _context.Tasks.FindAsync(task.Id);
            Assert.Null(deletedTask);
        }

        [Fact]
        public async Task CanQueryTasksByType()
        {
            // Arrange
            var immediateTask = new WorkflowTask { Title = "Immediate Task", TaskType = "immediate" };
            var scheduledTask = new WorkflowTask { Title = "Scheduled Task", TaskType = "scheduled" };
            var anotherImmediateTask = new WorkflowTask { Title = "Another Immediate", TaskType = "immediate" };

            _context.Tasks.AddRange(immediateTask, scheduledTask, anotherImmediateTask);
            await _context.SaveChangesAsync();

            // Act
            var immediateTasks = await _context.Tasks.Where(t => t.TaskType == "immediate").ToListAsync();
            var scheduledTasks = await _context.Tasks.Where(t => t.TaskType == "scheduled").ToListAsync();

            // Assert
            Assert.Equal(2, immediateTasks.Count);
            Assert.Single(scheduledTasks);
            Assert.All(immediateTasks, t => Assert.Equal("immediate", t.TaskType));
            Assert.All(scheduledTasks, t => Assert.Equal("scheduled", t.TaskType));
        }

        [Fact]
        public async Task CanQueryTasksByStatus()
        {
            // Arrange
            var pendingTask = new WorkflowTask { Title = "Pending Task", Status = "pending" };
            var completedTask = new WorkflowTask { Title = "Completed Task", Status = "completed", IsCompleted = true };
            var inProgressTask = new WorkflowTask { Title = "In Progress Task", Status = "in_progress" };

            _context.Tasks.AddRange(pendingTask, completedTask, inProgressTask);
            await _context.SaveChangesAsync();

            // Act
            var pendingTasks = await _context.Tasks.Where(t => t.Status == "pending").ToListAsync();
            var completedTasks = await _context.Tasks.Where(t => t.Status == "completed").ToListAsync();

            // Assert
            Assert.Single(pendingTasks);
            Assert.Single(completedTasks);
            Assert.All(pendingTasks, t => Assert.Equal("pending", t.Status));
            Assert.All(completedTasks, t => Assert.Equal("completed", t.Status));
        }

        [Fact]
        public async Task CanOrderTasksByCreatedAt()
        {
            // Arrange
            var task1 = new WorkflowTask { Title = "Task 1", CreatedAt = DateTime.UtcNow.AddMinutes(-10) };
            var task2 = new WorkflowTask { Title = "Task 2", CreatedAt = DateTime.UtcNow.AddMinutes(-5) };
            var task3 = new WorkflowTask { Title = "Task 3", CreatedAt = DateTime.UtcNow };

            _context.Tasks.AddRange(task1, task2, task3);
            await _context.SaveChangesAsync();

            // Act
            var tasksOrderedDesc = await _context.Tasks.OrderByDescending(t => t.CreatedAt).ToListAsync();
            var tasksOrderedAsc = await _context.Tasks.OrderBy(t => t.CreatedAt).ToListAsync();

            // Assert
            Assert.Equal("Task 3", tasksOrderedDesc[0].Title);
            Assert.Equal("Task 2", tasksOrderedDesc[1].Title);
            Assert.Equal("Task 1", tasksOrderedDesc[2].Title);

            Assert.Equal("Task 1", tasksOrderedAsc[0].Title);
            Assert.Equal("Task 2", tasksOrderedAsc[1].Title);
            Assert.Equal("Task 3", tasksOrderedAsc[2].Title);
        }

        [Fact]
        public async Task CanHandleScheduledTasks()
        {
            // Arrange
            var scheduledTime = DateTime.UtcNow.AddDays(1);
            var scheduledTask = new WorkflowTask
            {
                Title = "Scheduled Task",
                TaskType = "scheduled",
                ScheduledFor = scheduledTime
            };

            // Act
            _context.Tasks.Add(scheduledTask);
            await _context.SaveChangesAsync();

            // Assert
            var retrievedTask = await _context.Tasks.FindAsync(scheduledTask.Id);
            Assert.NotNull(retrievedTask);
            Assert.Equal("scheduled", retrievedTask.TaskType);
            Assert.Equal(scheduledTime, retrievedTask.ScheduledFor);
        }

        [Fact]
        public async Task DefaultValuesAreSetCorrectly()
        {
            // Arrange
            var task = new WorkflowTask { Title = "Test Task" };

            // Act
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Assert
            var retrievedTask = await _context.Tasks.FindAsync(task.Id);
            Assert.Equal("immediate", retrievedTask.TaskType);
            Assert.Equal("pending", retrievedTask.Status);
            Assert.False(retrievedTask.IsCompleted);
            Assert.Null(retrievedTask.CompletedAt);
            Assert.True(retrievedTask.CreatedAt != default);
        }
    }
}