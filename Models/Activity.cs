namespace Crm.Models;

public class Activity
{
    public static readonly string[] Types = { "Meeting", "Call", "Email", "Task" };

    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Type { get; set; } = "Meeting";
    public string Title { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    public string? Notes { get; set; }
    public bool Done { get; set; }

    // Returns an empty string when the activity is valid.
    // That the customer exists is checked where the services are available.
    public string Validate()
    {
        if (string.IsNullOrWhiteSpace(Title)) return "Title is required.";
        if (!Types.Contains(Type)) return "Type must be Meeting, Call, Email or Task.";
        return "";
    }
}