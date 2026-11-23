using System.ComponentModel;
using Crm.Services;
using ModelContextProtocol.Server;

namespace Crm.Tools;

// Key figures calculated on the server.
// The AI gets a small, correct answer instead of reading every row itself.
[McpServerToolType]
public static class KpiTools
{
    [McpServerTool, Description("Number of customers per status (Lead, Active, Inactive) and the share of customers that are Active.")]
    public static object GetCustomerStatusSummary(ICustomerService customerService)
    {
        var customers = customerService.GetAll();
        int total = customers.Count;

        return new
        {
            Total = total,
            ByStatus = customers
                .GroupBy(c => c.Status)
                .ToDictionary(g => g.Key, g => g.Count()),
            ActiveShare = total == 0 ? 0 : Math.Round((double)customers.Count(c => c.Status == "Active") / total, 2)
        };
    }

    [McpServerTool, Description("Activities that are not done and have a date in the past, with customer name, oldest first.")]
    public static object GetOverdueActivities(
        ICustomerService customerService,
        IActivityService activityService)
    {
        var customers = customerService.GetAll().ToDictionary(c => c.Id, c => c.Name);

        return activityService.GetOpen()
            .Where(a => a.Date < DateTime.Now)
            .Select(a => new
            {
                a.Id,
                a.CustomerId,
                Customer = customers.GetValueOrDefault(a.CustomerId, "(unknown)"),
                a.Type,
                a.Title,
                a.Date,
                DaysOverdue = (DateTime.Now - a.Date).Days
            })
            .ToList();
    }

    [McpServerTool, Description("Customers with no activity in the last N days, optionally only one status. Useful for finding leads that need follow-up.")]
    public static object GetCustomersWithoutRecentActivity(
        ICustomerService customerService,
        IActivityService activityService,
        [Description("Number of days to look back")] int days = 30,
        [Description("Only this status: Lead, Active or Inactive")] string? status = null)
    {
        var since = DateTime.Now.AddDays(-days);

        var lastActivity = activityService.GetAll()
            .Where(a => a.Date <= DateTime.Now)
            .GroupBy(a => a.CustomerId)
            .ToDictionary(g => g.Key, g => g.Max(a => a.Date));

        return customerService.Search(null, status)
            .Where(c => !lastActivity.TryGetValue(c.Id, out var last) || last < since)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Company,
                c.Status,
                LastActivity = lastActivity.TryGetValue(c.Id, out var last) ? last : (DateTime?)null
            })
            .ToList();
    }

    [McpServerTool, Description("Activity statistics for a period: count per type, how many are done, and average number of activities per customer.")]
    public static object GetActivityStatistics(
        IActivityService activityService,
        [Description("Start of period. Omit for no lower limit")] DateTime? from = null,
        [Description("End of period. Omit for no upper limit")] DateTime? to = null)
    {
        var activities = activityService.GetAll()
            .Where(a => (from == null || a.Date >= from) && (to == null || a.Date <= to))
            .ToList();

        int customersWithActivities = activities.Select(a => a.CustomerId).Distinct().Count();

        return new
        {
            Total = activities.Count,
            Done = activities.Count(a => a.Done),
            ByType = activities
                .GroupBy(a => a.Type)
                .ToDictionary(g => g.Key, g => g.Count()),
            AveragePerCustomer = customersWithActivities == 0
                ? 0
                : Math.Round((double)activities.Count / customersWithActivities, 1)
        };
    }
}
