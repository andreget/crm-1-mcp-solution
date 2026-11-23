using Crm.Data;
using Crm.Models;
using Microsoft.EntityFrameworkCore;

namespace Crm.Services;

public class EfCustomerService : ICustomerService
{
    private readonly CrmDbContext db;

    public EfCustomerService(CrmDbContext db)
    {
        this.db = db;
    }

    public List<Customer> GetAll()
    {
        return db.Customers.AsNoTracking().ToList();
    }

    public Customer? GetById(int id)
    {
        return db.Customers.AsNoTracking().FirstOrDefault(c => c.Id == id);
    }

    // Filtering runs in SQL, not in memory
    public List<Customer> Search(string? text, string? status)
    {
        var query = db.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);

        // SQL Server collation is case-insensitive, so no ToLower needed
        if (!string.IsNullOrWhiteSpace(text))
            query = query.Where(c => c.Name.Contains(text)
                                  || c.Email.Contains(text)
                                  || (c.Company != null && c.Company.Contains(text)));

        return query.OrderBy(c => c.Name).ToList();
    }

    public void Add(Customer customer)
    {
        customer.Id = 0; // the database assigns Id
        customer.Created = DateTime.Now;
        db.Customers.Add(customer);
        db.SaveChanges();
    }

    public bool Update(Customer updated)
    {
        var existing = db.Customers.Find(updated.Id);
        if (existing == null) return false;

        updated.Created = existing.Created; // keep original creation date
        db.Entry(existing).CurrentValues.SetValues(updated);
        db.SaveChanges();
        return true;
    }

    // Cascade delete removes the customer's activities in the database
    public bool Delete(int id)
    {
        return db.Customers.Where(c => c.Id == id).ExecuteDelete() > 0;
    }
}
