using Crm.Models;

namespace Crm.Services;

// Fake implementation: keeps the activities in a list in memory.
// Starts empty, like activities.json. Register with AddSingleton.
public class InMemoryActivityService : IActivityService
{
    private readonly List<Activity> activities = new();

    public List<Activity> GetAll() => activities.ToList();

    // Newest first
    public List<Activity> GetByCustomer(int customerId)
    {
        return activities
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.Date)
            .ToList();
    }

    // Not done, soonest first
    public List<Activity> GetOpen()
    {
        return activities.Where(a => !a.Done).OrderBy(a => a.Date).ToList();
    }

    public void Add(Activity activity)
    {
        activity.Id = activities.Count == 0 ? 1 : activities.Max(a => a.Id) + 1;
        activities.Add(activity);
    }

    public bool SetDone(int id, bool done)
    {
        var activity = activities.FirstOrDefault(a => a.Id == id);
        if (activity == null) return false;

        activity.Done = done;
        return true;
    }

    public bool Delete(int id) => activities.RemoveAll(a => a.Id == id) > 0;

    // Used when a customer is deleted
    public void DeleteByCustomer(int customerId)
    {
        activities.RemoveAll(a => a.CustomerId == customerId);
    }
}
