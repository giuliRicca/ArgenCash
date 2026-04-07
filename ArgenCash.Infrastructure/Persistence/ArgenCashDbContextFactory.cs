using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArgenCash.Infrastructure.Persistence;

public class ArgenCashDbContextFactory : IDesignTimeDbContextFactory<ArgenCashDbContext>
{
    public ArgenCashDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ArgenCashDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ARGENCASH_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=ArgenCashDB;Username=postgres;Password=admin";

        optionsBuilder.UseNpgsql(connectionString);

        return new ArgenCashDbContext(optionsBuilder.Options);
    }
}
