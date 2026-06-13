using Knights.Application.Dashboards;
using Knights.Application.Dashboards.Models;

namespace Knights.Infrastructure.Dashboards;

public sealed class DataSourceCatalog : IDataSourceCatalog
{
    private static readonly IReadOnlyCollection<DataSourceDescriptorResponse> Sources =
    [
        new("users", "Users", "TENANT_USERS_VIEW", [
            Field("userName", "User name", "string", true, true),
            Field("email", "Email", "string", true, true),
            Field("isActive", "Active", "boolean", true, true),
            Field("isEmailConfirmed", "Email confirmed", "boolean", true, true),
            Field("createdAt", "Created at", "date", true, true)
        ]),
        new("roles", "Roles", "TENANT_ROLES_VIEW", [
            Field("name", "Name", "string", true, true),
            Field("isActive", "Active", "boolean", true, true),
            Field("isDefault", "Default", "boolean", true, true),
            Field("isStatic", "Static", "boolean", true, true)
        ]),
        new("permissions", "Permissions", "TENANT_ROLES_VIEW", [
            Field("codeName", "Code", "string", true, true),
            Field("displayName", "Display name", "string", true, true)
        ])
    ];

    public IReadOnlyCollection<DataSourceDescriptorResponse> GetAll() => Sources;

    public DataSourceDescriptorResponse GetRequired(string key) =>
        Sources.FirstOrDefault(source => source.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
        ?? throw new KeyNotFoundException($"Datasource '{key}' is not registered.");

    private static DataSourceFieldResponse Field(string key, string name, string type, bool filterable, bool groupable) =>
        new(key, name, type, filterable, groupable);
}
