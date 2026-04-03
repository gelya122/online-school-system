using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OnlineSchoolAPI;

/// <summary>
/// Позволяет выполнять команды dotnet ef (миграции) с корректной строкой подключения из appsettings.
/// </summary>
public class OnlineSchoolDbContextFactory : IDesignTimeDbContextFactory<OnlineSchoolDbContext>
{
    public OnlineSchoolDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("OnlineSchoolConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=online_school_db;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<OnlineSchoolDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new OnlineSchoolDbContext(optionsBuilder.Options);
    }
}
