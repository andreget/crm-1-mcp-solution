using Crm.Models;
using Microsoft.EntityFrameworkCore;

namespace Crm.Data;

public class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Activity> Activities => Set<Activity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100);
            e.Property(c => c.Email).HasMaxLength(200);
            e.Property(c => c.Phone).HasMaxLength(20);
            e.Property(c => c.Company).HasMaxLength(100);
            e.Property(c => c.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<Activity>(e =>
        {
            e.Property(a => a.Type).HasMaxLength(20);
            e.Property(a => a.Title).HasMaxLength(200);

            // One customer has many activities; deleting a customer deletes its activities
            e.HasOne<Customer>()
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
