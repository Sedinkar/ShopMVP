using Microsoft.EntityFrameworkCore;

namespace ShopMVP.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
}

public class IDesignTimeDbContextFactory<T>
{

}