using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ArgenCash.Infrastructure.Persistence;

public class ArgenCashDbContextFactory : IDesignTimeDbContextFactory<ArgenCashDbContext>
{
    public ArgenCashDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables(prefix: "ARGENCASH_")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=ArgenCashDB;Username=postgres;Password=secret";

        var optionsBuilder = new DbContextOptionsBuilder<ArgenCashDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ArgenCashDbContext(optionsBuilder.Options);
    }
}
