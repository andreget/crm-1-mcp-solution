using Crm.Models;

namespace Crm.Services;

// Fake implementation: keeps the customers in a list in memory.
// Data is reset when the app restarts. Register with AddSingleton.
public class InMemoryCustomerService : ICustomerService
{
    // Same sample customers as customers.json
    private readonly List<Customer> customers = new()
    {
        new Customer { Id = 1, Name = "Ingrid Haugen", Email = "ingrid@nordlys.no", Phone = "91234567", Company = "Nordlys AS", Notes = "Signed the annual agreement in January.", Status = "Active", Created = new DateTime(2026, 1, 12, 9, 30, 0) },
        new Customer { Id = 2, Name = "Marius Berg", Email = "marius@fjordtech.no", Phone = "92345678", Company = "FjordTech", Notes = "Asked for a demo. Follow up next week.", Status = "Lead", Created = new DateTime(2026, 2, 3, 14, 15, 0) },
        new Customer { Id = 3, Name = "Sofie Lund", Email = "sofie@bergbygg.no", Phone = "93456789", Company = "Berg Bygg", Notes = "", Status = "Active", Created = new DateTime(2026, 3, 18, 11, 0, 0) },
        new Customer { Id = 4, Name = "Tarjei Moen", Email = "tarjei@havnetorget.no", Phone = "94567890", Company = "Havnetorget", Notes = "Cancelled the subscription.", Status = "Inactive", Created = new DateTime(2025, 11, 27, 16, 45, 0) },
        new Customer { Id = 5, Name = "Camilla Dahl", Email = "camilla@solbakken.no", Phone = "95678901", Company = "Solbakken Regnskap", Notes = "Met at a conference.", Status = "Lead", Created = new DateTime(2026, 4, 9, 8, 20, 0) }
    };

    public List<Customer> GetAll() => customers.ToList();

    public Customer? GetById(int id) => customers.FirstOrDefault(c => c.Id == id);

    public List<Customer> Search(string? text, string? status)
    {
        IEnumerable<Customer> result = customers;

        if (!string.IsNullOrWhiteSpace(status))
            result = result.Where(c => c.Status == status);

        if (!string.IsNullOrWhiteSpace(text))
        {
            string term = text.ToLower();
            result = result.Where(c => c.Name.ToLower().Contains(term)
                                    || c.Email.ToLower().Contains(term)
                                    || (c.Company ?? "").ToLower().Contains(term));
        }

        return result.OrderBy(c => c.Name).ToList();
    }

    public void Add(Customer customer)
    {
        customer.Id = customers.Count == 0 ? 1 : customers.Max(c => c.Id) + 1; // next free Id
        customer.Created = DateTime.Now;
        customers.Add(customer);
    }

    public bool Update(Customer updated)
    {
        int index = customers.FindIndex(c => c.Id == updated.Id);
        if (index == -1) return false;

        updated.Created = customers[index].Created; // keep original creation date
        customers[index] = updated;
        return true;
    }

    public bool Delete(int id) => customers.RemoveAll(c => c.Id == id) > 0;
}
