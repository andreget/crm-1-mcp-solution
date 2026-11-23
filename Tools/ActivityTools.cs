using System.ComponentModel;
using Crm.Models;
using Crm.Services;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Crm.Tools;

[McpServerToolType]
public static class ActivityTools
{
    [McpServerTool, Description("Get all activities for one customer, newest first.")]
    public static List<Activity> GetActivitiesForCustomer(
        IActivityService activityService,
        [Description("Customer id")] int customerId)
    {
        return activityService.GetByCustomer(customerId);
    }

    [McpServerTool, Description("Get all activities that are not done, soonest first.")]
    public static List<Activity> GetOpenActivities(IActivityService activityService)
    {
        return activityService.GetOpen();
    }

    [McpServerTool, Description("Create an activity (Meeting, Call, Email or Task) for a customer. Returns the activity with its new id.")]
    public static Activity AddActivity(
        ICustomerService customerService,
        IActivityService activityService,
        [Description("Id of an existing customer")] int customerId,
        [Description("Meeting, Call, Email or Task")] string type,
        [Description("Short title")] string title,
        [Description("When the activity happens or is due, ISO 8601, e.g. 2026-09-03T10:00:00")] DateTime date,
        [Description("Free text notes")] string? notes = null,
        [Description("True if the activity is already done")] bool done = false)
    {
        var activity = new Activity
        {
            CustomerId = customerId,
            Type = type,
            Title = title,
            Date = date,
            Notes = notes,
            Done = done
        };

        string error = Validate(activity, customerService);
        if (error != "")
            throw new McpException(error);

        activityService.Add(activity);
        return activity;
    }

    [McpServerTool, Description("Create many activities in one call. Use this for test data. Every CustomerId must be an existing customer. Id is set by the server. Nothing is saved if any activity is invalid.")]
    public static List<Activity> AddActivities(
        ICustomerService customerService,
        IActivityService activityService,
        [Description("The activities to create. Date is ISO 8601, e.g. 2026-09-03T10:00:00")] List<Activity> activities)
    {
        // Check all first, so either all or none are saved
        var errors = activities
            .Select((a, index) => (a, index, error: Validate(a, customerService)))
            .Where(x => x.error != "")
            .Select(x => $"#{x.index} ({x.a.Title}): {x.error}")
            .ToList();

        if (errors.Count > 0)
            throw new McpException("Invalid activities, nothing saved:\n" + string.Join("\n", errors));

        foreach (var activity in activities)
            activityService.Add(activity);

        return activities;
    }

    [McpServerTool, Description("Mark an activity as done or open again. Returns false if the activity does not exist.")]
    public static bool SetActivityDone(
        IActivityService activityService,
        [Description("Activity id")] int id,
        [Description("True for done, false for open")] bool done)
    {
        return activityService.SetDone(id, done);
    }

    [McpServerTool, Description("Delete an activity. Cannot be undone. Returns false if the activity does not exist.")]
    public static bool DeleteActivity(
        IActivityService activityService,
        [Description("Activity id")] int id)
    {
        return activityService.Delete(id);
    }

    // Model rules plus a check that the customer exists
    private static string Validate(Activity activity, ICustomerService customerService)
    {
        string error = activity.Validate();
        if (error != "") return error;

        if (customerService.GetById(activity.CustomerId) == null)
            return $"Customer {activity.CustomerId} does not exist.";

        return "";
    }
}