namespace Knights.Application.Users.Requests;

public sealed record CreateUserRequest(
    string FirstName,
    string MidName,
    string LastName,
    string UserName,
    string Email,
    string? PasswordHash = null,
    bool IsEmailConfirmed = false);
