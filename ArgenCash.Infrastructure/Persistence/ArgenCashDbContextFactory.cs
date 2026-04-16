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

        var currentDirectory = Directory.GetCurrentDirectory();
        var backendDirectory = ResolveBackendDirectory(currentDirectory);
        var apiDirectory = Path.Combine(backendDirectory, "ArgenCash.Api");

        LoadDotEnvFile(Path.Combine(backendDirectory, ".env"));
        LoadDotEnvFile(Path.Combine(apiDirectory, ".env"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(currentDirectory)
            .AddJsonFile(Path.Combine(apiDirectory, "appsettings.json"), optional: true)
            .AddJsonFile(Path.Combine(apiDirectory, $"appsettings.{environment}.json"), optional: true)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables(prefix: "ARGENCASH_")
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration["ARGENCASH_EF_DB_CONNECTION"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("ARGENCASH_EF_DB_CONNECTION");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = configuration["ConnectionStrings:DefaultConnection"];
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = configuration["ARGENCASH_DB_CONNECTION"];
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'ConnectionStrings:DefaultConnection' is missing for EF design-time operations. " +
                "Set ARGENCASH_EF_DB_CONNECTION for local EF commands, or configure ArgenCash.Api/appsettings.Development.json, " +
                "ArgenCash.Api/.env (ConnectionStrings__DefaultConnection), or Backend/.env (ARGENCASH_DB_CONNECTION)."
            );
        }

        var optionsBuilder = new DbContextOptionsBuilder<ArgenCashDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ArgenCashDbContext(optionsBuilder.Options);
    }

    private static string ResolveBackendDirectory(string currentDirectory)
    {
        var directory = new DirectoryInfo(currentDirectory);

        while (directory is not null)
        {
            if (string.Equals(directory.Name, "Backend", StringComparison.OrdinalIgnoreCase))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return currentDirectory;
    }

    private static void LoadDotEnvFile(string dotenvPath)
    {
        if (!File.Exists(dotenvPath))
        {
            return;
        }

        foreach (var rawLine in File.ReadAllLines(dotenvPath))
        {
            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 &&
                ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value[1..^1];
            }

            if (string.IsNullOrWhiteSpace(key) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                continue;
            }

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}
