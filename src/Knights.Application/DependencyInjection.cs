using Microsoft.Extensions.DependencyInjection;
using Knights.Application.Auth;
using Knights.Application.Common.Mapping;
using Knights.Application.Permissions;
using Knights.Application.Roles;
using Knights.Application.Tenants;
using Knights.Application.Users;

namespace Knights.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        MapsterConfig.Register();

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<IPermissionService, PermissionService>();

        services.AddScoped<ITenantService, TenantService>();

        return services;
    }
}
