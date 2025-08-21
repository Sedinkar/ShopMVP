using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ShopMVP.Infrastructure.Persistence;

namespace ShopMVP.Infrastructure.DependencyInjection

{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.ParseExact("00:00:10","c",null), errorNumbersToAdd: null)));
            
            return services;
        }
    }
}