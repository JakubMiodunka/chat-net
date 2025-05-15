namespace CommonUtilities.Models.Requests;

/// <summary>
/// Request for sending details about specified pool of users.
/// </summary>
/// <param name="UserIds">
/// Identifiers of users, which details are requested.
/// </param>
public sealed record class GetUsersRequest(int[] UserIds) : Request;
