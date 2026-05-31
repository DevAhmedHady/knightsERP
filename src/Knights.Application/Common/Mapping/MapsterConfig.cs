using Mapster;
using Knights.Application.Permissions.Responses;
using Knights.Application.Roles.Responses;
using Knights.Application.Users.Responses;
using Knights.Domain.Identity;

namespace Knights.Application.Common.Mapping;

public static class MapsterConfig
{
    public static TypeAdapterConfig Register()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.NewConfig<User, UserResponse>()
            .Map(destination => destination.FirstName, source => source.Name.FirstName)
            .Map(destination => destination.MidName, source => source.Name.MidName)
            .Map(destination => destination.LastName, source => source.Name.LastName)
            .Map(destination => destination.RoleIds, source => source.UserRoles.Select(role => role.RoleId).ToArray())
            .Map(destination => destination.PermissionIds, source => source.UserPermissions.Select(permission => permission.PermissionId).ToArray());

        config.NewConfig<Role, RoleResponse>();

        config.NewConfig<Role, RoleWithPermissionsResponse>()
            .Map(destination => destination.PermissionIds, source => source.Permissions.Select(rp => rp.PermissionId).ToArray());

        config.NewConfig<Permission, PermissionResponse>();

        return config;
    }
}
