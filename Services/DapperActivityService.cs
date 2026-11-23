using Crm.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Crm.Services;

public class DapperActivityService : IActivityService
{
    private const string Columns = "Id, CustomerId, Type, Title, [Date], Notes, Done";

    private readonly string connectionString;

    public DapperActivityService(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("Crm")!;
    }

    private SqlConnection Connect() => new SqlConnection(connectionString);

    public List<Activity> GetAll()
    {
        using var db = Connect();
        return db.Query<Activity>($"SELECT {Columns} FROM Activities").ToList();
    }

    // Newest first
    public List<Activity> GetByCustomer(int customerId)
    {
        using var db = Connect();
        return db.Query<Activity>(
            $"SELECT {Columns} FROM Activities WHERE CustomerId = @customerId ORDER BY [Date] DESC",
            new { customerId }).ToList();
    }

    // Not done, soonest first
    public List<Activity> GetOpen()
    {
        using var db = Connect();
        return db.Query<Activity>(
            $"SELECT {Columns} FROM Activities WHERE Done = 0 ORDER BY [Date]").ToList();
    }

    public void Add(Activity activity)
    {
        const string sql = @"
            INSERT INTO Activities (CustomerId, Type, Title, [Date], Notes, Done)
            OUTPUT INSERTED.Id
            VALUES (@CustomerId, @Type, @Title, @Date, @Notes, @Done)";

        using var db = Connect();
        activity.Id = db.ExecuteScalar<int>(sql, activity); // the database assigns Id
    }

    public bool SetDone(int id, bool done)
    {
        using var db = Connect();
        return db.Execute("UPDATE Activities SET Done = @done WHERE Id = @id", new { id, done }) > 0;
    }

    public bool Delete(int id)
    {
        using var db = Connect();
        return db.Execute("DELETE FROM Activities WHERE Id = @id", new { id }) > 0;
    }

    // Not strictly needed: cascade delete already does this
    public void DeleteByCustomer(int customerId)
    {
        using var db = Connect();
        db.Execute("DELETE FROM Activities WHERE CustomerId = @customerId", new { customerId });
    }
}
