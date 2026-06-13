namespace Knights.Application.Common.Interfaces;

public interface IUserPermissionChecker
{
    Task<bool> HasPermissionAsync(string permissionCode, CancellationToken cancellationToken = default);
}
