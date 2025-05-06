namespace CommonUtilities.Models.Requests;

/// <summary>
/// Request for authentication of requester on the server site.
/// </summary>
/// <param name="UserName">
/// The user name as which the requester wants to authenticate.
/// </param>
/// <param name="PasswordHash">
/// Hash of a password associated with specified user name. 
/// </param>
public sealed record AuthenticateRequest(string UserName, string PasswordHash) : Request;
