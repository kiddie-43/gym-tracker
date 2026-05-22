using GymTracker.Infrastructure.Admin;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GymTracker.Api;

public sealed class AdminDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
{
    public AdminDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("GYMTRACKER_SQL_CONNECTION")
            ?? "Server=localhost,14333;Database=GymTracker;User Id=sa;Password=GymTracker_SaP4ss!;TrustServerCertificate=True;Encrypt=False";

        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AdminDbContext(options);
    }
}
