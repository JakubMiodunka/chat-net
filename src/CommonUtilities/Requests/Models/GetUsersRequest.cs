namespace CommonUtilities.Requests.Models;

/// <summary>
/// Request used by client to ask server about details related to specified pool of users.
/// </summary>
/// <param name="UserIdentifiers">
/// Identifiers of users, which details are requested.
/// </param>
public sealed record class GetUsersRequest(int[] UserIdentifiers) : Request;
