using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly WorkflowDbContext _context;

    public TasksController(WorkflowDbContext context)
    {
        _context = context;
    }

    // GET: api/tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkflowTask>>> GetTasks()
    {
        return await _context.Tasks
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    // GET: api/tasks/5
    [HttpGet("{id}")]
    public async Task<ActionResult<WorkflowTask>> GetTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
        {
            return NotFound();
        }

        return task;
    }

    // GET: api/tasks/immediate
    [HttpGet("immediate")]
    public async Task<ActionResult<IEnumerable<WorkflowTask>>> GetImmediateTasks()
    {
        return await _context.Tasks
            .Where(t => t.TaskType == "immediate")
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    // GET: api/tasks/scheduled
    [HttpGet("scheduled")]
    public async Task<ActionResult<IEnumerable<WorkflowTask>>> GetScheduledTasks()
    {
        return await _context.Tasks
            .Where(t => t.TaskType == "scheduled")
            .OrderBy(t => t.ScheduledFor)
            .ToListAsync();
    }

    // POST: api/tasks
    [HttpPost]
    public async Task<ActionResult<WorkflowTask>> CreateTask(CreateTaskDto dto)
    {
        var task = new WorkflowTask
        {
            Title = dto.Title,
            Description = dto.Description,
            TaskType = dto.ScheduledFor.HasValue ? "scheduled" : "immediate",
            ScheduledFor = dto.ScheduledFor,
            CreatedAt = DateTime.UtcNow,
            Status = "pending"
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    // PUT: api/tasks/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto dto)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
        {
            return NotFound();
        }

        if (dto.Title != null)
            task.Title = dto.Title;
        
        if (dto.Description != null)
            task.Description = dto.Description;
        
        if (dto.Status != null)
        {
            task.Status = dto.Status;
            if (dto.Status == "completed")
            {
                task.IsCompleted = true;
                task.CompletedAt = DateTime.UtcNow;
            }
        }

        if (dto.ScheduledFor.HasValue)
        {
            task.ScheduledFor = dto.ScheduledFor;
            task.TaskType = "scheduled";
        }

        await _context.SaveChangesAsync();

        return Ok(task);
    }

    // PUT: api/tasks/5/complete
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> CompleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
        {
            return NotFound();
        }

        task.IsCompleted = true;
        task.Status = "completed";
        task.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(task);
    }

    // DELETE: api/tasks/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
        {
            return NotFound();
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// DTOs
public class CreateTaskDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTime? ScheduledFor { get; set; }
}

public class UpdateTaskDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public DateTime? ScheduledFor { get; set; }
}
