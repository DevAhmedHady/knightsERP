using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Knights.Application.Common.Interfaces;
using Knights.Infrastructure.Persistence;
using Knights.Infrastructure.Persistence.Interceptors;
using Knights.Infrastructure.Persistence.Repositories;

namespace Knights.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddDbContext<KnightsDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(configuration.GetConnectionString("KnightsDb"))
                .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>()));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        return services;
    }
}
