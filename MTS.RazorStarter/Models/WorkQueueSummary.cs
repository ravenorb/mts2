namespace MTS.RazorStarter.Models;

public class WorkQueueSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public int QueueCount { get; set; }
}
