namespace CommonUtilities.Requests.Models;

/// <summary>
/// Request for authentication of requester on the server site.
/// </summary>
/// <param name="UserId">
/// Identifier of user as which the requester wants to authenticate.
/// </param>
/// <param name="PasswordHash">
/// Hash of a password associated with specified user name. 
/// </param>
public sealed record GetAuthenticationRequest(int UserId, string PasswordHash) : Request;
