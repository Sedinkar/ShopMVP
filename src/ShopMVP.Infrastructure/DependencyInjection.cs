using Microsoft.Extensions.DependencyInjection;

namespace ShopMVP.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
        {
            return services;
        }
    }
}