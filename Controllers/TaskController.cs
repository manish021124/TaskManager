using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Data;
using TaskManager.Models;
using Microsoft.AspNetCore.Authorization;

namespace TaskManager.Controllers
{
  [Authorize]
  public class TaskController : Controller
  {
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public TaskController(AppDbContext context, UserManager<IdentityUser> userManager)
    {
      _context = context;
      _userManager = userManager;
    }

    public IActionResult Index()
    {
      var userId = _userManager.GetUserId(User);
      var tasks = _context.Tasks.Where(t => t.UserId == userId).ToList();
      return View(tasks);
    }

    public IActionResult Create()
    {
      var task = new TaskItem();
      return View(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create([Bind("Title,Description,IsCompleted")] TaskItem task)
    {
      Console.WriteLine($"IsCompleted in POST: {task.IsCompleted}");
      if (task == null || !ModelState.IsValid)
      {
        Console.WriteLine("Model state is invalid.");
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
          Console.WriteLine($"Error: {error.ErrorMessage}");
        }
        return View(task);
      }

      var userId = _userManager.GetUserId(User);
      if (string.IsNullOrEmpty(userId))
      {
        return Unauthorized();
      }

      task.UserId = userId;

      try
      {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        Console.WriteLine("Task successfully added.");
        return RedirectToAction(nameof(Index));
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred while saving the task: {ex.Message}");
        return View(task);
      }
    }
    
    public async Task<IActionResult> Edit(int id)
    {
      var task = await _context.Tasks.FindAsync(id);
      if (task == null) return NotFound();

      var userId = _userManager.GetUserId(User);
      if (task.UserId != userId) return Unauthorized();

      return View(task);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TaskItem task)
    {
      if (ModelState.IsValid)
      {
        var existingTask = await _context.Tasks.FindAsync(task.Id);
        if (existingTask == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (existingTask.UserId != userId) return Unauthorized();

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.IsCompleted = task.IsCompleted;

        _context.Tasks.Update(existingTask);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      return View(task);
    }

    public async Task<IActionResult> Delete(int id)
    {
      var task = await _context.Tasks.FindAsync(id);
      if (task == null) return NotFound();

      var userId = _userManager.GetUserId(User);
      if (task.UserId != userId) return Unauthorized();

      return View(task);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var task = await _context.Tasks.FindAsync(id);
      if (task != null)
      {
        var userId = _userManager.GetUserId(User);
        if (task.UserId != userId) return Unauthorized();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
      }
      return RedirectToAction(nameof(Index));
    }
  }
}
