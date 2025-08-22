using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShopMVP.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
}

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        if (String.IsNullOrEmpty(connectionString))
        {
            connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ShopMvpDev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        }
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
        return new AppDbContext(optionsBuilder.Options);
    }
}
