using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models
{
  public class TaskItem
  {
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    [ScaffoldColumn(false)]
    public string? UserId { get; set; }
  }
}
