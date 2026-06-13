using System.Linq.Expressions;
using Knights.Application.Common.Interfaces;
using Knights.Application.Dashboards;
using Knights.Application.Dashboards.Models;
using Knights.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Knights.Infrastructure.Dashboards;

public sealed class WidgetQueryExecutor(
    KnightsDbContext dbContext,
    IDataSourceCatalog catalog,
    IUserPermissionChecker permissionChecker,
    ITenantContext tenantContext) : IWidgetQueryExecutor
{
    public async Task<WidgetDataResponse> ExecuteAsync(
        string dataSourceKey,
        WidgetQuerySpec query,
        CancellationToken cancellationToken = default)
    {
        var source = catalog.GetRequired(dataSourceKey);
        if (!await permissionChecker.HasPermissionAsync(source.RequiredPermission, cancellationToken))
            throw new UnauthorizedAccessException($"Permission '{source.RequiredPermission}' is required for datasource '{source.Key}'.");

        return source.Key.ToLowerInvariant() switch
        {
            "users" => await ExecuteUsersAsync(query, cancellationToken),
            "roles" => await ExecuteRolesAsync(query, cancellationToken),
            "permissions" => await ExecutePermissionsAsync(query, cancellationToken),
            _ => throw new KeyNotFoundException($"Datasource '{dataSourceKey}' is not registered.")
        };
    }

    private async Task<WidgetDataResponse> ExecuteUsersAsync(WidgetQuerySpec query, CancellationToken cancellationToken)
    {
        var users = dbContext.Users.AsNoTracking()
            .Where(user => !tenantContext.TenantId.HasValue || user.TenantId == tenantContext.TenantId)
            .Select(user => new UserRow(user.UserName, user.Email, user.IsActive, user.IsEmailConfirmed, user.CreatedAt));

        foreach (var filter in query.Filters ?? [])
            users = ApplyUserFilter(users, filter);

        if (query.Aggregation is not null)
            return await AggregateUsersAsync(users, query, cancellationToken);

        users = SortUsers(users, query).Take(query.Limit);
        var rows = await users.ToListAsync(cancellationToken);
        return Project(query.Fields, rows, UserValue);
    }

    private async Task<WidgetDataResponse> ExecuteRolesAsync(WidgetQuerySpec query, CancellationToken cancellationToken)
    {
        var roleIds = tenantContext.TenantId.HasValue
            ? dbContext.TenantRoles.Where(link => link.TenantId == tenantContext.TenantId).Select(link => link.RoleId)
            : dbContext.Roles.Select(role => role.Id);
        var roles = dbContext.Roles.AsNoTracking()
            .Where(role => !role.IsDeleted && roleIds.Contains(role.Id))
            .Select(role => new RoleRow(role.Name, role.IsActive, role.IsDefault, role.IsStatic));

        foreach (var filter in query.Filters ?? [])
            roles = ApplyRoleFilter(roles, filter);

        if (query.Aggregation is not null)
            return await AggregateRolesAsync(roles, query, cancellationToken);

        roles = SortRoles(roles, query).Take(query.Limit);
        var rows = await roles.ToListAsync(cancellationToken);
        return Project(query.Fields, rows, RoleValue);
    }

    private async Task<WidgetDataResponse> ExecutePermissionsAsync(WidgetQuerySpec query, CancellationToken cancellationToken)
    {
        var permissionIds = tenantContext.TenantId.HasValue
            ? dbContext.TenantPermissions.Where(link => link.TenantId == tenantContext.TenantId).Select(link => link.PermissionId)
            : dbContext.Permissions.Select(permission => permission.Id);
        var permissions = dbContext.Permissions.AsNoTracking()
            .Where(permission => !permission.IsDeleted && permissionIds.Contains(permission.Id))
            .Select(permission => new PermissionRow(permission.CodeName, permission.DisplayName));

        foreach (var filter in query.Filters ?? [])
            permissions = ApplyPermissionFilter(permissions, filter);

        if (query.Aggregation is not null)
            return await AggregatePermissionsAsync(permissions, query, cancellationToken);

        permissions = SortPermissions(permissions, query).Take(query.Limit);
        var rows = await permissions.ToListAsync(cancellationToken);
        return Project(query.Fields, rows, PermissionValue);
    }

    private static IQueryable<UserRow> ApplyUserFilter(IQueryable<UserRow> rows, WidgetFilter filter) => filter.Field switch
    {
        "userName" => ApplyFilter(rows, filter, row => row.UserName),
        "email" => ApplyFilter(rows, filter, row => row.Email),
        "isActive" => ApplyFilter(rows, filter, row => row.IsActive),
        "isEmailConfirmed" => ApplyFilter(rows, filter, row => row.IsEmailConfirmed),
        "createdAt" => ApplyFilter(rows, filter, row => row.CreatedAt),
        _ => throw InvalidField(filter.Field)
    };

    private static IQueryable<RoleRow> ApplyRoleFilter(IQueryable<RoleRow> rows, WidgetFilter filter) => filter.Field switch
    {
        "name" => ApplyFilter(rows, filter, row => row.Name),
        "isActive" => ApplyFilter(rows, filter, row => row.IsActive),
        "isDefault" => ApplyFilter(rows, filter, row => row.IsDefault),
        "isStatic" => ApplyFilter(rows, filter, row => row.IsStatic),
        _ => throw InvalidField(filter.Field)
    };

    private static IQueryable<PermissionRow> ApplyPermissionFilter(IQueryable<PermissionRow> rows, WidgetFilter filter) => filter.Field switch
    {
        "codeName" => ApplyFilter(rows, filter, row => row.CodeName),
        "displayName" => ApplyFilter(rows, filter, row => row.DisplayName),
        _ => throw InvalidField(filter.Field)
    };

    private static IQueryable<T> ApplyFilter<T, TValue>(
        IQueryable<T> rows,
        WidgetFilter filter,
        Expression<Func<T, TValue>> field)
    {
        var body = BuildFilterExpression(field.Body, filter, typeof(TValue));
        return rows.Where(Expression.Lambda<Func<T, bool>>(body, field.Parameters));
    }

    private static Expression BuildFilterExpression(Expression field, WidgetFilter filter, Type fieldType)
    {
        if (fieldType == typeof(string))
            return BuildStringFilter(field, filter);

        if (!filter.Operator.Equals("equals", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Filter operator '{filter.Operator}' is not supported for field '{filter.Field}'.");

        var parsedValue = ParseFilterValue(filter, fieldType);
        return Expression.Equal(field, Expression.Constant(parsedValue, fieldType));
    }

    private static object ParseFilterValue(WidgetFilter filter, Type fieldType)
    {
        if (fieldType == typeof(bool) && bool.TryParse(filter.Value, out var booleanValue))
            return booleanValue;
        if (fieldType == typeof(DateTime) && DateTime.TryParse(filter.Value, System.Globalization.CultureInfo.InvariantCulture, out var dateValue))
            return dateValue;

        throw new ArgumentException($"Filter value '{filter.Value}' is invalid for field '{filter.Field}'.");
    }

    private static Expression BuildStringFilter(Expression field, WidgetFilter filter)
    {
        var toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
        var normalizedField = Expression.Call(field, toLower);
        var normalizedValue = Expression.Constant(filter.Value.ToLowerInvariant());
        return filter.Operator.ToLowerInvariant() switch
        {
            "equals" => Expression.Equal(normalizedField, normalizedValue),
            "contains" => Expression.Call(normalizedField, nameof(string.Contains), Type.EmptyTypes, normalizedValue),
            "startswith" => Expression.Call(normalizedField, nameof(string.StartsWith), Type.EmptyTypes, normalizedValue),
            _ => throw new ArgumentException($"Filter operator '{filter.Operator}' is not supported.")
        };
    }

    private static IQueryable<UserRow> SortUsers(IQueryable<UserRow> rows, WidgetQuerySpec query) => query.SortBy switch
    {
        "userName" => Order(rows, row => row.UserName, query.SortDescending),
        "email" => Order(rows, row => row.Email, query.SortDescending),
        "isActive" => Order(rows, row => row.IsActive, query.SortDescending),
        "isEmailConfirmed" => Order(rows, row => row.IsEmailConfirmed, query.SortDescending),
        "createdAt" => Order(rows, row => row.CreatedAt, query.SortDescending),
        null or "" => rows,
        _ => throw InvalidField(query.SortBy)
    };

    private static IQueryable<RoleRow> SortRoles(IQueryable<RoleRow> rows, WidgetQuerySpec query) => query.SortBy switch
    {
        "name" => Order(rows, row => row.Name, query.SortDescending),
        "isActive" => Order(rows, row => row.IsActive, query.SortDescending),
        "isDefault" => Order(rows, row => row.IsDefault, query.SortDescending),
        "isStatic" => Order(rows, row => row.IsStatic, query.SortDescending),
        null or "" => rows,
        _ => throw InvalidField(query.SortBy)
    };

    private static IQueryable<PermissionRow> SortPermissions(IQueryable<PermissionRow> rows, WidgetQuerySpec query) => query.SortBy switch
    {
        "codeName" => Order(rows, row => row.CodeName, query.SortDescending),
        "displayName" => Order(rows, row => row.DisplayName, query.SortDescending),
        null or "" => rows,
        _ => throw InvalidField(query.SortBy)
    };

    private static IOrderedQueryable<T> Order<T, TKey>(IQueryable<T> rows, Expression<Func<T, TKey>> key, bool descending) =>
        descending ? rows.OrderByDescending(key) : rows.OrderBy(key);

    private static Task<WidgetDataResponse> AggregateUsersAsync(IQueryable<UserRow> rows, WidgetQuerySpec query, CancellationToken cancellationToken) =>
        query.GroupBy switch
        {
            "userName" => AggregateAsync(rows, query, row => row.UserName, cancellationToken),
            "email" => AggregateAsync(rows, query, row => row.Email, cancellationToken),
            "isActive" => AggregateAsync(rows, query, row => row.IsActive, cancellationToken),
            "isEmailConfirmed" => AggregateAsync(rows, query, row => row.IsEmailConfirmed, cancellationToken),
            "createdAt" => AggregateAsync(rows, query, row => row.CreatedAt, cancellationToken),
            _ => AggregateTotalAsync(rows, query, cancellationToken)
        };

    private static Task<WidgetDataResponse> AggregateRolesAsync(IQueryable<RoleRow> rows, WidgetQuerySpec query, CancellationToken cancellationToken) =>
        query.GroupBy switch
        {
            "name" => AggregateAsync(rows, query, row => row.Name, cancellationToken),
            "isActive" => AggregateAsync(rows, query, row => row.IsActive, cancellationToken),
            "isDefault" => AggregateAsync(rows, query, row => row.IsDefault, cancellationToken),
            "isStatic" => AggregateAsync(rows, query, row => row.IsStatic, cancellationToken),
            _ => AggregateTotalAsync(rows, query, cancellationToken)
        };

    private static Task<WidgetDataResponse> AggregatePermissionsAsync(IQueryable<PermissionRow> rows, WidgetQuerySpec query, CancellationToken cancellationToken) =>
        query.GroupBy switch
        {
            "codeName" => AggregateAsync(rows, query, row => row.CodeName, cancellationToken),
            "displayName" => AggregateAsync(rows, query, row => row.DisplayName, cancellationToken),
            _ => AggregateTotalAsync(rows, query, cancellationToken)
        };

    private static async Task<WidgetDataResponse> AggregateAsync<T, TKey>(
        IQueryable<T> rows,
        WidgetQuerySpec query,
        Expression<Func<T, TKey>> groupBy,
        CancellationToken cancellationToken)
    {
        var grouped = rows.GroupBy(groupBy).Select(group => new GroupCount<TKey>(group.Key, group.Count()));
        grouped = query.SortBy == query.Aggregation!.Alias
            ? Order(grouped, group => group.Count, query.SortDescending)
            : Order(grouped, group => group.Key, query.SortDescending);
        var values = await grouped.Take(query.Limit).ToListAsync(cancellationToken);
        var groupColumn = query.GroupBy!;
        return new WidgetDataResponse(
            [groupColumn, query.Aggregation.Alias],
            values.Select(value => Row((groupColumn, value.Key), (query.Aggregation.Alias, value.Count))).ToArray());
    }

    private static async Task<WidgetDataResponse> AggregateTotalAsync<T>(
        IQueryable<T> rows,
        WidgetQuerySpec query,
        CancellationToken cancellationToken)
    {
        var count = await rows.CountAsync(cancellationToken);
        return new WidgetDataResponse(
            ["label", query.Aggregation!.Alias],
            [Row(("label", "Total"), (query.Aggregation.Alias, count))]);
    }

    private static WidgetDataResponse Project<T>(
        IReadOnlyCollection<string> fields,
        IReadOnlyCollection<T> rows,
        Func<T, string, object?> valueSelector) =>
        new(fields, rows.Select(row => Row(fields.Select(field => (field, valueSelector(row, field))).ToArray())).ToArray());

    private static object? UserValue(UserRow row, string field) => field switch
    {
        "userName" => row.UserName,
        "email" => row.Email,
        "isActive" => row.IsActive,
        "isEmailConfirmed" => row.IsEmailConfirmed,
        "createdAt" => row.CreatedAt,
        _ => throw InvalidField(field)
    };

    private static object? RoleValue(RoleRow row, string field) => field switch
    {
        "name" => row.Name,
        "isActive" => row.IsActive,
        "isDefault" => row.IsDefault,
        "isStatic" => row.IsStatic,
        _ => throw InvalidField(field)
    };

    private static object? PermissionValue(PermissionRow row, string field) => field switch
    {
        "codeName" => row.CodeName,
        "displayName" => row.DisplayName,
        _ => throw InvalidField(field)
    };

    private static IReadOnlyDictionary<string, object?> Row(params (string Key, object? Value)[] values) =>
        values.ToDictionary(value => value.Key, value => value.Value);

    private static ArgumentException InvalidField(string field) => new($"Field '{field}' is not supported.");

    private sealed record UserRow(string UserName, string Email, bool IsActive, bool IsEmailConfirmed, DateTime CreatedAt);
    private sealed record RoleRow(string Name, bool IsActive, bool IsDefault, bool IsStatic);
    private sealed record PermissionRow(string CodeName, string DisplayName);
    private sealed record GroupCount<TKey>(TKey Key, int Count);
}
