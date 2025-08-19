using Microsoft.EntityFrameworkCore;

namespace ShopMVP.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
}