using BookHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Used by `dotnet ef` tooling at design time to generate migrations.
/// Always targets PostgreSQL so migrations are compatible with the production database.
/// Set the BOOKHUB_POSTGRES_URL environment variable to your PostgreSQL connection string before running migrations.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("BOOKHUB_POSTGRES_URL")
            ?? throw new InvalidOperationException(
                "Set BOOKHUB_POSTGRES_URL to your PostgreSQL connection string before running 'dotnet ef migrations add'.");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
