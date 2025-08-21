using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShopMVP.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
}
/*
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    DbContextOptionsBuilder<AppDbContext> builder = DbContextOption;
}
*/