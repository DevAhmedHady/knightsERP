using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Knights.Application.Common.Interfaces;
using Knights.Infrastructure.Persistence;
using Knights.Infrastructure.Persistence.Interceptors;
using Knights.Infrastructure.Persistence.Repositories;
using Knights.Infrastructure.Security;

namespace Knights.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddHttpContextAccessor();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<PersistenceDateTimeOptions>(configuration.GetSection(PersistenceDateTimeOptions.SectionName));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ITenantContext, HttpTenantContext>();

        services.AddDbContext<KnightsDbContext>((serviceProvider, options) =>
            options
                .UseNpgsql(configuration.GetConnectionString("KnightsDb"))
                .AddInterceptors(serviceProvider.GetRequiredService<AuditableEntityInterceptor>()));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();

        return services;
    }
}
