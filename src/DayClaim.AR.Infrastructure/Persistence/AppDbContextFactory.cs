using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DayClaim.AR.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations add/update-database` construct the DbContext
/// without spinning up the full web host (and its RabbitMQ/Redis/JWT
/// dependencies). Only used at design time; the running app gets its
/// DbContextOptions from DependencyInjection.AddInfrastructure instead.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("DAYCLAIM_AR_MIGRATIONS_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=dayclaim_ar;Username=dayclaim_ar;Password=dayclaim_ar_dev_password";
        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}
