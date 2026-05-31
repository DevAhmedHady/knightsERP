namespace Knights.Application.Users.Requests;

public sealed record UpdateUserRequest(
    string FirstName,
    string MidName,
    string LastName,
    string UserName,
    string Email,
    bool IsEmailConfirmed);
