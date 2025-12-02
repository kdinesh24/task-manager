using System.ComponentModel.DataAnnotations;

namespace server.Models;

public class WorkflowTask
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ScheduledFor { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedAt { get; set; }

    // "immediate" or "scheduled"
    [Required]
    public string TaskType { get; set; } = "immediate";

    // "pending", "in_progress", "completed"
    [Required]
    public string Status { get; set; } = "pending";
}
