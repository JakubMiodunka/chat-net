namespace CommonUtilities.Requests.Models;

/// <summary>
/// Respond containing details about result of authentication attempt.
/// </summary>
/// <remarks>
/// Respond to AuthenticateRequest.
/// </remarks>
/// <param name="IsAccessGranted">
/// Indicator if access to server was granted to requester.
/// </param>
public sealed record PutAuthenticationRequest(bool IsAccessGranted) : Request;
