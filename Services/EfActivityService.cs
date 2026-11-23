using Crm.Data;
using Crm.Models;
using Microsoft.EntityFrameworkCore;

namespace Crm.Services;

public class EfActivityService : IActivityService
{
    private readonly CrmDbContext db;

    public EfActivityService(CrmDbContext db)
    {
        this.db = db;
    }

    public List<Activity> GetAll()
    {
        return db.Activities.AsNoTracking().ToList();
    }

    // Newest first
    public List<Activity> GetByCustomer(int customerId)
    {
        return db.Activities.AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.Date)
            .ToList();
    }

    // Not done, soonest first
    public List<Activity> GetOpen()
    {
        return db.Activities.AsNoTracking()
            .Where(a => !a.Done)
            .OrderBy(a => a.Date)
            .ToList();
    }

    public void Add(Activity activity)
    {
        activity.Id = 0; // the database assigns Id
        db.Activities.Add(activity);
        db.SaveChanges();
    }

    // One UPDATE statement, no need to load the row first
    public bool SetDone(int id, bool done)
    {
        return db.Activities
            .Where(a => a.Id == id)
            .ExecuteUpdate(s => s.SetProperty(a => a.Done, done)) > 0;
    }

    public bool Delete(int id)
    {
        return db.Activities.Where(a => a.Id == id).ExecuteDelete() > 0;
    }

    // Not strictly needed: cascade delete already does this
    public void DeleteByCustomer(int customerId)
    {
        db.Activities.Where(a => a.CustomerId == customerId).ExecuteDelete();
    }
}
