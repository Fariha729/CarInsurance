using CarInsurance.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Data;

public class InsuranceContext(DbContextOptions<InsuranceContext> options) : DbContext(options)
{
    public DbSet<Insuree> Insurees => Set<Insuree>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Explicit precision documents that quotes represent money.
        modelBuilder.Entity<Insuree>().Property(i => i.Quote).HasPrecision(18, 2);
    }
}
