using server.Models;
using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace server.Tests.Models
{
    public class WorkflowTaskTests
    {
        [Fact]
        public void WorkflowTask_CanBeCreated()
        {
            // Act
            var task = new WorkflowTask();

            // Assert
            Assert.NotNull(task);
            Assert.Equal(0, task.Id);
            Assert.Equal("immediate", task.TaskType);
            Assert.Equal("pending", task.Status);
            Assert.False(task.IsCompleted);
            Assert.Null(task.CompletedAt);
            Assert.Null(task.ScheduledFor);
            Assert.True(task.CreatedAt != default);
        }

        [Fact]
        public void WorkflowTask_CanSetProperties()
        {
            // Arrange
            var scheduledTime = DateTime.UtcNow.AddDays(1);
            var completedTime = DateTime.UtcNow;

            // Act
            var task = new WorkflowTask
            {
                Id = 1,
                Title = "Test Task",
                Description = "Test Description",
                TaskType = "scheduled",
                Status = "completed",
                IsCompleted = true,
                CompletedAt = completedTime,
                ScheduledFor = scheduledTime,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            // Assert
            Assert.Equal(1, task.Id);
            Assert.Equal("Test Task", task.Title);
            Assert.Equal("Test Description", task.Description);
            Assert.Equal("scheduled", task.TaskType);
            Assert.Equal("completed", task.Status);
            Assert.True(task.IsCompleted);
            Assert.Equal(completedTime, task.CompletedAt);
            Assert.Equal(scheduledTime, task.ScheduledFor);
        }

        [Theory]
        [InlineData("immediate")]
        [InlineData("scheduled")]
        public void WorkflowTask_AcceptsValidTaskTypes(string taskType)
        {
            // Arrange & Act
            var task = new WorkflowTask { TaskType = taskType };

            // Assert
            Assert.Equal(taskType, task.TaskType);
        }

        [Theory]
        [InlineData("pending")]
        [InlineData("in_progress")]
        [InlineData("completed")]
        public void WorkflowTask_AcceptsValidStatuses(string status)
        {
            // Arrange & Act
            var task = new WorkflowTask { Status = status };

            // Assert
            Assert.Equal(status, task.Status);
        }

        [Fact]
        public void WorkflowTask_TitleIsRequired()
        {
            // Arrange
            var task = new WorkflowTask();

            // Act
            var validationContext = new ValidationContext(task);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(task, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Title"));
        }

        [Fact]
        public void WorkflowTask_TitleMaxLengthIs200()
        {
            // Arrange
            var task = new WorkflowTask
            {
                Title = new string('A', 201) // Exceeds max length
            };

            // Act
            var validationContext = new ValidationContext(task);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(task, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Title"));
        }

        [Fact]
        public void WorkflowTask_TitleWithinMaxLengthIsValid()
        {
            // Arrange
            var task = new WorkflowTask
            {
                Title = new string('A', 200) // Exactly max length
            };

            // Act
            var validationContext = new ValidationContext(task);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(task, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void WorkflowTask_DescriptionMaxLengthIs1000()
        {
            // Arrange
            var task = new WorkflowTask
            {
                Title = "Valid Title",
                Description = new string('A', 1001) // Exceeds max length
            };

            // Act
            var validationContext = new ValidationContext(task);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(task, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Description"));
        }

        [Fact]
        public void WorkflowTask_DescriptionWithinMaxLengthIsValid()
        {
            // Arrange
            var task = new WorkflowTask
            {
                Title = "Valid Title",
                Description = new string('A', 1000) // Exactly max length
            };

            // Act
            var validationContext = new ValidationContext(task);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(task, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void WorkflowTask_DescriptionCanBeNull()
        {
            // Arrange
            var task = new WorkflowTask
            {
                Title = "Valid Title",
                Description = null!
            };

            // Act
            var validationContext = new ValidationContext(task);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(task, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Null(task.Description);
        }

        [Fact]
        public void WorkflowTask_TaskTypeIsRequired()
        {
            // Arrange
            var task = new WorkflowTask
            {
                Title = "Valid Title",
                TaskType = null!
            };

            // Act
            var validationContext = new ValidationContext(task);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(task, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("TaskType"));
        }

        [Fact]
        public void WorkflowTask_StatusIsRequired()
        {
            // Arrange
            var task = new WorkflowTask
            {
                Title = "Valid Title",
                Status = null!
            };

            // Act
            var validationContext = new ValidationContext(task);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(task, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, vr => vr.MemberNames.Contains("Status"));
        }

        [Fact]
        public void WorkflowTask_ValidTaskPassesValidation()
        {
            // Arrange
            var task = new WorkflowTask
            {
                Title = "Valid Task Title",
                Description = "Valid task description",
                TaskType = "immediate",
                Status = "pending"
            };

            // Act
            var validationContext = new ValidationContext(task);
            var validationResults = new System.Collections.Generic.List<ValidationResult>();
            var isValid = Validator.TryValidateObject(task, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void WorkflowTask_CanBeCompleted()
        {
            // Arrange
            var task = new WorkflowTask
            {
                Title = "Task to Complete",
                Status = "pending",
                IsCompleted = false
            };

            // Act
            task.Status = "completed";
            task.IsCompleted = true;
            task.CompletedAt = DateTime.UtcNow;

            // Assert
            Assert.Equal("completed", task.Status);
            Assert.True(task.IsCompleted);
            Assert.NotNull(task.CompletedAt);
        }

        [Fact]
        public void WorkflowTask_CanBeScheduled()
        {
            // Arrange
            var scheduledTime = DateTime.UtcNow.AddDays(1);

            // Act
            var task = new WorkflowTask
            {
                Title = "Scheduled Task",
                TaskType = "scheduled",
                ScheduledFor = scheduledTime
            };

            // Assert
            Assert.Equal("scheduled", task.TaskType);
            Assert.Equal(scheduledTime, task.ScheduledFor);
        }
    }
}