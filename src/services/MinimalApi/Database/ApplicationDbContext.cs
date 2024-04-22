using Microsoft.EntityFrameworkCore;
using MinimalApi.Entities;
namespace MinimalApi.Database;


public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<Customer>(builder =>
        //    builder.OwnsOne(a => a.Tags, tagsBuilder => tagsBuilder.ToJson()));
    }

    public DbSet<Customer> Customers { get; set; }
}
