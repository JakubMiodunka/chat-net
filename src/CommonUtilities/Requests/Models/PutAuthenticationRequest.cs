namespace CommonUtilities.Requests.Models;

/// <summary>
/// Server respond containing result of client authentication attempt.
/// </summary>
/// <remarks>
/// Respond to AuthenticateRequest.
/// </remarks>
/// <param name="IsAccessGranted">
/// Indicator if access to server was granted to requester.
/// </param>
public sealed record PutAuthenticationRequest(bool IsAccessGranted) : Request;
