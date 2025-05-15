namespace CommonUtilities.Models.Requests;

/// <summary>
/// Respond containing details about pool of users.
/// </summary>
/// <param name="Users">
/// Users, which shall be included in request content.
/// </param>
public sealed record PutUsersRequest(User[] Users) : Request;
