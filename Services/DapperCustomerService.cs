using Crm.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Crm.Services;

// Plain SQL with Dapper: we write the queries, Dapper maps rows to objects
public class DapperCustomerService : ICustomerService
{
    private const string Columns = "Id, Name, Email, Phone, Company, Notes, Status, Created";

    private readonly string connectionString;

    public DapperCustomerService(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("Crm")!;
    }

    // A new connection per call; the connection pool makes this cheap
    private SqlConnection Connect() => new SqlConnection(connectionString);

    public List<Customer> GetAll()
    {
        using var db = Connect();
        return db.Query<Customer>($"SELECT {Columns} FROM Customers").ToList();
    }

    public Customer? GetById(int id)
    {
        using var db = Connect();
        return db.QuerySingleOrDefault<Customer>(
            $"SELECT {Columns} FROM Customers WHERE Id = @id", new { id });
    }

    // Build the WHERE clause step by step, always with parameters
    public List<Customer> Search(string? text, string? status)
    {
        string sql = $"SELECT {Columns} FROM Customers WHERE 1 = 1";

        if (!string.IsNullOrWhiteSpace(status))
            sql += " AND Status = @status";

        if (!string.IsNullOrWhiteSpace(text))
            sql += " AND (Name LIKE @term OR Email LIKE @term OR Company LIKE @term)";

        sql += " ORDER BY Name";

        using var db = Connect();
        return db.Query<Customer>(sql, new { status, term = $"%{text}%" }).ToList();
    }

    public void Add(Customer customer)
    {
        customer.Created = DateTime.Now;

        const string sql = @"
            INSERT INTO Customers (Name, Email, Phone, Company, Notes, Status, Created)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Email, @Phone, @Company, @Notes, @Status, @Created)";

        using var db = Connect();
        customer.Id = db.ExecuteScalar<int>(sql, customer); // the database assigns Id
    }

    // Created is not in the SET list, so the original creation date is kept
    public bool Update(Customer updated)
    {
        const string sql = @"
            UPDATE Customers
            SET Name = @Name, Email = @Email, Phone = @Phone, Company = @Company,
                Notes = @Notes, Status = @Status
            WHERE Id = @Id";

        using var db = Connect();
        int rows = db.Execute(sql, updated);
        if (rows == 0) return false;

        updated.Created = db.ExecuteScalar<DateTime>(
            "SELECT Created FROM Customers WHERE Id = @Id", updated);
        return true;
    }

    // Cascade delete removes the customer's activities in the database
    public bool Delete(int id)
    {
        using var db = Connect();
        return db.Execute("DELETE FROM Customers WHERE Id = @id", new { id }) > 0;
    }
}
