namespace Crm.Models;

public class Customer
{
    public static readonly string[] Statuses = { "Lead", "Active", "Inactive" };

    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Lead";
    public DateTime Created { get; set; } = DateTime.Now;

    // The rules in one place, used by the HTML forms, the API and the MCP tools.
    // Returns an empty string when the customer is valid.
    public string Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) return "Name is required.";
        if (string.IsNullOrEmpty(Email) || !Email.Contains('@')) return "A valid email is required.";
        if (!Statuses.Contains(Status)) return "Status must be Lead, Active or Inactive.";
        return "";
    }
}